using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetMaterialRenderQueue : MonoBehaviour {

    public Material material;
    public int queueOverrideValue;

    private void Start()
    {
        material.renderQueue = queueOverrideValue;
    }
}
