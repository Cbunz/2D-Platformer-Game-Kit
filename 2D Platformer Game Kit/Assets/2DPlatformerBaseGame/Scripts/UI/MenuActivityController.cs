using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuActivityController : MonoBehaviour {

    Canvas[] canvases = new Canvas[0];
    GraphicRaycaster[] raycasters = new GraphicRaycaster[0];

    private void Awake()
    {
        canvases = GetComponentsInChildren<Canvas>(true);
        raycasters = GetComponentsInChildren<GraphicRaycaster>(true);
    }

    public void SetActive(bool active)
    {
        for (int i = 0; i < canvases.Length; i++)
        {
            canvases[i].enabled = active;
        }

        for (int i = 0; i < raycasters.Length; i++)
        {
            raycasters[i].enabled = active;
        }
    }
}
