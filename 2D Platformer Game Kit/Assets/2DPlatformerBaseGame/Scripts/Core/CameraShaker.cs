using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShaker : MonoBehaviour {

    static protected CameraShaker instance = null;

    protected Vector3 lastVector;
    protected float sinceShakeTime = 0.0f;
    protected float shakeIntensity = 0.2f;

    private void OnEnable()
    {
        instance = this;
    }

    private void OnPreRender()
    {
        if (sinceShakeTime > 0.0f)
        {
            lastVector = Random.insideUnitCircle * shakeIntensity;
            transform.localPosition = transform.localPosition + lastVector;
        }
    }

    private void OnPostRender()
    {
        if (sinceShakeTime > 0.0f)
        {
            transform.localPosition = transform.localPosition - lastVector;
            sinceShakeTime -= Time.deltaTime;
        }
    }

    static public void Shake(float amount, float time)
    {
        if (instance == null)
            return;

        instance.shakeIntensity = amount;
        instance.sinceShakeTime = time;
    }
}
