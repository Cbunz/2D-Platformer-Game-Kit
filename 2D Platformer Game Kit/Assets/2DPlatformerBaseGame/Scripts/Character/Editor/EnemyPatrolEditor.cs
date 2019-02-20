using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyPatrol))]
[CanEditMultipleObjects]
public class EnemyPatrolEditor : Editor
{

    /*
    SerializedProperty patrolLeftProperty;
    SerializedProperty patrolRightProperty;

    private void OnEnable()
    {
        patrolLeftProperty = serializedObject.FindProperty("patrolLeft");
        patrolRightProperty = serializedObject.FindProperty("patrolRight");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(patrolLeftProperty);
        EditorGUILayout.PropertyField(patrolRightProperty);

        serializedObject.ApplyModifiedProperties();
    }

    private void OnSceneGUI()
    {
        EnemyPatrol enemyPatrol = (EnemyPatrol)target;

        if (!enemyPatrol.enabled)
            return;

        EditorGUI.BeginChangeCheck();
        
        Vector3 newLeftPosition = Handles.PositionHandle(enemyPatrol.patrolLeft, Quaternion.identity);
        Vector3 newRightPosition = Handles.PositionHandle(enemyPatrol.patrolRight, Quaternion.identity);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(enemyPatrol, "Modify Enemy Patrol");

            enemyPatrol.patrolLeft = newLeftPosition;
            enemyPatrol.patrolRight = newRightPosition;
        }
    }
    */
}
