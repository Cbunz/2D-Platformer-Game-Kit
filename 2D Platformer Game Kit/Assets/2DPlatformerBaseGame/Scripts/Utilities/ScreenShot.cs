using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShot : MonoBehaviour
{
    public string filePath;

    static int count = 0;
    
	void Update ()
    {
		if (Input.GetKeyDown(KeyCode.B))
        {
            string fileName = filePath + count + ".png";
            ScreenCapture.CaptureScreenshot(fileName);
            count++;
            Debug.Log("Screenshot Captured!");
        }
	}
}
