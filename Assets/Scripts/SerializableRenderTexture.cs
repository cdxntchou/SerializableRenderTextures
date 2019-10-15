using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;            // NativeArray
using UnityEngine;
using UnityEngine.Experimental.Rendering;


namespace Environment
{

    // lifecycles:

    // (Create)      --> data stored in GPU RenderTexture
    // (Read)        --> read from GPU RenderTexture
    // (Modify)      --> modify GPU RenderTexture
    // (Serialize)   --> GPU RenderTexture -> Temp Local CPU-only Texture2D -> ReadRawData -> bytes on disk

    // (Deserialize) --> temp (CPU + GPU) Texture2D --> blit to GPU RenderTexture, destroy temp
    // (Read)        --> read from GPU RenderTexture
    // (Modify)      --> modify GPU RenderTexture
    // (Serialize)   --> GPU RenderTexture -> Temp Local CPU-only Texture2D -> ReadRawData -> bytes on disk

    // ideal:   IF we had a GPU-only Texture2D (we don't, not from user land at least)
    //            we could keep data there until it is requested to be modified (and then upgrade it to a GPU RenderTexture)
    //            this is a slight optimization, because RenderTextures are typically slightly more expensive than Textures...

    [Serializable]
    public class SerializableRenderTexture : ISerializationCallbackReceiver
    {
        // format data -- set by constructor, never modified
        #region Serialized Data
        [SerializeField]
        private int width;

        [SerializeField]
        private int height;

        [SerializeField]
        private string format;

        [SerializeField]
        private bool mipChain;

        [SerializeField]
        private byte[] data;

//        [SerializeField]
//        private Color defaultColor;       // TODO
        #endregion

        #region Runtime Only Data
        private GraphicsFormat graphicsFormat;
        private RenderTexture renderTexture;
        private State state;
        #endregion

        private enum State
        {
            _Uninitialized,     // format data not declared (width/height/format)
            _Unallocated,       // format data declared, but nothing allocated
            _Deserialized,      // valid, but has just been deserialized (requires unpacking)
            _Valid,             // m_RenderTexture is allocated and holds the authoritative data
        };

        public SerializableRenderTexture(int width, int height, GraphicsFormat format, bool mipChain = false)
        {
            this.width = width;
            this.height = height;
            this.graphicsFormat = format;
            this.mipChain = mipChain;
            renderTexture = null;
            state = State._Unallocated;
        }

        public Texture GetTexture()     // for GPU read
        {
            switch (state)
            {
                case State._Uninitialized:
                    return null;
                case State._Unallocated:
                    CreateRenderTexture(true);
                    return renderTexture;
                case State._Deserialized:
                    if (data != null && (data.Length > 0))
                    {
                        Enum.TryParse<GraphicsFormat>(format, out graphicsFormat);
                        DeserializeBytes(data);
                        data = null;
                        state = State._Valid;
                    }
                    return renderTexture;
                case State._Valid:
                    return renderTexture;
                default:
                    Debug.LogError("SerializableRenderTexture: Invalid state in GetTexture");
                    return null;
            }
        }

        public RenderTexture GetRenderTexture()     // for GPU write
        {
            switch (state)
            {
                case State._Uninitialized:
                    return null;
                case State._Unallocated:
                    CreateRenderTexture(true);
                    return renderTexture;
                case State._Deserialized:
                    if (data != null && (data.Length > 0))
                    {
                        Enum.TryParse<GraphicsFormat>(format, out graphicsFormat);
                        DeserializeBytes(data);
                        data = null;
                        state = State._Valid;
                    }
                    return renderTexture;
                case State._Valid:
                    return renderTexture;
                default:
                    Debug.LogError("SerializableRenderTexture: Invalid state in GetRenderTexture");
                    return null;
            }
        }

        //     interface ISerializer
        //     {
        //         void SerializeBytes(NativeArray<byte> data);
        //         NativeArray<byte> DeserializeBytes();
        //     }

        public void OnBeforeSerialize()
        {
            if (state == State._Valid)
            {
                format = graphicsFormat.ToString("g");
                data = SerializeBytes();
            }
        }

        public void OnAfterDeserialize()
        {
            state = State._Deserialized;
        }

        private void DeserializeBytes(byte[] data)
        {
            if (renderTexture == null)
                CreateRenderTexture(false);

            Texture2D CPUGPUTexture = CreateTexture();

            CPUGPUTexture.LoadRawTextureData(data);
            CPUGPUTexture.Apply();     // upload to GPU (so we can copy to render texture)

            RenderTexture old = RenderTexture.active;
            Graphics.Blit(CPUGPUTexture, renderTexture);
            RenderTexture.active = old;

            DestroyTexture(CPUGPUTexture);

            state = State._Valid;
        }

        private byte[] SerializeBytes()
        {
            if ((state != State._Valid) || (renderTexture == null))
            {
                Debug.LogError("SerializableRenderTexture: Serialize called in invalid state");
                return null;
            }

            // gpu copy back -- doesn't update cpu copy of the data...
            // could only do this if we do a roundabout GPU Texture -> GPU RenderTarget -> CPU Texture readback before serializing....
            // BUT -- given we only ever want to call this function for serialization -- no point in bothering to update the GPU copy at all, really..
            //        Graphics.CopyTexture(sourceRT, indexMap);

            Texture2D CPUTexture = CreateTexture();

            // cpu copy back
            RenderTexture old = RenderTexture.active;
            RenderTexture.active = renderTexture;
            CPUTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            // CPUTexture.Apply();        // this updates GPU copy -- not needed, we are throwing this texture away immediately
            RenderTexture.active = old;

            byte[] data = CPUTexture.GetRawTextureData();
            //        NativeArray<byte> data = CPUTexture.GetRawTextureData<byte>();
            //        serializer.SerializeBytes(data);

            DestroyTexture(CPUTexture);
            CPUTexture = null;

            return data;
        }

        private Texture2D CreateTexture()
        {
            Texture2D texture = null;

            // TODO: not sure if this allocates the GPU copy of the texture, or if that is deferred until we call Apply or something...
            // we are not interested in the GPU side, only the CPU
            texture = new Texture2D(width, height, graphicsFormat, mipChain ? TextureCreationFlags.MipChain : TextureCreationFlags.None);
            return texture;
        }

        private void DestroyTexture(Texture2D temp)
        {
            // I think this is what we use to throw away a Texture2D ?
            UnityEngine.Object.DestroyImmediate(temp);
        }

        private RenderTexture CreateRenderTexture(bool setToDefault)
        {
            if (renderTexture != null)
            {
                Debug.LogError("SerializableRenderTexture: Attempting to allocate render texture this is already allocated");
                return renderTexture;
            }

            // TODO: more control
            //                 //                renderTexture = new RenderTexture(64, 64, 1, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm);
            //                 RenderTextureDescriptor desc = new RenderTextureDescriptor(64, 64, RenderTextureFormat.ARGB32, 0);
            //                 desc.sRGB = false;
            //                 desc.useMipMap = false;
            //                 desc.msaaSamples = 1;
            //                 renderTexture = new RenderTexture(desc);
            //                 renderTexture.anisoLevel = 1;
            //                 renderTexture.name = "indexMapRT";
            //                 renderTexture.filterMode = FilterMode.Point;
            //                 renderTexture.wrapMode = TextureWrapMode.Clamp;

            renderTexture = new RenderTexture(width, height, 1, graphicsFormat);

            if (setToDefault)
                SetRenderTextureToDefault();

            state = State._Valid;

            return renderTexture;
        }

        private void SetRenderTextureToDefault()
        {
            // TODO: 
        }
    }

} // namespace UnityEngine.Experimental.Environment
