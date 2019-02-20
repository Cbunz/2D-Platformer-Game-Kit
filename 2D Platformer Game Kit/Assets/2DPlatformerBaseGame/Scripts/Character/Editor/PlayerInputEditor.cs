using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerInput))]
public class PlayerInputEditor : DataPersisterEditor
{
    private bool isPrefab = false;
    private bool isNotInstance = false;

    protected override void OnEnable()
    {
        base.OnEnable();

        isPrefab = AssetDatabase.Contains(target);
        isNotInstance = PrefabUtility.GetCorrespondingObjectFromSource(target) == null;
    }

    public override void OnInspectorGUI()
    {
        if (isPrefab || isNotInstance)
        {
            base.OnInspectorGUI();
        }
        else
        {
            EditorGUILayout.HelpBox("Modify the prefab and not this instance.", MessageType.Warning);
            if (GUILayout.Button("Select Prefab"))
            {
                Selection.activeObject = PrefabUtility.GetCorrespondingObjectFromSource(target);
            }
        }
    }
}
