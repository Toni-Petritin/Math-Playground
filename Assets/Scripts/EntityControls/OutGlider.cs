using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class OutGlider : MonoBehaviour
{
    // Vision related
    public float sightRange = 6;
    public GameObject lookDirObject;
    public GameObject targetObject;
    public float sightConeAngleHorizontal = 60;
    private float horCircleProj;
    public float sightConeAngleVertical = 20;
    private float vertCircleProj;

    // Spotlight - not rectangle like the vision. I don't know how to make a rectangle light, I'm sorry.
    [SerializeField]
    private Light spotlight;
    private float alphashift;

    // AI state condition related
    [Range(2f,30f)]
    public float lifeFuel = 10;
    public float activationTimer = 2;
    private bool active = false;
    private bool seeTarget = false;

    // Pathing related
    [SerializeField]
    public Collider pathingBox;
    private Vector3 movingTo;
    public float acceleration = 5;
    public Vector3 dir = Vector3.zero;

    private void Start()
    {
        Vector3 position = pathingBox.bounds.center;
        Vector3 extents = pathingBox.bounds.extents;
        position.y += Random.Range(-extents.y, extents.y);
        movingTo = new Vector3(targetObject.transform.position.x, position.y, targetObject.transform.position.z);
        GetComponent<TrailRenderer>().enabled = false;

        // These are constant so there's no point in calculating them on each frame.
        horCircleProj = Mathf.Cos(Mathf.Deg2Rad * sightConeAngleHorizontal * 0.5f);
        vertCircleProj = Mathf.Cos(Mathf.Deg2Rad * sightConeAngleVertical * 0.5f);
    }

    private void Update()
    {
        // It would be really cool if these attacked or tried to crash into the BlueWing,
        // but alas, this is just a random little experiment.
        if (active)
        {
            lifeFuel -= Time.deltaTime;
            if (lifeFuel > 0)
            {
                CheckSight();
                ContinueMoving();
            }
            else if (lifeFuel < -5)
            {
                Destroy(gameObject);
            }
            if (lifeFuel < 0 && seeTarget)
            {
                seeTarget = false;
            }
        }
        else
        {
            activationTimer -= Time.deltaTime;
            if (activationTimer < 0)
            {
                active = true;
                GetComponent<TrailRenderer>().enabled = true;
            }
        }

        if (seeTarget && alphashift < 10.0f)
        {
            alphashift += Time.deltaTime;
        }
        else if (!seeTarget && alphashift > 1f)
        {
            alphashift -= Time.deltaTime;
        }

        spotlight.intensity = alphashift;

        Move();
    }

    // In the absense of the placeon surface crossproduct, which we did in class,
    // I hope this is somewhat evidence I know how to use crossproduct.
    private void CheckSight()
    {
        Vector3 lookAtUp = Vector3.Cross(lookDirObject.transform.position - transform.position, transform.right).normalized;
        Vector3 lookAtForward = (lookDirObject.transform.position - transform.position).normalized;
        Vector3 targetDir = (targetObject.transform.position - transform.position).normalized;

        Vector3 verticalProj = Vector3.Cross(transform.right, Vector3.Cross(targetDir, transform.right)).normalized;
        Vector3 horizontalProj = Vector3.Cross(lookAtUp, Vector3.Cross(targetDir, lookAtUp)).normalized;

        // Checks if the target is within a projected rectangle along lookAtForward vector within sightRange distance.
        if (Vector3.Dot(lookAtForward, verticalProj) > vertCircleProj && Vector3.Dot(lookAtForward,horizontalProj) > horCircleProj
            && Vector3.Distance(transform.position, targetObject.transform.position) < sightRange)
        {
            seeTarget= true;
        }
        else
        {
            seeTarget = false;
        }

        //These are to help visualize what's what in play mode, if you need it.
        // I put a few more originally, but it became such a mess of lines I just left these.
        Debug.DrawRay(transform.position, lookAtForward * 2, Color.blue);
        Debug.DrawRay(transform.position, transform.right * 2, Color.red);
        Debug.DrawRay(transform.position, lookAtUp * 2, Color.green);
        if (seeTarget)
        {
            Debug.DrawRay(transform.position, targetDir * 5, Color.magenta);
        }
        else
        {
            Debug.DrawRay(transform.position, targetDir * 5, Color.white);
        }
    }

    // Everything down from here is just outglider movement stuff.
    private void Move()
    {
        GravityFalls();
        dir *= .998f;
        transform.position += dir * Time.deltaTime;
        Vector3 adjustedDir;
        if (seeTarget)
        {
            adjustedDir = new Vector3(targetObject.transform.position.x, transform.position.y, targetObject.transform.position.z);
        }
        else
        {
            adjustedDir = new Vector3(dir.x, transform.position.y, dir.z);
        }
        transform.LookAt(adjustedDir);
    }

    private void ContinueMoving()
    {
        if (Vector3.Distance(this.transform.position, movingTo) < 1)
        {
            movingTo = GetNewDestination();
        }

        dir += (movingTo - transform.position).normalized * Time.deltaTime * acceleration;
    }

    private void GravityFalls()
    {
        dir += Vector3.down * Time.deltaTime * 2;
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
