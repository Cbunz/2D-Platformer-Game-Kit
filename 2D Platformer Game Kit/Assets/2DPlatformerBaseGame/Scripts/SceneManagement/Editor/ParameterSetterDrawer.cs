using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

[CustomPropertyDrawer(typeof(CharacterStateSetter.ParameterSetter))]
public class ParameterSetterDrawer : PropertyDrawer
{
    SerializedProperty animatorProperty;
    SerializedProperty parameterNameProperty;
    SerializedProperty parameterTypeProperty;
    SerializedProperty boolValueProperty;
    SerializedProperty floatValueProperty;
    SerializedProperty intValueProperty;
    bool setupCalled;
    string[] parameterNames;
    CharacterStateSetter.ParameterSetter.ParameterType[] parameterTypes;
    int parameterNameIndex;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (animatorProperty == null)
            return 0f;

        if (animatorProperty.objectReferenceValue == null)
            return EditorGUIUtility.singleLineHeight;

        return EditorGUIUtility.singleLineHeight * 3f;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (!setupCalled || parameterNames == null)
            ParameterSetterSetup(property);

        position.height = EditorGUIUtility.singleLineHeight;
        EditorGUI.PropertyField(position, animatorProperty);

        if (animatorProperty.objectReferenceValue == null)
            return;

        position.y += position.height;
        parameterNameIndex = EditorGUI.Popup(position, parameterNameIndex, parameterNames);
        parameterNameProperty.stringValue = parameterNames[parameterNameIndex];
        parameterTypeProperty.enumValueIndex = (int)parameterTypes[parameterNameIndex];

        position.y += position.height;
        switch ((CharacterStateSetter.ParameterSetter.ParameterType)parameterTypeProperty.enumValueIndex)
        {
            case CharacterStateSetter.ParameterSetter.ParameterType.Bool:
                EditorGUI.PropertyField(position, boolValueProperty);
                break;
            case CharacterStateSetter.ParameterSetter.ParameterType.Float:
                EditorGUI.PropertyField(position, floatValueProperty);
                break;
            case CharacterStateSetter.ParameterSetter.ParameterType.Int:
                EditorGUI.PropertyField(position, intValueProperty);
                break;
        }
    }

    void ParameterSetterSetup(SerializedProperty property)
    {
        setupCalled = true;

        animatorProperty = property.FindPropertyRelative("animator");
        parameterNameProperty = property.FindPropertyRelative("parameterName");
        parameterTypeProperty = property.FindPropertyRelative("parameterType");
        boolValueProperty = property.FindPropertyRelative("boolValue");
        floatValueProperty = property.FindPropertyRelative("floatValue");
        intValueProperty = property.FindPropertyRelative("intValue");

        if (animatorProperty.objectReferenceValue == null)
        {
            parameterNames = null;
            return;
        }

        Animator animator = animatorProperty.objectReferenceValue as Animator;

        if (animator.runtimeAnimatorController == null)
        {
            parameterNames = null;
            return;
        }

        AnimatorController animatorController = animator.runtimeAnimatorController as AnimatorController;

        AnimatorControllerParameter[] parameters = animatorController.parameters;

        parameterNames = new string[parameters.Length];
        parameterTypes = new CharacterStateSetter.ParameterSetter.ParameterType[parameters.Length];

        for (int i = 0; i < parameterNames.Length; i++)
        {
            parameterNames[i] = parameters[i].name;

            switch (parameters[i].type)
            {
                case AnimatorControllerParameterType.Float:
                    parameterTypes[i] = CharacterStateSetter.ParameterSetter.ParameterType.Float;
                    break;
                case AnimatorControllerParameterType.Int:
                    parameterTypes[i] = CharacterStateSetter.ParameterSetter.ParameterType.Int;
                    break;
                case AnimatorControllerParameterType.Bool:
                    parameterTypes[i] = CharacterStateSetter.ParameterSetter.ParameterType.Bool;
                    break;
                case AnimatorControllerParameterType.Trigger:
                    parameterTypes[i] = CharacterStateSetter.ParameterSetter.ParameterType.Trigger;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        for (int i = 0; i < parameterNames.Length; i++)
        {
            if (parameterNames[i] == parameterNameProperty.stringValue)
            {
                parameterNameIndex = i;
                parameterTypeProperty.enumValueIndex = (int)parameterTypes[i];
            }
        }
    }
}
