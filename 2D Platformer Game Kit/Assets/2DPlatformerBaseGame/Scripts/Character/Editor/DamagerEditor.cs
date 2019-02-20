using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

[CustomEditor(typeof(Damager))]
public class DamagerEditor : Editor
{
    static BoxBoundsHandle boxBoundsHandle = new BoxBoundsHandle();
    static Color enabledColor = Color.green + Color.grey;

    SerializedProperty damageProperty;
    SerializedProperty offsetProperty;
    SerializedProperty sizeProperty;
    SerializedProperty offsetBasedOnSpriteFacingProperty;
    SerializedProperty spriteRendererProperty;
    SerializedProperty canHitTriggersProperty;
    SerializedProperty forceRespawnProperty;
    SerializedProperty ignoreInvincibilityProperty;
    SerializedProperty hittableLayersProperty;
    SerializedProperty onDamageableHitProperty;
    SerializedProperty onNonDamageableHitProperty;

    void OnEnable()
    {
        damageProperty = serializedObject.FindProperty("damage");
        offsetProperty = serializedObject.FindProperty("offset");
        sizeProperty = serializedObject.FindProperty("size");
        offsetBasedOnSpriteFacingProperty = serializedObject.FindProperty("offsetBasedOnSpriteFacing");
        spriteRendererProperty = serializedObject.FindProperty("spriteRenderer");
        canHitTriggersProperty = serializedObject.FindProperty("canHitTriggers");
        forceRespawnProperty = serializedObject.FindProperty("forceRespawn");
        ignoreInvincibilityProperty = serializedObject.FindProperty("ignoreInvincibility");
        hittableLayersProperty = serializedObject.FindProperty("hittableLayers");
        onDamageableHitProperty = serializedObject.FindProperty("OnDamageableHit");
        onNonDamageableHitProperty = serializedObject.FindProperty("OnNonDamageableHit");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(damageProperty);
        EditorGUILayout.PropertyField(offsetProperty);
        EditorGUILayout.PropertyField(sizeProperty);
        EditorGUILayout.PropertyField(offsetBasedOnSpriteFacingProperty);
        if (offsetBasedOnSpriteFacingProperty.boolValue)
            EditorGUILayout.PropertyField(spriteRendererProperty);
        EditorGUILayout.PropertyField(canHitTriggersProperty);
        EditorGUILayout.PropertyField(forceRespawnProperty);
        EditorGUILayout.PropertyField(ignoreInvincibilityProperty);
        EditorGUILayout.PropertyField(hittableLayersProperty);
        EditorGUILayout.PropertyField(onDamageableHitProperty);
        EditorGUILayout.PropertyField(onNonDamageableHitProperty);

        serializedObject.ApplyModifiedProperties();
    }

    void OnSceneGUI()
    {
        Damager damager = (Damager)target;

        if (!damager.enabled)
        {
            return;
        }

        Matrix4x4 handleMatrix = damager.transform.localToWorldMatrix;
        handleMatrix.SetRow(0, Vector4.Scale(handleMatrix.GetRow(0), new Vector4(1f, 1f, 0f, 1f)));
        handleMatrix.SetRow(1, Vector4.Scale(handleMatrix.GetRow(1), new Vector4(1f, 1f, 0f, 1f)));
        handleMatrix.SetRow(2, new Vector4(0f, 0f, 1f, damager.transform.position.z));
        
        using (new Handles.DrawingScope(handleMatrix))
        {
            boxBoundsHandle.center = damager.offset;
            boxBoundsHandle.size = damager.size;

            boxBoundsHandle.SetColor(enabledColor);
            EditorGUI.BeginChangeCheck();
            boxBoundsHandle.DrawHandle();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(damager, "Modify Damager");

                damager.size = boxBoundsHandle.size;
                damager.offset = boxBoundsHandle.center;
            }
        }
    }
}
