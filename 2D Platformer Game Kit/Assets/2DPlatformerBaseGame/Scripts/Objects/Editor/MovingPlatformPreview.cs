using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MovingPlatformPreview
{
    static public MovingPlatformPreview Preview = null;
    static public GameObject preview;

    static protected MovingPlatform movingPlatform;

    static MovingPlatformPreview()
    {
        Selection.selectionChanged += SelectionChanged;
    }

    static void SelectionChanged()
    {
        if (movingPlatform != null && Selection.activeGameObject != movingPlatform.gameObject)
        {
            DestroyPreview();
        }
    }

    static public void DestroyPreview()
    {
        if (preview == null)
        {
            return;
        }

        Object.DestroyImmediate(preview);
        preview = null;
        movingPlatform = null;
    }

    static public void CreateNewPreview(MovingPlatform origin)
    {
        if (preview != null)
        {
            Object.DestroyImmediate(preview);
        }

        movingPlatform = origin;

        preview = Object.Instantiate(origin.gameObject);
        preview.hideFlags = HideFlags.DontSave;
        MovingPlatform plat = preview.GetComponentInChildren<MovingPlatform>();
        Object.DestroyImmediate(plat);

        Color c = new Color(0.2f, 0.2f, 0.2f, 0.4f);
        SpriteRenderer[] renderers = preview.GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < renderers.Length; ++i)
        {
            renderers[i].color = c;
        }
    }
}
