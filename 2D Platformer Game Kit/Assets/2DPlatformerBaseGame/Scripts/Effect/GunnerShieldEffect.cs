using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunnerShieldEffect : MonoBehaviour
{
    protected Material material;
    protected float intensity = 0.0f;

    const int count = 2;

    private void Start()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        material = renderer.material;
        intensity = 0.0f;
    }

    public void ShieldHit(Damager damager, Damageable damageable)
    {
        Vector3 localPosition = transform.InverseTransformPoint(damager.transform.position);

        material.SetVector("HitPosition", localPosition);
        intensity = 1.0f;
    }

    public void Update()
    {
        if (intensity > 0.0f)
            intensity = Mathf.Clamp(intensity - Time.deltaTime, 0, 1);

        material.SetFloat("HitIntensity", intensity);
    }
}
