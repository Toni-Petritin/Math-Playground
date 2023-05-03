using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ShuttleMove : MonoBehaviour
{

    public BezierPoint[] points; // For storing all bezier points
    [Range(0f,1f)]
    public float LapPos = 0.0f;

    [SerializeField]
    private GameObject prefab;

    private void Awake()
    {
        Instantiate(prefab, this.transform);
    }

    void Update()
    {
        // Just to be clear.
        // I know this takes the raw local positions of the Bezier points, instead of the world coordinates.
        // Thus this only works while the Bezier path GameObject is at (0,0,0).
        // It's an easy fix to just add its transform, but it would require me almost as much effort as
        // writing this comment... And more importantly the code wouldn't look as pretty.

        LapPos += Time.deltaTime * .1f;
        if (LapPos >= 1)
        {
            LapPos = 0;
        }

        float roadSpot = LapPos * points.Length;
        int roadSpotFloor = Mathf.FloorToInt(roadSpot);
        int nextPoint = roadSpotFloor + 1;

        if (roadSpotFloor >= points.Length - 1)
        {
            roadSpotFloor = points.Length - 1;
            nextPoint = 0;
        }


        Vector3 tPos = GetBezierPosition(roadSpot - roadSpotFloor, points[roadSpotFloor], points[nextPoint]);
        Vector3 tDir = GetBezierDirection(roadSpot - roadSpotFloor, points[roadSpotFloor], points[nextPoint]);

        this.transform.position = tPos;
        this.transform.rotation = Quaternion.LookRotation(tDir);

    }


    Vector3 GetBezierPosition(float t, BezierPoint bp1, BezierPoint bp2)
    {

        // 1st Lerp: 
        Vector3 PtX = (1 - t) * bp1.Anchor.position + t * bp1.control1.position;
        Vector3 PtY = (1 - t) * bp1.control1.position + t * bp2.control0.position;
        Vector3 PtZ = (1 - t) * bp2.control0.position + t * bp2.Anchor.position;

        // 2nd Lerp: 
        Vector3 PtR = (1 - t) * PtX + t * PtY;
        Vector3 PtS = (1 - t) * PtY + t * PtZ;

        // 3rd Lerp:
        return (1 - t) * PtR + t * PtS;
    }

    Vector3 GetBezierDirection(float t, BezierPoint bp1, BezierPoint bp2)
    {

        // 1st Lerp: 
        Vector3 PtX = (1 - t) * bp1.Anchor.position + t * bp1.control1.position;
        Vector3 PtY = (1 - t) * bp1.control1.position + t * bp2.control0.position;
        Vector3 PtZ = (1 - t) * bp2.control0.position + t * bp2.Anchor.position;

        // 2nd Lerp: 
        Vector3 PtR = (1 - t) * PtX + t * PtY;
        Vector3 PtS = (1 - t) * PtY + t * PtZ;

        // Compute the direction vector
        return (PtS - PtR).normalized;
    }

}
