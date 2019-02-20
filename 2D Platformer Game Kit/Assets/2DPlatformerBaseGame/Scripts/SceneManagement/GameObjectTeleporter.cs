using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectTeleporter : MonoBehaviour {

    protected static GameObjectTeleporter instance;
	public static GameObjectTeleporter Instance
    {
        get
        {
            if (instance != null)
                return instance;

            instance = FindObjectOfType<GameObjectTeleporter>();

            if (instance != null)
                return instance;

            GameObject gameObjectTeleporter = new GameObject("GameObjectTeleporter");
            instance = gameObjectTeleporter.AddComponent<GameObjectTeleporter>();

            return instance;
        }
    }

    protected bool transitioning;
    public static bool Transitioning
    {
        get { return Instance.transitioning; }
    }

    protected PlayerInput playerInput;

    void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        playerInput = FindObjectOfType<PlayerInput>();
    }

    public static void Teleport(TransitionPoint transitionPoint)
    {
        Transform destinationTransform = Instance.GetDestination(transitionPoint.transitionDestinationTag).transform;
        Instance.StartCoroutine(Instance.Transition(transitionPoint.transitioningGameObject, true, transitionPoint.resetInputValuesOnTransition, destinationTransform.position, true));
    }

    public static void Teleport(GameObject transitioningGameObject, Transform destination)
    {
        Instance.StartCoroutine(Instance.Transition(transitioningGameObject, false, false, destination.position, false));
    }

    public static void Teleport(GameObject transitioningGameObject, Vector3 destinationPosition)
    {
        Instance.StartCoroutine(Instance.Transition(transitioningGameObject, false, false, destinationPosition, false));
    }

    protected IEnumerator Transition(GameObject transitioningGameObject, bool releaseControl, bool resetInputValues, Vector3 destinationPosition, bool fade)
    {
        transitioning = true;

        if (releaseControl)
        {
            if (playerInput == null)
                playerInput = FindObjectOfType<PlayerInput>();
            playerInput.ReleaseControl(resetInputValues);
        }
        
        if (fade)
            yield return StartCoroutine(ScreenFader.FadeSceneOut());

        transitioningGameObject.transform.position = destinationPosition;
        
        if (fade)
            yield return StartCoroutine(ScreenFader.FadeSceneIn());

        if (releaseControl)
            playerInput.GainControl();

        transitioning = false;
    }

    protected SceneTransitionDestination GetDestination(SceneTransitionDestination.DestinationTag destinationTag)
    {
        SceneTransitionDestination[] entrances = FindObjectsOfType<SceneTransitionDestination>();
        for (int i = 0; i < entrances.Length; i++)
        {
            if (entrances[i].destinationTag == destinationTag)
                return entrances[i];
        }
        Debug.LogWarning("No entrance was found with the " + destinationTag + " tag.");
        return null;
    }
}
