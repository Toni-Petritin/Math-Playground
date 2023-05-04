using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadSupports : MonoBehaviour
{

    public BezierPoint[] points;

    [SerializeField]
    private GameObject supportPrefab;

    // This is a dumb thing to have here, because it doesn't scale with the road and has to be set manually,
    // but it's not meant to be perfect to begin with.
    private float roadThickness = -0.1f;

    void OnEnable()
    {
        //supportPrefabs.Clear();
        foreach(BezierPoint bp in points)
        {
            PlaceSupports(bp);
        }
    }


    void PlaceSupports(BezierPoint bp)
    {
        Vector3 pos = new Vector3(bp.Anchor.position.x, roadThickness, bp.Anchor.position.z);
        var support = Instantiate(supportPrefab, pos, Quaternion.identity, this.transform);
        support.transform.localScale = new Vector3(1, bp.Anchor.position.y * .5f, 1);
        
    }
}
