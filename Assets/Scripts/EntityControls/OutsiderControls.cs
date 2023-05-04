using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class OutsiderControls : MonoBehaviour
{

    public float sightRange = 15;
    public GameObject lookDirObject;
    public float sightConeAngle = 60;
    public GameObject targetObject;
    public float targetAtRange;
    // These just so we can see the numbers in the editor.
    [SerializeField]
    private float scalarDot;
    [SerializeField]
    private float unitCircleProj;

    [SerializeField]
    private Light spotlight;
    private float redshift;

    // Pathing
    [SerializeField]
    private Collider pathingBox;
    private Vector3 movingTo;
    public float acceleration = 2;
    public Vector3 dir = Vector3.zero;


    void Start()
    {
        unitCircleProj = Mathf.Cos(Mathf.Deg2Rad * sightConeAngle * 0.5f);
        movingTo = GetNewDestination();
    }

    void Update()
    {

        Vector3 lookDirV = lookDirObject.transform.position - transform.position;
        Vector3 targetV = targetObject.transform.position - transform.position;

        targetAtRange = MyMagnitude(targetV);

        lookDirV = MyNormalize(lookDirV, MyMagnitude(lookDirV));
        targetV = MyNormalize(targetV, MyMagnitude(targetV));

        scalarDot = targetV.x * lookDirV.x + targetV.y * lookDirV.y + targetV.z * lookDirV.z;

        // This thing checks, if the target is in the spotlight and close enough to be seen.
        // (It's a radial trigger, just in 3d.)
        if (scalarDot > unitCircleProj && targetAtRange < sightRange)
        {
            if (redshift < 1)
            {
                redshift += Time.deltaTime * 2;
            }
        }
        else
        {
            if (redshift > 0)
            {
                redshift -= Time.deltaTime;
            }

            ContinueMoving();
        }
        // Just to be on the safe side this never flips over.
        redshift = Mathf.Clamp01(redshift);

        spotlight.intensity = 2 + redshift * 3;
        spotlight.color = new Color(1, 1 - redshift, 1 - redshift, 1);

        Move();
    }

    private void OnValidate()
    {
        // This adjusts the spotlight to correspond to the Outsider's vision cone.
        spotlight.spotAngle = sightConeAngle; 
    }

    // I'm using my own magnitude and normalize, because why not.
    private float MyMagnitude(Vector3 toMeasure)
    {
        float magnit = Mathf.Sqrt(toMeasure.x * toMeasure.x + toMeasure.y * toMeasure.y + toMeasure.z * toMeasure.z);
        return magnit;
    }

    private Vector3 MyNormalize(Vector3 unNormed, float magnit)
    {
        if (magnit > 1E-05f)
        {
            unNormed /= magnit;
        }
        else
        {
            unNormed = new(0, 0, 0);
        }
        return unNormed;
    }

    // Everything down from here is just outsider movement stuff.
    private void Move()
    {
        dir *= .998f;
        transform.position += dir * Time.deltaTime;
    }

    private void ContinueMoving()
    {
        if (MyMagnitude(this.transform.position - movingTo) < 1)
        {
            movingTo = GetNewDestination();
        }

        dir += (movingTo - transform.position).normalized * Time.deltaTime * acceleration;
    }

    private Vector3 GetNewDestination()
    {
        Vector3 position = pathingBox.bounds.center;
        Vector3 extents = pathingBox.bounds.extents;

        position.x += Random.Range(-extents.x, extents.x);
        position.y += Random.Range(-extents.y, extents.y);
        position.z += Random.Range(-extents.z, extents.z);
        return position;
    }


}
