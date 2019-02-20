using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DataPersisterEditor : Editor
{
    IDataPersister dataPersister;

    protected virtual void OnEnable()
    {
        dataPersister = (IDataPersister)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DataPersisterGUI(dataPersister);
    }

    public static void DataPersisterGUI (IDataPersister dataPersister)
    {
        DataSettings dataSettings = dataPersister.GetDataSettings();

        DataSettings.PersistenceType persistenceType = (DataSettings.PersistenceType)EditorGUILayout.EnumPopup("Persistence Type", dataSettings.persistenceType);
        string dataTag = EditorGUILayout.TextField("Data Tag", dataSettings.dataTag);

        dataPersister.SetDataSettings(dataTag, persistenceType);
    }
}
