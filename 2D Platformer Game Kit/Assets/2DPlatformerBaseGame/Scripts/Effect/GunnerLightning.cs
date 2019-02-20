using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunnerLightning : MonoBehaviour
{
    public Transform end;

    public float updateInterval = 0.5f;

    public int pointCount = 10;
    public float randomOffset = 0.5f;

    Transform[] branch;
    float updateTime = 0;
    Vector3[] points;
    LineRenderer lineRenderer;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        points = new Vector3[pointCount];
        lineRenderer.positionCount = pointCount;
        lineRenderer.useWorldSpace = false;
    }

    private void Update()
    {
        if (Time.time >= updateTime)
        {
            lineRenderer.positionCount = pointCount;

            points[0] = Vector3.zero;
            Vector3 segment = (end.position - transform.position) / (pointCount - 1);

            for (int i = 1; i < pointCount - 1; i++)
            {
                points[i] = segment * i;
                points[i].y += Random.Range(-randomOffset, randomOffset);
                points[i].x += Random.Range(-randomOffset, randomOffset);
            }

            points[pointCount - 1] = end.position - transform.position;
            lineRenderer.SetPositions(points);

            updateTime += updateInterval;
        }
    }
}
