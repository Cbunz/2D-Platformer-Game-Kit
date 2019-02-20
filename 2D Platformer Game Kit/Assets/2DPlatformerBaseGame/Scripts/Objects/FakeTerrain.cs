using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeTerrain : MonoBehaviour {

    public Transform ground;
    public Transform wall;
    public string triggerTag;


    private void OnEnable()
    {
        EventManager.StartListening("Vanish", Vanish);
    }

    private void OnDisable()
    {
        EventManager.StopListening("Vanish", Vanish);
    }

    public void Vanish(string triggerTag)
    {
        Debug.Log("Vanish Called");
        Destroy(ground.gameObject);
        Destroy(wall.gameObject);
    }
}
