using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteColorChange : MonoBehaviour
{
    public Color newColor = Color.white;
    public float timer = 0.2f;

    Color initialColor;
    SpriteRenderer spriteRenderer;

    private void OnEnable()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        initialColor = spriteRenderer.color;
        StartCoroutine(ChangeColor());
    }

    IEnumerator ChangeColor()
    {
        spriteRenderer.color = newColor;
        yield return new WaitForSeconds(timer);
        spriteRenderer.color = initialColor;
        this.enabled = false;
    }
}
