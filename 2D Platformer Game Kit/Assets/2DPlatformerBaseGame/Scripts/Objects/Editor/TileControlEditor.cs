using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

/// <summary>
/// Editor for the TileControl that creates a box with grabbable handles to dynamically adjust the size.
/// </summary>

[CustomEditor(typeof(TileControl))]
public class TileControlEditor : Editor
{
    static BoxBoundsHandle boxBoundsHandle = new BoxBoundsHandle();
    static Color enabledColor = Color.green + Color.grey;

    SerializedProperty dimensionsProperty;
    SerializedProperty centerProperty;
    SerializedProperty roundDimensionsToInt;
    SerializedProperty customColliderHeightProperty;

    private void OnEnable()
    {
        dimensionsProperty = serializedObject.FindProperty("dimensions");
        centerProperty = serializedObject.FindProperty("center");
        roundDimensionsToInt = serializedObject.FindProperty("roundDimensionsToInt");
        customColliderHeightProperty = serializedObject.FindProperty("customColliderHeight");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(dimensionsProperty);
        EditorGUILayout.PropertyField(centerProperty);
        EditorGUILayout.PropertyField(roundDimensionsToInt);
        EditorGUILayout.PropertyField(customColliderHeightProperty);

        serializedObject.ApplyModifiedProperties();
    }

    private void OnSceneGUI()
    {
        TileControl tileControl = (TileControl)target;

        if (!tileControl.enabled)
            return;

        Matrix4x4 handleMatrix = tileControl.transform.localToWorldMatrix;
        handleMatrix.SetRow(0, Vector4.Scale(handleMatrix.GetRow(0), new Vector4(1f, 1f, 0f, 1f)));
        handleMatrix.SetRow(1, Vector4.Scale(handleMatrix.GetRow(1), new Vector4(1f, 1f, 0f, 1f)));
        handleMatrix.SetRow(2, new Vector4(0f, 0f, 1f, tileControl.transform.position.z));

        using (new Handles.DrawingScope(handleMatrix))
        {
            boxBoundsHandle.center = tileControl.center; //new Vector2(tileControl.center.x, tileControl.center.y + 0.5f);
            boxBoundsHandle.size = tileControl.dimensions;

            boxBoundsHandle.SetColor(enabledColor);
            EditorGUI.BeginChangeCheck();
            boxBoundsHandle.DrawHandle();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(tileControl, "Modify Tile Control");

                if (tileControl.roundDimensionsToInt)
                    tileControl.dimensions = new Vector2((int)boxBoundsHandle.size.x, (int)boxBoundsHandle.size.y);
                else
                    tileControl.dimensions = boxBoundsHandle.size;
            }
        }
    }
}
