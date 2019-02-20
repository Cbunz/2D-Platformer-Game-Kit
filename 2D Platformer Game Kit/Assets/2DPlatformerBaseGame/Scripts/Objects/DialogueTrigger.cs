using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour {

    public string[] textList;
    public float leaveDelay;
    public string triggerTag;
    public DialogueCanvasController dialogueCanvasController;

    private bool canContinue = false;
    private bool entered = false;

    private void OnEnable()
    {
        EventManager.StartListening("CanContinue", CanContinue);
        //EventManager.StartListening("CantContinue", CantContinue);
    }

    private void OnDisable()
    {
        EventManager.StopListening("CanContinue", CanContinue);
        //EventManager.StopListening("CantContinue", CantContinue);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if (collision.gameObject.layer == 9 && entered == false)
        if (collision.gameObject.layer == 9 && entered == false)
        {
            entered = true;
            StartCoroutine(TriggerText());
        }
    }

    IEnumerator TriggerText()
    {
        PlayerInput.Instance.ReleaseControl(true);

        canContinue = false;
        dialogueCanvasController.ActivateCanvasWithText(textList[0], true);
        yield return new WaitUntil(() => (canContinue) == true);
        yield return new WaitUntil(() => (Input.anyKey) == true);

        //foreach (string text in textList)
        //{
        //    canContinue = false;
        //    dialogueCanvasController.ActivateCanvasWithText(text, true);
        //    yield return new WaitUntil(() => (canContinue) == true);
        //    yield return new WaitUntil(() => (Input.anyKey) == true);
        //}

        dialogueCanvasController.DeactivateCanvasWithDelay(leaveDelay);

        yield return new WaitForSeconds(leaveDelay);
        PlayerInput.Instance.GainControl();
    }

    private void CanContinue(string otherTag)
    {
        if (otherTag == triggerTag)
            canContinue = true;
    }

    //private void CantContinue()
    //{
    //    canContinue = false;
    //}
}
