using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BTAI;

public class BTDebug : EditorWindow
{
    protected BTAI.Root currentRoot = null;

    [MenuItem("Kit Tools/Behaviour Tree Debug")]
    static void OpenWindow()
    {
        BTDebug btDebug = GetWindow<BTDebug>();
        btDebug.Show();
    }

    private void OnGUI()
    {
        if (!Application.isPlaying)
            EditorGUILayout.HelpBox("Only works during play mode.", MessageType.Info);
        else
        {
            if (currentRoot == null)
                FindRoot();
            else
                RecursiveTreeParsing(currentRoot, 0, true);
        }
    }

    private void Update()
    {
        Repaint();
    }

    void RecursiveTreeParsing(Branch branch, int indent, bool parentIsActive)
    {
        List<BTNode> nodes = branch.Children();

        for (int i = 0; i < nodes.Count; ++i)
        {
            EditorGUI.indentLevel = indent;

            bool isActiveChild = branch.ActiveChild() == i;
            GUI.color = (isActiveChild && parentIsActive) ? Color.green : Color.white;
            EditorGUILayout.LabelField(nodes[i].ToString());

            if (nodes[i] is Branch)
                RecursiveTreeParsing(nodes[i] as Branch, indent + 1, isActiveChild);
        }
    }

    void FindRoot()
    {
        if (Selection.activeGameObject == null)
        {
            currentRoot = null;
            return;
        }

        IBTDebugable debugable = Selection.activeGameObject.GetComponentInChildren<IBTDebugable>();

        if (debugable != null)
            currentRoot = debugable.GetAIRoot();
    }
}
