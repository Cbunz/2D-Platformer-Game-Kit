using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueCanvasController : MonoBehaviour
{
    public Animator animator;
    public TextMeshProUGUI textMeshProUGUI;
    public string triggerTag = "";
    public float textDelay = 0.5f;
    public float skipDelay = 0.5f;
    [HideInInspector]
    public bool textFinished = false;

    private string currentText = "";
    private bool skip = false;
    private bool canSkip = false;
    

    protected Coroutine deactivationCoroutine;

    protected readonly int hashActiveParameter = Animator.StringToHash("Active");

    IEnumerator SetAnimatorParameterWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        animator.SetBool(hashActiveParameter, false);
    }

    public void ActivateCanvasWithText(string text, bool useTypeWriter)
    {
        if (deactivationCoroutine != null)
        {
            StopCoroutine(deactivationCoroutine);
            deactivationCoroutine = null;
        }

        gameObject.SetActive(true);
        animator.SetBool(hashActiveParameter, true);
        if (useTypeWriter)
            StartCoroutine(TypeWriterText(text));
        else
            textMeshProUGUI.text = text;
    }

    public void ActivateCanvasWithTranslatedText(string phrasekey)
    {
        if (deactivationCoroutine != null)
        {
            StopCoroutine(deactivationCoroutine);
            deactivationCoroutine = null;
        }

        gameObject.SetActive(true);
        animator.SetBool(hashActiveParameter, true);
        //textMeshProUGUI.text = Translator.Instance[phrasekey];
    }

    public void DeactivateCanvasWithDelay(float delay)
    {
        deactivationCoroutine = StartCoroutine(SetAnimatorParameterWithDelay(delay));
    }

    IEnumerator TypeWriterText(string fullText)
    {
        skip = false;
        canSkip = false;
        Invoke("CanSkip", skipDelay);

        for (int i = 0; i < fullText.Length; i++)
        {
            if (Input.anyKey && canSkip)
            {
                skip = true;
                break;
            }
            currentText = fullText.Substring(0, i);
            textMeshProUGUI.text = currentText;
            yield return new WaitForSeconds(textDelay);
        }

        if (skip == true)
        {
            textMeshProUGUI.text = fullText;
        }
        
        EventManager.TriggerEvent("CanContinue", triggerTag);
    }

    private void CanSkip()
    {
        canSkip = true;
    }
}
