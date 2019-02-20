using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformCatcher : MonoBehaviour
{
	[Serializable]
    public class CaughtObject
    {
        public Rigidbody2D rigidbody;
        public Collider2D collider;
        public CharacterController2D character;
        public bool inContact;
        public bool checkedThisFrame;

        public void Move(Vector2 movement)
        {
            if (!inContact)
                return;

            if (character != null)
                character.Move(movement);
            else
                rigidbody.MovePosition(rigidbody.position + movement);
        }
    }

    public Rigidbody2D platformRigidbody;
    public ContactFilter2D contactFilter;

    protected List<CaughtObject> caughtObjects = new List<CaughtObject>(128);
    protected ContactPoint2D[] contactPoints = new ContactPoint2D[20];
    new protected Collider2D collider;
    protected PlatformCatcher parentCatcher;
    protected Action<Vector2> moveDelegate = null;

    public int CaughtObjectCount
    {
        get
        {
            int count = 0;
            for (int i = 0; i < caughtObjects.Count; i++)
            {
                if (caughtObjects[i].inContact)
                    count++;
            }
            return count;
        }
    }

    public float CaughtObjectsMass
    {
        get
        {
            float mass = 0f;
            for (int i = 0; i < caughtObjects.Count; i++)
            {
                if (caughtObjects[i].inContact)
                    mass += caughtObjects[i].rigidbody.mass;
            }
            return mass;
        }
    }

    private void Awake()
    {
        if (platformRigidbody == null)
            platformRigidbody = GetComponent<Rigidbody2D>();

        if (collider == null)
            collider = GetComponent<Collider2D>();

        parentCatcher = null;
        Transform currentParent = transform.parent;

        while (currentParent != null)
        {
            PlatformCatcher catcher = currentParent.GetComponent<PlatformCatcher>();
            if (catcher != null)
                parentCatcher = catcher;
            currentParent = currentParent.parent;
        }

        if (parentCatcher != null)
            parentCatcher.moveDelegate += MoveCaughtObjects;
    }

    void FixedUpdate()
    {
        for (int i = 0, count = caughtObjects.Count; i < count; i++)
        {
            CaughtObject caughtObject = caughtObjects[i];
            caughtObject.inContact = false;
            caughtObject.checkedThisFrame = false;
        }

        CheckRigidbodyContacts(platformRigidbody);

        bool checkAgain;
        do
        {
            for (int i = 0, count = caughtObjects.Count; i < count; i++)
            {
                CaughtObject caughtObject = caughtObjects[i];

                if (caughtObject.inContact)
                {
                    if (!caughtObject.checkedThisFrame)
                    {
                        CheckRigidbodyContacts(caughtObject.rigidbody);
                        caughtObject.checkedThisFrame = true;
                    }
                }

                if (!caughtObject.inContact)
                {
                    Collider2D caughtObjectCollider = caughtObjects[i].collider;

                    bool verticalAlignment = (caughtObjectCollider.bounds.max.x > collider.bounds.min.x) && (caughtObjectCollider.bounds.min.x < collider.bounds.max.x);
                    if (verticalAlignment)
                    {
                        float yDiff = caughtObjects[i].collider.bounds.min.y - collider.bounds.max.y;

                        if (yDiff > 0 && yDiff < 0.05f)
                        {
                            caughtObject.inContact = true;
                            caughtObject.checkedThisFrame = true;
                        }
                    }
                }
            }

            checkAgain = false;

            for (int i = 0, count = caughtObjects.Count; i < count; i++)
            {
                CaughtObject caughtObject = caughtObjects[i];
                if (caughtObject.inContact && !caughtObject.checkedThisFrame)
                {
                    checkAgain = true;
                    break;
                }
            }
        }
        while (checkAgain);
    }

    void CheckRigidbodyContacts (Rigidbody2D rb)
    {
        int contactCount = rb.GetContacts(contactFilter, contactPoints);

        for (int j = 0; j < contactCount; j++)
        {
            ContactPoint2D contactPoint2D = contactPoints[j];
            Rigidbody2D contactRigidbody = contactPoint2D.rigidbody == rb ? contactPoint2D.otherRigidbody : contactPoint2D.rigidbody;
            int listIndex = -1;

            for (int k = 0; k < caughtObjects.Count; k++)
            {
                if (contactRigidbody == caughtObjects[k].rigidbody)
                {
                    listIndex = k;
                    break;
                }
            }

            if (listIndex == -1)
            {
                if (contactRigidbody != null)
                {
                    if (contactRigidbody.bodyType != RigidbodyType2D.Static && contactRigidbody != platformRigidbody)
                    {
                        float dot = Vector2.Dot(contactPoint2D.normal, Vector2.down);
                        if (dot > 0.8f)
                        {
                            CaughtObject newCaughtObject = new CaughtObject
                            {
                                rigidbody = contactRigidbody,
                                character = contactRigidbody.GetComponent<CharacterController2D>(),
                                collider = contactRigidbody.GetComponent<Collider2D>(),
                                inContact = true,
                                checkedThisFrame = false
                            };

                            if (newCaughtObject.collider == null)
                                newCaughtObject.collider = contactRigidbody.GetComponentInChildren<Collider2D>();

                            caughtObjects.Add(newCaughtObject);
                        }
                    }
                }
            }
            else
            {
                caughtObjects[listIndex].inContact = true;
            }
        }
    }

    public void MoveCaughtObjects (Vector2 velocity)
    {
        if (moveDelegate != null)
        {
            moveDelegate.Invoke(velocity);
        }

        for (int i = 0, count = caughtObjects.Count; i < count; i++)
        {
            CaughtObject caughtObject = caughtObjects[i];
            if (parentCatcher != null && parentCatcher.caughtObjects.Find((CaughtObject A) => { return A.rigidbody == caughtObject.rigidbody; }) != null)
            {
                continue;
            }

            caughtObjects[i].Move(velocity);
        }
    }

    public bool HasCaughtObject (GameObject gameObject)
    {
        for (int i = 0; i < caughtObjects.Count; i++)
        {
            if (caughtObjects[i].collider.gameObject == gameObject && caughtObjects[i].inContact)
                return true;
        }

        return false;
    }
}
