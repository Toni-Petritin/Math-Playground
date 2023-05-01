using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierPoint : MonoBehaviour
{

    public Transform control0; // "First" control point
    public Transform control1; // "Second" contorl point

    public Transform Anchor { get { return gameObject.transform; } } // Anchor point

    private void OnDrawGizmos()
    {

        Gizmos.color = Color.white;
        Gizmos.DrawLine(Anchor.position, control0.position);
        Gizmos.DrawLine(Anchor.position, control1.position);
    }

}
