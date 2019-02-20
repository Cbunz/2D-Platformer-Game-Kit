using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class EditModeGizmos : MonoBehaviour {

    public enum GizmoType
    {
        patrolBorder,
        location,
    }

    public GizmoType gizmoType;

    public enum BorderType
    {
        left,
        right,
    }

    public BorderType borderType;

    public EnemyBehaviour enemyBehaviour;

    private void Update()
    {
        if (enemyBehaviour.usePatrolBorders)
        {
            switch (borderType)
            {
                case BorderType.left:
                    transform.position = enemyBehaviour.patrolLeft + enemyBehaviour.transform.position;
                    break;
                case BorderType.right:
                    transform.position = enemyBehaviour.patrolRight + enemyBehaviour.transform.position;
                    break;
            }
        }
    }

    private void OnDrawGizmos()
    {
        switch (gizmoType)
        {
            case GizmoType.patrolBorder:
                if (enemyBehaviour.usePatrolBorders)
                {
                    Vector3 gizmoSize = new Vector3(1f, 1f, 1f);

                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireCube(transform.position, gizmoSize);
                }
                break;
        }
    }
}
