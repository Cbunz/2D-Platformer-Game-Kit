using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoutineHelper : MonoBehaviour {

    static RoutineHelper instance;

    public static void StartRoutine(IEnumerator routine)
    {
        if (!instance)
        {
            var obj = new GameObject("RoutineHelper", typeof(RoutineHelper));
            DontDestroyOnLoad(obj);
            instance = obj.GetComponent<RoutineHelper>();
        }
        instance.StartCoroutine(routine);
    }
}
