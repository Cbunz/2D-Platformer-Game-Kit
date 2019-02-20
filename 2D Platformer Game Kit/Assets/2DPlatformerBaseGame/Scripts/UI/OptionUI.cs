using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionUI : MonoBehaviour
{
    public void ExitPause()
    {
        PlayerCharacter.Instance.Unpause();
    }

    public void RestartLevel()
    {
        ExitPause();
        SceneController.RestartLevel();
    }
}
