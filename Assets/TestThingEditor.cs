using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Environment;


[CanEditMultipleObjects]
[CustomEditor(typeof(TestThing))]
public class TestThingEditor : UnityEditor.Editor
{
    public void OnEnable()
    {
    }

    public override void OnInspectorGUI()
    {
        TestThing test = target as TestThing;
        if (test == null)
            return;
        var textureSet = test.textureSet;

        if (GUILayout.Button("Red"))
        {
            int index = textureSet.CreateTexture(128, 128);
            var rta = textureSet.GetRenderTextureAsset(index);
            RenderTexture rt = rta.GetRenderTexture();
            RenderTexture old = RenderTexture.active;
            RenderTexture.active = rt;
            GL.Clear(false, true, new Color(1.0f, 0.0f, 0.0f, 1.0f));
            RenderTexture.active = old;
            EditorUtility.SetDirty(rta);
            Debug.Log("Created red texture " + index);
        }

        if (GUILayout.Button("Green"))
        {
            int index = textureSet.CreateTexture(128, 128);
            var rta = textureSet.GetRenderTextureAsset(index);
            RenderTexture rt = rta.GetRenderTexture();
            RenderTexture old = RenderTexture.active;
            RenderTexture.active = rt;
            GL.Clear(false, true, new Color(0.0f, 1.0f, 0.0f, 1.0f));
            RenderTexture.active = old;
            EditorUtility.SetDirty(rta);
            Debug.Log("Created green texture " + index);
        }

        if (GUILayout.Button("Blue"))
        {
            int index = textureSet.CreateTexture(128, 128);
            var rta = textureSet.GetRenderTextureAsset(index);
            RenderTexture rt = rta.GetRenderTexture();
            RenderTexture old = RenderTexture.active;
            RenderTexture.active = rt;
            GL.Clear(false, true, new Color(0.0f, 0.0f, 1.0f, 1.0f));
            RenderTexture.active = old;
            EditorUtility.SetDirty(rta);
            Debug.Log("Created blue texture " + index);
        }

        EditorGUILayout.TextField("Textures: " + textureSet.textureCount);

        for (int x = 0; x < textureSet.textureCount; x++)
        {
            GUILayout.Label(textureSet.GetRenderTexture(x));
        }
    }
}

