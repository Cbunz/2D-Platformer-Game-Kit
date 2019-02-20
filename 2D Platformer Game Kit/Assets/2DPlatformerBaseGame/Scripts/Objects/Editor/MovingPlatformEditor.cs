using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MovingPlatform))]
public class MovingPlatformEditor : Editor {

    MovingPlatform movingPlatform;

    float previewPosition;

    private void OnEnable()
    {
        previewPosition = 0;
        movingPlatform = target as MovingPlatform;

        if (!EditorApplication.isPlayingOrWillChangePlaymode)
        {
            MovingPlatformPreview.CreateNewPreview(movingPlatform);
        }
    }

    private void OnDisable()
    {
        MovingPlatformPreview.DestroyPreview();
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        movingPlatform.platformCatcher = EditorGUILayout.ObjectField("Platform Catcher", movingPlatform.platformCatcher, typeof(PlatformCatcher), true) as PlatformCatcher;
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Changed Catcher");
        }

        EditorGUI.BeginChangeCheck();
        previewPosition = EditorGUILayout.Slider("Preview position", previewPosition, 0.0f, 1.0f);
        if (EditorGUI.EndChangeCheck())
        {
            MovePreview();
        }

        EditorGUILayout.Separator();
        EditorGUILayout.Separator();

        EditorGUILayout.BeginVertical("box");
        EditorGUI.BeginChangeCheck();
        
        bool isStartingMoving = EditorGUILayout.Toggle("Start moving", movingPlatform.isMovingAtStart);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Changed move at start");
            movingPlatform.isMovingAtStart = isStartingMoving;
        }

        if (isStartingMoving)
        {
            EditorGUI.indentLevel += 1;
            EditorGUI.BeginChangeCheck();
            bool startOnlyWhenVisible = EditorGUILayout.Toggle("When becoming visible", movingPlatform.startMovingOnlyWhenVisible);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Changed move when visible");
                movingPlatform.startMovingOnlyWhenVisible = startOnlyWhenVisible;
            }
            EditorGUI.indentLevel -= 1;
        }
        
        EditorGUILayout.EndVertical();

        if (!isStartingMoving)
        {
            EditorGUI.BeginChangeCheck();
            string newTriggerTag = EditorGUILayout.TextField("Trigger Tag", movingPlatform.triggerTag);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Change Trigger Tag");
                movingPlatform.triggerTag = newTriggerTag;
            }
        }

        EditorGUI.BeginChangeCheck();
        MovingPlatform.MovingPlatformType platformType = (MovingPlatform.MovingPlatformType)EditorGUILayout.EnumPopup("Platform Type", movingPlatform.platformType);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Changed Moving Platform type");
            movingPlatform.platformType = platformType;
        }

        EditorGUI.BeginChangeCheck();
        float newSpeed = EditorGUILayout.FloatField("Speed", movingPlatform.speed);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Change Speed");
            movingPlatform.speed = newSpeed;
        }

        EditorGUILayout.Separator();
        EditorGUILayout.Separator();

        if (GUILayout.Button("Add Node"))
        {
            Undo.RecordObject(target, "added node");

            Vector3 position = movingPlatform.localNodes[movingPlatform.localNodes.Length - 1] + Vector3.right;

            ArrayUtility.Add(ref movingPlatform.localNodes, position);
            ArrayUtility.Add(ref movingPlatform.waitTimes, 0);
        }

        EditorGUIUtility.labelWidth = 64;
        int delete = -1;
        for (int i = 0; i < movingPlatform.localNodes.Length; ++i)
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();

            int size = 64;
            EditorGUILayout.BeginVertical(GUILayout.Width(size));
            EditorGUILayout.LabelField("Node " + i, GUILayout.Width(size));
            if (i != 0 && GUILayout.Button("Delete", GUILayout.Width(size)))
            {
                delete = i;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            Vector3 newPosition;
            if (i == 0)
            {
                newPosition = movingPlatform.localNodes[i];
            }
            else
            {
                newPosition = EditorGUILayout.Vector3Field("Position", movingPlatform.localNodes[i]);
            }
            float newTime = EditorGUILayout.FloatField("Wait Time", movingPlatform.waitTimes[i]);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "changed time or position");
                movingPlatform.waitTimes[i] = newTime;
                movingPlatform.localNodes[i] = newPosition;
            }
        }
        EditorGUIUtility.labelWidth = 0;

        if (delete != -1)
        {
            Undo.RecordObject(target, "Removed point moving platform");

            ArrayUtility.RemoveAt(ref movingPlatform.localNodes, delete);
            ArrayUtility.RemoveAt(ref movingPlatform.waitTimes, delete);
        }
    }

    private void OnSceneGUI()
    {
        MovePreview();

        for (int i = 0; i < movingPlatform.localNodes.Length; ++i)
        {
            Vector3 worldPos;
            if (Application.isPlaying)
            {
                worldPos = movingPlatform.WorldNode[i];
            }
            else
            {
                worldPos = movingPlatform.transform.TransformPoint(movingPlatform.localNodes[i]);
            }

            Vector3 newWorld = worldPos;
            if (i != 0)
            {
                newWorld = Handles.PositionHandle(worldPos, Quaternion.identity);
            }

            Handles.color = Color.red;

            if (i == 0)
            {
                if (movingPlatform.platformType != MovingPlatform.MovingPlatformType.Loop)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Handles.DrawDottedLine(worldPos, movingPlatform.WorldNode[movingPlatform.WorldNode.Length - 1], 10);
                }
                else
                {
                    Handles.DrawDottedLine(worldPos, movingPlatform.transform.TransformPoint(movingPlatform.localNodes[movingPlatform.localNodes.Length - 1]), 10);
                }
            }
            else
            {
                if (Application.isPlaying)
                {
                    Handles.DrawDottedLine(worldPos, movingPlatform.WorldNode[i - 1], 10);
                }
                else
                {
                    Handles.DrawDottedLine(worldPos, movingPlatform.transform.TransformPoint(movingPlatform.localNodes[i - 1]), 10);
                }

                if (worldPos != newWorld)
                {
                    Undo.RecordObject(target, "moved point");
                    movingPlatform.localNodes[i] = movingPlatform.transform.InverseTransformPoint(newWorld);
                }
            }
        }
    }

    void MovePreview()
    {
        if (Application.isPlaying)
        {
            return;
        }

        float step = 1.0f / (movingPlatform.localNodes.Length - 1);

        int starting = Mathf.FloorToInt(previewPosition / step);

        if (starting > movingPlatform.localNodes.Length - 2)
        {
            return;
        }

        float localRatio = (previewPosition - (step * starting)) / step;

        Vector3 localPos = Vector3.Lerp(movingPlatform.localNodes[starting], movingPlatform.localNodes[starting + 1], localRatio);

        MovingPlatformPreview.preview.transform.position = movingPlatform.transform.TransformPoint(localPos);

        SceneView.RepaintAll();
    }
}
