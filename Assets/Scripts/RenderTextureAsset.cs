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

    [CreateAssetMenu(fileName = "RenderTexture", menuName = "Environment/RenderTexture", order = 1)]
    public class RenderTextureAsset : ScriptableObject
    {
        [SerializeField]
        SerializableRenderTexture srt;

        public void InitializeOnCreation(int width, int height, GraphicsFormat format, bool mipChain = false)
        {
            srt = new SerializableRenderTexture(width, height, format, mipChain);
        }

        public Texture GetTexture()
        {
            return srt?.GetTexture();
        }

        public RenderTexture GetRenderTexture()
        {
            return srt?.GetRenderTexture();
        }
    }
} // namespace UnityEngine.Experimental.Environment
