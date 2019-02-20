using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointControl : MonoBehaviour
{
    public bool returnToNormal = true;
    public float rotateSpeed = 1f;

    private HingeJoint2D joint;
    private bool returnToOriginalRotation = true;
    private Quaternion originalRotation;
    new private Collider2D collider;

    private void Awake()
    {
        joint = GetComponent<HingeJoint2D>();
        collider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        originalRotation = transform.rotation;
    }

    private void FixedUpdate()
    {
        if (returnToNormal && returnToOriginalRotation && transform.rotation != originalRotation)
        {
            if (joint.jointAngle > 0)
                transform.RotateAround(collider.bounds.center, new Vector3(0,0,1), Quaternion.Angle(transform.rotation, originalRotation) * Time.deltaTime * rotateSpeed);
            else if (joint.jointAngle < 0)
                transform.RotateAround(collider.bounds.center, new Vector3(0, 0, 1), -Quaternion.Angle(transform.rotation, originalRotation) * Time.deltaTime * rotateSpeed);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 9)
        {
            returnToOriginalRotation = false;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 9)
        {
            returnToOriginalRotation = true;
        }
    }
}
