using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace Environment
{
    [Serializable]
	public class LayerTextureSet
    {
        [SerializeField]
        List<RenderTextureAsset> textures;

        public int textureCount { get { return textures.Count; } }

        public RenderTextureAsset GetRenderTextureAsset(int index)
        {
            return textures[index];
        }
        public RenderTexture GetRenderTexture(int index)
        {
            return textures[index].GetRenderTexture();
        }

        public int CreateTexture(int width, int height)
        {
            if (textures == null)
                textures = new List<RenderTextureAsset>();

            string assetPath = AssetDatabase.GenerateUniqueAssetPath("Assets\\Textures\\Blah");
            ScriptableObject scriptableObject = ScriptableObject.CreateInstance(typeof(RenderTextureAsset));
            RenderTextureAsset rta = scriptableObject as RenderTextureAsset;
            if (rta != null)
            {
                rta.InitializeOnCreation(width, height, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm);
                rta.GetTexture();
            }
            AssetDatabase.CreateAsset(rta, assetPath);
            EditorUtility.SetDirty(rta);

            int index = textures.Count;
            textures.Add(rta);
            return index;
        }

//         AssetDatabase.AddObjectToAsset(Object objectToAdd, Object assetObject);
//         AssetDatabase.AssetPathToGUID(string path)
//         bool AssetDatabase.TryGetGUIDAndLocalFileIdentifier(UnityEngine.Object obj, out string guid, out int localId);
    }
}


