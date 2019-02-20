using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenFader : MonoBehaviour
{

    public enum FadeType
    {
        Black, Loading, GameOver,
    }

    protected static ScreenFader instance;
    public static ScreenFader Instance
    {
        get
        {
            if (instance != null)
                return instance;

            instance = FindObjectOfType<ScreenFader>();

            if (instance != null)
                return instance;

            Create();

            return instance;
        }
    }

    protected bool isFading;
    public static bool IsFading
    {
        get { return Instance.isFading; }
    }

    public static void Create()
    {
        ScreenFader controllerPrefab = Resources.Load<ScreenFader>("ScreenFader");
        instance = Instantiate(controllerPrefab);
    }

    public CanvasGroup faderCanvasGroup;
    public CanvasGroup loadingCanvasGroup;
    public CanvasGroup gameOverCanvasGroup;
    public float fadeDuration = 1f;

    const int maxSortingLayer = 32767;

    void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    protected IEnumerator Fade(float finalAlpha, CanvasGroup canvasGroup)
    {
        isFading = true;
        canvasGroup.blocksRaycasts = true;
        float fadeSpeed = Mathf.Abs(canvasGroup.alpha - finalAlpha) / fadeDuration;
        while (!Mathf.Approximately(canvasGroup.alpha, finalAlpha))
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, finalAlpha,
                fadeSpeed * Time.deltaTime);
            yield return null;
        }
        canvasGroup.alpha = finalAlpha;
        isFading = false;
        canvasGroup.blocksRaycasts = false;
    }

    public static void SetAlpha(float alpha)
    {
        Instance.faderCanvasGroup.alpha = alpha;
    }

    public static IEnumerator FadeSceneIn()
    {
        CanvasGroup canvasGroup;
        if (Instance.faderCanvasGroup.alpha > 0.1f)
            canvasGroup = Instance.faderCanvasGroup;
        else if (Instance.gameOverCanvasGroup.alpha > 0.1f)
            canvasGroup = Instance.gameOverCanvasGroup;
        else
            canvasGroup = Instance.loadingCanvasGroup;

        yield return Instance.StartCoroutine(Instance.Fade(0f, canvasGroup));

        canvasGroup.gameObject.SetActive(false);
    }

    public static IEnumerator FadeSceneOut(FadeType fadeType = FadeType.Black)
    {
        CanvasGroup canvasGroup;
        switch (fadeType)
        {
            case FadeType.Black:
                canvasGroup = Instance.faderCanvasGroup;
                break;
            case FadeType.GameOver:
                canvasGroup = Instance.gameOverCanvasGroup;
                break;
            default:
                canvasGroup = Instance.loadingCanvasGroup;
                break;
        }

        canvasGroup.gameObject.SetActive(true);

        yield return Instance.StartCoroutine(Instance.Fade(1f, canvasGroup));
    }
}
