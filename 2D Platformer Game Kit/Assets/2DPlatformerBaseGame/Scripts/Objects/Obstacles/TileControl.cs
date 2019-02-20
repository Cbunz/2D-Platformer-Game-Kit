using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Allows the player to change the size of tiled objects by changing the size of the renderer, collider, and damager if applicable. Uses custom editor to create box with grabbable handles.
/// </summary>

[ExecuteInEditMode]
public class TileControl : MonoBehaviour {

    public Vector2 dimensions = new Vector2(2, 1);
    public Vector2 center = new Vector2(0, 0f);
    public bool roundDimensionsToInt = false;
    public bool customColliderHeight = true;

    private SpriteRenderer spriteRenderer;
    private new BoxCollider2D collider;
    private Damager damager;


	private void Awake ()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider = GetComponent<BoxCollider2D>();
        damager = GetComponent<Damager>();
	}
	
	private void Update ()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.size = dimensions;
        }
        if (collider != null)
        {
            if (customColliderHeight)
                collider.size = new Vector2(dimensions.x, collider.size.y);
            else
                collider.size =  new Vector2(dimensions.x, dimensions.y);
        }
        if (damager != null)
        {
            damager.size = dimensions;
        }

        //transform.position = (Vector2)transform.position + center;
	}
}
