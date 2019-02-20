using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SceneNameAttribute))]
public class SceneNameDrawer : PropertyDrawer
{
    int sceneIndex = -1;
    GUIContent[] sceneNames;
    readonly string[] scenePathSplitters = { "/", ".unity" };

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (EditorBuildSettings.scenes.Length == 0)
            return;
        if (sceneIndex == -1)
            Setup(property);

        int oldIndex = sceneIndex;
        sceneIndex = EditorGUI.Popup(position, label, sceneIndex, sceneNames);

        if (oldIndex != sceneIndex)
            property.stringValue = sceneNames[sceneIndex].text;
    }

    void Setup(SerializedProperty property)
    {
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        sceneNames = new GUIContent[scenes.Length];

        for (int i = 0; i < sceneNames.Length; i++)
        {
            string path = scenes[i].path;
            string[] splitPath = path.Split(scenePathSplitters, StringSplitOptions.RemoveEmptyEntries);
            sceneNames[i] = new GUIContent(splitPath[splitPath.Length - 1]);
        }

        if (sceneNames.Length == 0)
        {
            sceneNames = new[] { new GUIContent("[No Scenes In Build Settings]") };
        }

        if (!string.IsNullOrEmpty(property.stringValue))
        {
            bool sceneNameFound = false;
            for (int i = 0; i < sceneNames.Length; i++)
            {
                if (sceneNames[i].text == property.stringValue)
                {
                    sceneIndex = i;
                    sceneNameFound = true;
                    break;
                }
            }
            if (!sceneNameFound)
            {
                sceneIndex = 0;
            }
        }
        else sceneIndex = 0;

        property.stringValue = sceneNames[sceneIndex].text;
    }
}
