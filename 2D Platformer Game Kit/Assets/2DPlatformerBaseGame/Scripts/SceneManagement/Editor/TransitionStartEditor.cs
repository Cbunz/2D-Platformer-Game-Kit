using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TransitionPoint))]
public class TransitionStartEditor : Editor
{
    SerializedProperty transitioningGameObjectProperty;
    SerializedProperty transitionTypeProperty;
    SerializedProperty newSceneNameProperty;
    SerializedProperty transitionDestinationTagProperty;
    SerializedProperty destinationTransformProperty;
    SerializedProperty transitionWhenProperty;
    SerializedProperty resetInputValuesOnTransitionProperty;
    SerializedProperty requiresInventoryCheckProperty;
    SerializedProperty inventoryControllerProperty;
    SerializedProperty inventoryCheckProperty;
    SerializedProperty inventoryItemsProperty;
    SerializedProperty onHasItemProperty;
    SerializedProperty onDoesNotHaveItemProperty;

    GUIContent[] inventoryControllerItems = new GUIContent[0];

    void OnEnable()
    {
        transitioningGameObjectProperty = serializedObject.FindProperty("transitioningGameObject");
        transitionTypeProperty = serializedObject.FindProperty("transitionType");
        newSceneNameProperty = serializedObject.FindProperty("newSceneName");
        transitionDestinationTagProperty = serializedObject.FindProperty("transitionDestinationTag");
        destinationTransformProperty = serializedObject.FindProperty("destinationTransform");
        transitionWhenProperty = serializedObject.FindProperty("transitionWhen");
        resetInputValuesOnTransitionProperty = serializedObject.FindProperty("resetInputValuesOnTransition");
        requiresInventoryCheckProperty = serializedObject.FindProperty("requiresInventoryCheck");
        inventoryControllerProperty = serializedObject.FindProperty("inventoryController");
        inventoryCheckProperty = serializedObject.FindProperty("inventoryCheck");
        inventoryItemsProperty = inventoryCheckProperty.FindPropertyRelative("inventoryItems");
        onHasItemProperty = inventoryCheckProperty.FindPropertyRelative("OnHasItem");
        onDoesNotHaveItemProperty = inventoryCheckProperty.FindPropertyRelative("OnDoesNotHaveItem");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(transitioningGameObjectProperty);

        EditorGUILayout.PropertyField(transitionTypeProperty);
        EditorGUI.indentLevel++;
        if ((TransitionPoint.TransitionType)transitionTypeProperty.enumValueIndex == TransitionPoint.TransitionType.SameScene)
        {
            EditorGUILayout.PropertyField(destinationTransformProperty);
        }
        else
        {
            EditorGUILayout.PropertyField(newSceneNameProperty);
            EditorGUILayout.PropertyField(transitionDestinationTagProperty);
        }
        EditorGUI.indentLevel--;

        EditorGUILayout.PropertyField(transitionWhenProperty);
        EditorGUILayout.PropertyField(resetInputValuesOnTransitionProperty);

        EditorGUILayout.PropertyField(requiresInventoryCheckProperty);
        if (requiresInventoryCheckProperty.boolValue)
        {
            EditorGUI.indentLevel++;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(inventoryControllerProperty);
            if (EditorGUI.EndChangeCheck() || (inventoryControllerProperty.objectReferenceValue != null && inventoryControllerItems.Length == 0))
            {
                SetupInventoryItemGUI();
            }

            if (inventoryControllerProperty.objectReferenceValue != null)
            {
                InventoryController controller = inventoryControllerProperty.objectReferenceValue as InventoryController;
                inventoryItemsProperty.arraySize = EditorGUILayout.IntField("Inventory Items", inventoryItemsProperty.arraySize);
                EditorGUI.indentLevel++;
                for (int i = 0; i < inventoryItemsProperty.arraySize; i++)
                {
                    SerializedProperty elementProperty = inventoryItemsProperty.GetArrayElementAtIndex(i);

                    int itemIndex = ItemIndexFromController(controller, elementProperty.stringValue);
                    if (itemIndex == -1)
                    {
                        EditorGUILayout.LabelField("No items found in controller");
                    }
                    else if (itemIndex == -2)
                    {
                        elementProperty.stringValue = inventoryControllerItems[0].text;
                    }
                    else if (itemIndex == -3)
                    {
                        Debug.LogWarning("Previously listed item check not found, resetting to item index 0");
                        elementProperty.stringValue = inventoryControllerItems[0].text;
                    }
                    else
                    {
                        itemIndex = EditorGUILayout.Popup(new GUIContent("Item " + i), itemIndex, inventoryControllerItems);
                        elementProperty.stringValue = inventoryControllerItems[itemIndex].text;
                    }
                }
                EditorGUI.indentLevel--;

                EditorGUILayout.PropertyField(onHasItemProperty);
                EditorGUILayout.PropertyField(onDoesNotHaveItemProperty);
            }
            else
            {
                for (int i = 0; i < inventoryItemsProperty.arraySize; i++)
                {
                    SerializedProperty elementProperty = inventoryItemsProperty.GetArrayElementAtIndex(i);
                    elementProperty.stringValue = "";
                }
            }

            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }

    void SetupInventoryItemGUI()
    {
        if (inventoryControllerProperty.objectReferenceValue == null)
        {
            return;
        }

        InventoryController inventoryController = inventoryControllerProperty.objectReferenceValue as InventoryController;

        inventoryControllerItems = new GUIContent[inventoryController.inventoryEvents.Length];
        for (int i = 0; i < inventoryController.inventoryEvents.Length; i++)
        {
            inventoryControllerItems[i] = new GUIContent(inventoryController.inventoryEvents[i].key);
        }
    }

    int ItemIndexFromController(InventoryController controller, string itemName)
    {
        if (controller.inventoryEvents.Length == 0)
        {
            return -1;
        }

        if (string.IsNullOrEmpty(itemName))
        {
            return -2;
        }

        for (int i = 0; i < controller.inventoryEvents.Length; i++)
        {
            if (controller.inventoryEvents[i].key == itemName)
            {
                return i;
            }
        }
        return -3;
    }
}
