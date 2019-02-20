using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

[CustomEditor(typeof(CharacterStateSetter))]
public class CharacterStateSetterEditor : Editor {

    SerializedProperty animatorProperty;

    SerializedProperty setCharacterVelocityProperty;
    SerializedProperty playerCharacterProperty;
    SerializedProperty characterVelocityProperty;

    SerializedProperty setCharacterFacingProperty;
    SerializedProperty faceLeftProperty;

    SerializedProperty setStateProperty;
    SerializedProperty animatorStateNameProperty;

    SerializedProperty setParametersProperty;
    SerializedProperty parameterSettersProperty;

    GUIContent animatorContent;

    GUIContent setCharacterVelocityContent;
    GUIContent playerCharacterContent;
    GUIContent characterVelocityContent;

    GUIContent setCharacterFacingContent;
    GUIContent faceLeftContent;

    GUIContent setStateContent;
    GUIContent animatorStateNameContent;

    GUIContent setParametersContent;
    GUIContent parameterSettersContent;

    GUIContent parameterSetterNameContent;
    GUIContent parameterSetterValueContent;

    string[] animatorStateNames;
    int stateNamesIndex;

    string[] parameterNames;
    CharacterStateSetter.ParameterSetter.ParameterType[] parameterTypes;
    int parameterNameIndex;

    void OnEnable()
    {
        animatorProperty = serializedObject.FindProperty("animator");

        setCharacterVelocityProperty = serializedObject.FindProperty("setCharacterVelocity");
        playerCharacterProperty = serializedObject.FindProperty("playerCharacter");
        characterVelocityProperty = serializedObject.FindProperty("characterVelocity");

        setCharacterFacingProperty = serializedObject.FindProperty("setCharacterFacing");
        faceLeftProperty = serializedObject.FindProperty("faceLeft");

        setStateProperty = serializedObject.FindProperty("setState");
        animatorStateNameProperty = serializedObject.FindProperty("animatorStateName");

        setParametersProperty = serializedObject.FindProperty("setParameters");
        parameterSettersProperty = serializedObject.FindProperty("parameterSetters");

        animatorContent = new GUIContent("Animator");

        setCharacterVelocityContent = new GUIContent("Set Character Velocity");
        playerCharacterContent = new GUIContent("Player Character");
        characterVelocityContent = new GUIContent("Character Velocity");

        setCharacterFacingContent = new GUIContent("Set Character Facing Content");
        faceLeftContent = new GUIContent("Face Left");

        setStateContent = new GUIContent("Set State");
        animatorStateNameContent = new GUIContent("Animator State Name");

        setParametersContent = new GUIContent("Set Parameters");
        parameterSettersContent = new GUIContent("Parameter Settings");

        parameterSetterNameContent = new GUIContent("Name");
        parameterSetterValueContent = new GUIContent("Value");

        SetAnimatorStateNames();
        if (animatorStateNames != null)
        {
            for (int i = 0; i < animatorStateNames.Length; i++)
            {
                if (animatorStateNames[i] == animatorStateNameProperty.stringValue)
                {
                    stateNamesIndex = i;
                }
            }
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(setCharacterVelocityProperty, setCharacterVelocityContent);
        if (setCharacterVelocityProperty.boolValue)
        {
            EditorGUILayout.PropertyField(playerCharacterProperty, playerCharacterContent);
            EditorGUILayout.PropertyField(characterVelocityProperty, characterVelocityContent);
        }

        EditorGUILayout.PropertyField(setCharacterFacingProperty, setCharacterFacingContent);
        if (setCharacterFacingProperty.boolValue)
        {
            EditorGUILayout.PropertyField(playerCharacterProperty, playerCharacterContent);
            EditorGUILayout.PropertyField(faceLeftProperty, faceLeftContent);
        }

        EditorGUILayout.PropertyField(setStateProperty, setStateContent);
        if (setStateProperty.boolValue)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(animatorProperty, animatorContent);
            if (EditorGUI.EndChangeCheck())
            {
                SetAnimatorStateNames();
            }

            if (animatorProperty.objectReferenceValue == null || ((Animator)animatorProperty.objectReferenceValue).runtimeAnimatorController == null)
            {
                EditorGUILayout.HelpBox("An animator controller has not been found and so state names cannot be chosen.", MessageType.Warning);
                animatorStateNameProperty.stringValue = "";
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                stateNamesIndex = EditorGUILayout.Popup(animatorStateNameContent.text, stateNamesIndex, animatorStateNames);
                if (EditorGUI.EndChangeCheck())
                {
                    animatorStateNameProperty.stringValue = animatorStateNames[stateNamesIndex];
                }
            }
        }

        EditorGUILayout.PropertyField(setParametersProperty, setParametersContent);
        if (setParametersProperty.boolValue)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(animatorProperty, animatorContent);
            if (EditorGUI.EndChangeCheck())
            {
                SetAnimatorStateNames();
            }

            if (animatorProperty.objectReferenceValue == null || ((Animator)animatorProperty.objectReferenceValue).runtimeAnimatorController == null)
            {
                EditorGUILayout.HelpBox("An animator controller has not been found and so state names cannot be chosen.", MessageType.Warning);
                parameterSettersProperty.arraySize = 0;
            }
            else
            {
                parameterSettersProperty.arraySize = EditorGUILayout.IntField(parameterSettersContent, parameterSettersProperty.arraySize);
                EditorGUI.indentLevel++;
                for (int i = 0; i < parameterSettersProperty.arraySize; i++)
                {
                    SerializedProperty elementProperty = parameterSettersProperty.GetArrayElementAtIndex(i);
                    ParameterSetterGUI(elementProperty);
                }
                EditorGUI.indentLevel--;
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    void SetAnimatorStateNames()
    {
        if (animatorProperty.objectReferenceValue == null)
        {
            animatorStateNames = null;
            parameterNames = null;
            parameterTypes = null;
            return;
        }

        Animator animator = animatorProperty.objectReferenceValue as Animator;

        if (animator.runtimeAnimatorController == null)
        {
            animatorStateNames = null;
            parameterNames = null;
            parameterTypes = null;
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

        List<string> stateNamesList = new List<string>();

        for (int i = 0; i < animatorController.layers.Length; i++)
        {
            for (int j = 0; j < animatorController.layers[i].stateMachine.states.Length; j++)
            {
                stateNamesList.Add(animatorController.layers[i].stateMachine.states[j].state.name);
            }

            GetStateMachinesFromStateMachineAndAddNAmes(stateNamesList, animatorController.layers[i].stateMachine);
        }

        animatorStateNames = stateNamesList.ToArray();
    }

    static void GetStateMachinesFromStateMachineAndAddNAmes (List<string> stateNamesList, AnimatorStateMachine stateMachine)
    {
        AnimatorStateMachine[] stateMachines = new AnimatorStateMachine[stateMachine.stateMachines.Length];

        for (int i = 0; i < stateMachines.Length; i++)
        {
            stateMachines[i] = stateMachine.stateMachines[i].stateMachine;

            for (int j = 0; j < stateMachines[i].states.Length; j++)
            {
                stateNamesList.Add(stateMachines[i].states[j].state.name);
            }

            GetStateMachinesFromStateMachineAndAddNAmes(stateNamesList, stateMachines[i]);
        }
    }

    void ParameterSetterGUI(SerializedProperty parameterSetterProperty)
    {
        SerializedProperty parameterNameProperty = parameterSetterProperty.FindPropertyRelative("parameterName");
        SerializedProperty parameterTypeProperty = parameterSetterProperty.FindPropertyRelative("parameterType");
        SerializedProperty boolValueProperty = parameterSetterProperty.FindPropertyRelative("boolValue");
        SerializedProperty floatValueProperty = parameterSetterProperty.FindPropertyRelative("floatValue");
        SerializedProperty intValueProperty = parameterSetterProperty.FindPropertyRelative("intValue");

        for (int i = 0; i < parameterNames.Length; i++)
        {
            if (parameterNames[i] == parameterNameProperty.stringValue)
            {
                parameterNameIndex = i;
                parameterTypeProperty.enumValueIndex = (int)parameterTypes[i];
            }
        }

        Rect position = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
        Rect nameLabelRect = new Rect(position.x, position.y, position.width * 0.2f, EditorGUIUtility.singleLineHeight);
        Rect nameControlRect = new Rect(nameLabelRect.x + nameLabelRect.width, position.y, position.width * 0.3f, position.height);
        Rect valueLabelRect = new Rect(nameControlRect.x + nameControlRect.width, position.y, position.width * 0.2f, position.height);
        Rect valueControlRect = new Rect(valueLabelRect.x + valueLabelRect.width, position.y, position.width * 0.3f, position.height);

        EditorGUI.LabelField(nameLabelRect, parameterSetterNameContent);
        parameterNameIndex = EditorGUI.Popup(nameControlRect, GUIContent.none.text, parameterNameIndex, parameterNames);
        parameterNameProperty.stringValue = parameterNames[parameterNameIndex];
        parameterTypeProperty.enumValueIndex = (int)parameterTypes[parameterNameIndex];

        switch ((CharacterStateSetter.ParameterSetter.ParameterType)parameterTypeProperty.enumValueIndex)
        {
            case CharacterStateSetter.ParameterSetter.ParameterType.Bool:
                EditorGUI.LabelField(valueLabelRect, parameterSetterValueContent);
                EditorGUI.PropertyField(valueControlRect, boolValueProperty, GUIContent.none);
                break;
            case CharacterStateSetter.ParameterSetter.ParameterType.Float:
                EditorGUI.LabelField(valueLabelRect, parameterSetterValueContent);
                EditorGUI.PropertyField(valueControlRect, floatValueProperty, GUIContent.none);
                break;
            case CharacterStateSetter.ParameterSetter.ParameterType.Int:
                EditorGUI.LabelField(valueLabelRect, parameterSetterValueContent);
                EditorGUI.PropertyField(valueControlRect, intValueProperty, GUIContent.none);
                break;
        }
    }
}
