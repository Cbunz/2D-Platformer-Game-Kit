using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestParameterSetter : MonoBehaviour {

    Animator chestAnimator;
    bool open = false;
    protected readonly int hashChestOpenParameter = Animator.StringToHash("Open");
    protected readonly int hashChestLockedParameter = Animator.StringToHash("Locked");

    private void Awake()
    {
        chestAnimator = GetComponent<Animator>();
    }

    public void Rattle()
    {
        chestAnimator.SetTrigger(hashChestLockedParameter);
    }

    public void OpenClose()
    {
        open = !open;
        chestAnimator.SetBool(hashChestOpenParameter, open);
    }

}
