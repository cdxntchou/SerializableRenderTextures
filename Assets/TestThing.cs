using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Environment;


[ExecuteInEditMode]
public class TestThing : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    LayerTextureSet textures;

    [SerializeField]
    bool doit;

    void Start()
    {
        if (textures == null)
            textures = new LayerTextureSet();
    }

    // Update is called once per frame
    void Update()
    {
        if (doit)
        {
            int index = textures.CreateTexture(128, 128);
            Debug.Log("Created texture " + index);
            doit = false;
        }
    }

    public LayerTextureSet textureSet { get { return textures; } }
}

