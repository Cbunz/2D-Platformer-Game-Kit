using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneControllerWrapper : MonoBehaviour {

	public void RestartLevel(bool resetHealth)
    {
        SceneController.RestartLevel(resetHealth);
    }

    public void TransitionToScene(TransitionPoint transitionPoint)
    {
        SceneController.TransitionToScene(transitionPoint);
    }

    public void RestartLevelWithDelay(float delay)
    {
        SceneController.RestartLevelWithDelay(delay, false);
    }

    public void RestartLevelWithDelayAndHealthReset(float delay)
    {
        SceneController.RestartLevelWithDelay(delay, true);
    }
}
