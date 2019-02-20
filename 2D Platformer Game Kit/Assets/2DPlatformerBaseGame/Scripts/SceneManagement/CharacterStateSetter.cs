using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sets values and current state of player. Used when transitioning between destinations, entering/exiting cutscenes, etc.
/// </summary>

public class CharacterStateSetter : MonoBehaviour
{
    [Serializable]
    public class ParameterSetter
    {
        public enum ParameterType
        {
            Bool, Float, Int, Trigger
        }

        public string parameterName;
        public ParameterType parameterType;
        public bool boolValue;
        public float floatValue;
        public int intValue;

        protected int hash;

        public void Awake()
        {
            hash = Animator.StringToHash(parameterName);
        }

        public void SetParameter (Animator animator)
        {
            switch (parameterType)
            {
                case ParameterType.Bool:
                    animator.SetBool(hash, boolValue);
                    break;
                case ParameterType.Float:
                    animator.SetFloat(hash, floatValue);
                    break;
                case ParameterType.Int:
                    animator.SetInteger(hash, intValue);
                    break;
                case ParameterType.Trigger:
                    animator.SetTrigger(hash);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public PlayerCharacter playerCharacter;

    public bool setCharacterVelocity;
    public Vector2 characterVelocity;

    public bool setCharacterFacing;
    public bool faceLeft;

    public Animator animator;

    public bool setState;
    public string animatorStateName;

    public bool setParameters;
    public ParameterSetter[] parameterSetters;

    int hashStateName;
    Coroutine SetCharacterStateCoroutine;

    void Awake()
    {
        hashStateName = Animator.StringToHash(animatorStateName);

        for (int i = 0; i < parameterSetters.Length; i++)
        {
            parameterSetters[i].Awake();
        }
    }

    public void SetCharacterState()
    {
        if (SetCharacterStateCoroutine != null)
        {
            StopCoroutine(SetCharacterStateCoroutine);
        }

        if (setCharacterVelocity)
        {
            playerCharacter.SetMoveVector(characterVelocity);
        }

        if (setCharacterFacing)
        {
            playerCharacter.ForceFacing(faceLeft);
        }

        if (setState)
        {
            animator.Play(hashStateName);
        }

        if (setParameters)
        {
            for (int i = 0; i < parameterSetters.Length; i++)
            {
                parameterSetters[i].SetParameter(animator);
            }
        }
    }

    public void SetCharacterState(float delay)
    {
        if (SetCharacterStateCoroutine != null)
        {
            StopCoroutine(SetCharacterStateCoroutine);
        }
        SetCharacterStateCoroutine = StartCoroutine(CallWithDelay(delay, SetCharacterState));
    }

    IEnumerator CallWithDelay(float delay, Action call)
    {
        yield return new WaitForSeconds(delay);
        call();
    }
}
