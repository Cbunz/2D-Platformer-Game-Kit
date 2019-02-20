using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour {

    protected static SceneController instance;
    public static SceneController Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }

            instance = FindObjectOfType<SceneController>();

            if (instance != null)
            {
                return instance;
            }

            Create();

            return instance;
        }
    }

    protected bool transitioning;
    public static bool Transitioning
    {
        get { return Instance.transitioning; }
    }

    public static SceneController Create()
    {
        GameObject sceneControllerGameObject = new GameObject("SceneController");
        instance = sceneControllerGameObject.AddComponent<SceneController>();

        return instance;
    }

    public SceneTransitionDestination initialSceneTransitionDestination;

    protected Scene currentLevelScene;
    protected SceneTransitionDestination.DestinationTag levelRestartDestinationTag;
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

        if (initialSceneTransitionDestination != null)
        {
            SetEnteringGameObjectLocation(initialSceneTransitionDestination);
            // ScreenFader.SetAlpha(1f);
            // StartCoroutine(ScreenFader.FadeSceneIn());
            initialSceneTransitionDestination.OnReachDestination.Invoke();
        }
        else
        {
            currentLevelScene = SceneManager.GetActiveScene();
            levelRestartDestinationTag = SceneTransitionDestination.DestinationTag.A;
        }
    }

    public static void RestartLevel(bool resetHealth = true)
    {
        if (resetHealth && PlayerCharacter.Instance != null)
        {
            PlayerCharacter.Instance.damageable.SetHealth(PlayerCharacter.Instance.damageable.startingHealth);
        }

        Instance.StartCoroutine(Instance.Transition(Instance.currentLevelScene.name, true, Instance.levelRestartDestinationTag, TransitionPoint.TransitionType.DifferentLevel));
    }

    public static void RestartLevelWithDelay(float delay, bool resetHealth = true)
    {
        Instance.StartCoroutine(CallWithDelay(delay, RestartLevel, resetHealth));
    }

    public static void TransitionToScene(TransitionPoint transitionPoint)
    {
        Instance.StartCoroutine(Instance.Transition(transitionPoint.newSceneName, transitionPoint.resetInputValuesOnTransition, transitionPoint.transitionDestinationTag, transitionPoint.transitionType));
    }

    public static SceneTransitionDestination GetDestinationFromTag(SceneTransitionDestination.DestinationTag destinationTag)
    {
        return Instance.GetDestination(destinationTag);
    }

    protected IEnumerator Transition(string newSceneName, bool resetInputValues, SceneTransitionDestination.DestinationTag destinationTag, TransitionPoint.TransitionType transitionType = TransitionPoint.TransitionType.DifferentLevel)
    {
        transitioning = true;
        PersistentDataManager.SaveAllData();

        if (playerInput == null)
        {
            playerInput = FindObjectOfType<PlayerInput>();
        }
        playerInput.ReleaseControl(resetInputValues);
        // yield return StartCoroutine(ScreenFader.FadeSceneOut(ScreenFader.FadeType.Loading));
        PersistentDataManager.ClearPersisters();

        yield return SceneManager.LoadSceneAsync(newSceneName);
        playerInput = FindObjectOfType<PlayerInput>();
        playerInput.ReleaseControl(resetInputValues);

        PersistentDataManager.LoadAllData();
        SceneTransitionDestination entrance = GetDestination(destinationTag);
        SetEnteringGameObjectLocation(entrance);
        SetupNewScene(transitionType, entrance);
        if (entrance != null)
        {
            entrance.OnReachDestination.Invoke();
        }
        // yield return StartCoroutine(ScreenFader.FadeSceneIn());
        playerInput.GainControl();

        transitioning = false;
    }

    protected SceneTransitionDestination GetDestination(SceneTransitionDestination.DestinationTag destinationTag)
    {
        SceneTransitionDestination[] entrances = FindObjectsOfType<SceneTransitionDestination>();
        for (int i = 0; i < entrances.Length; i++)
        {
            if (entrances[i].destinationTag == destinationTag)
            {
                return entrances[i];
            }
        }
        Debug.LogWarning("No entrance found with the " + destinationTag + " tag.");
        return null;
    }

    protected void SetEnteringGameObjectLocation(SceneTransitionDestination entrance)
    {
        if (entrance == null)
        {
            Debug.LogWarning("Entering object's transform location has not been set.");
            return;
        }
        Transform entranceLocation = entrance.transform;
        Transform enteringTransform = entrance.transitioningGameObject.transform;
        enteringTransform.position = entranceLocation.position;
        enteringTransform.rotation = entranceLocation.rotation;
    }

    protected void SetupNewScene(TransitionPoint.TransitionType transitionType, SceneTransitionDestination entrance)
    {
        if (entrance == null)
        {
            Debug.LogWarning("Level restart spawn has not been set.");
            return;
        }

        if (transitionType == TransitionPoint.TransitionType.DifferentLevel)
        {
            SetLevelStart(entrance);
        }
    }

    protected void SetLevelStart(SceneTransitionDestination entrance)
    {
        currentLevelScene = entrance.gameObject.scene;
        levelRestartDestinationTag = entrance.destinationTag;
    }

    static IEnumerator CallWithDelay<T>(float delay, Action<T> call, T parameter)
    {
        yield return new WaitForSeconds(delay);
        call(parameter);
    }
}
