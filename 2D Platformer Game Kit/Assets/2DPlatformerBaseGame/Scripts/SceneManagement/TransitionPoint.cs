using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

[RequireComponent(typeof(Collider2D))]
public class TransitionPoint : MonoBehaviour {

	public enum TransitionType
    {
        DifferentLevel,
        DifferentCutscene,
        SameScene,
    }

    public enum TransitionWhen
    {
        ExternalCall,
        InteractPressed,
        OnTriggerEnter,
    }

    [SceneName]
    public string newSceneName;
    public GameObject transitioningGameObject;
    public TransitionType transitionType;
    public SceneTransitionDestination.DestinationTag transitionDestinationTag;
    public TransitionPoint destinationTransform;
    public TransitionWhen transitionWhen;
    public bool resetInputValuesOnTransition = true;
    public bool requiresInventoryCheck;
    public InventoryController inventoryController;
    public InventoryController.InventoryChecker inventoryCheck;

    bool transitioningGameObjectPresent;

    private void Start()
    {
        if (transitionWhen == TransitionWhen.ExternalCall)
        {
            transitioningGameObjectPresent = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == transitioningGameObject)
        {
            transitioningGameObjectPresent = true;


        }
    }

    protected void TransitionInternal()
    {
        if (requiresInventoryCheck)
        {
            if (!inventoryCheck.CheckInventory(inventoryController))
            {
                return;
            }
        }
        
        if (transitionType == TransitionType.SameScene)
        {
            GameObjectTeleporter.Teleport(transitioningGameObject, destinationTransform.transform);
        }
        else
        {
            
        }
    }

    public void Transition()
    {
        if (!transitioningGameObjectPresent)
        {
            return;
        }

        if (transitionWhen == TransitionWhen.ExternalCall)
        {
            TransitionInternal();
        }
    }
}
