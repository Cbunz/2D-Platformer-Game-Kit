using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedDisabling : MonoBehaviour
{
    public float timeBeforeDisable = 1.0f;

    private float timer = 0.0f;

    private void OnEnable()
    {
        timer = timeBeforeDisable;
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer < 0.0f)
            gameObject.SetActive(false);
    }
}
