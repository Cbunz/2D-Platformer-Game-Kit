using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/*
[CustomEditor(typeof(PressurePad))]
public class PressurePadEditor : Editor
{
    SerializedProperty platformCatcherProperty;
    SerializedProperty activationTypeProperty;
    SerializedProperty requiredCountProperty;
    SerializedProperty requiredMassProperty;
    SerializedProperty deactivatedBoxSpriteProperty;
    SerializedProperty activatedBoxSpriteProperty;
    SerializedProperty boxesProperty;
    SerializedProperty onPressedProperty;
    SerializedProperty onReleaseProperty;

    void OnEnable()
    {
        platformCatcherProperty = serializedObject.FindProperty("platformCatcher");
        activationTypeProperty = serializedObject.FindProperty("activationType");
        requiredCountProperty = serializedObject.FindProperty("requiredCount");
        requiredMassProperty = serializedObject.FindProperty("requiredMass");
        deactivatedBoxSpriteProperty = serializedObject.FindProperty("deactivatedBoxSprite");
        activatedBoxSpriteProperty = serializedObject.FindProperty("activatedBoxSprite");
        boxesProperty = serializedObject.FindProperty("boxes");
        onPressedProperty = serializedObject.FindProperty("OnPressed");
        onReleaseProperty = serializedObject.FindProperty("OnRelease");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(platformCatcherProperty);
        EditorGUILayout.PropertyField(activationTypeProperty);
        if ((PressurePad.ActivationType)activationTypeProperty.enumValueIndex == PressurePad.ActivationType.ItemCount)
            EditorGUILayout.PropertyField(requiredCountProperty);
        else
            EditorGUILayout.PropertyField(requiredMassProperty);

        EditorGUILayout.PropertyField(deactivatedBoxSpriteProperty);
        EditorGUILayout.PropertyField(activatedBoxSpriteProperty);
        EditorGUILayout.PropertyField(boxesProperty, true);


        EditorGUILayout.PropertyField(onPressedProperty);
        EditorGUILayout.PropertyField(onReleaseProperty);

        serializedObject.ApplyModifiedProperties();
    }
}
*/
