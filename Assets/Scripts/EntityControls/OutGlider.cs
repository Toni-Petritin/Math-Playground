using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class OutGlider : MonoBehaviour
{
    // Vision related
    public float sightRange = 5;
    public GameObject lookDirObject;
    public GameObject targetObject;
    public float sightConeAngleHorizontal = 60;
    [SerializeField]
    private float horCircleProj;
    public float sightConeAngleVertical = 20;
    [SerializeField]
    private float vertCircleProj;
    public float targetAtRange;

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
        CheckSight();

        if (active)
        {
            lifeFuel -= Time.deltaTime;
            if (lifeFuel > 0)
            {
                ContinueMoving();
            }
            else if (lifeFuel < -5)
            {
                Destroy(gameObject);
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

        Move();
    }

    private void CheckSight()
    {
        Vector3 lookAtUp = Vector3.Cross(lookDirObject.transform.position - transform.position, transform.right).normalized;
        Vector3 lookAtForward = (lookDirObject.transform.position - transform.position).normalized;
        Vector3 targetDir = (targetObject.transform.position - transform.position).normalized;

        Vector3 verticalProj = Vector3.Cross(transform.right, Vector3.Cross(targetDir, transform.right)).normalized;
        Vector3 horizontalProj = Vector3.Cross(lookAtUp, Vector3.Cross(targetDir, lookAtUp)).normalized;

        // Checks if the target is within a projected rectangle along lookAtForward vector within sightRange distance.
        if (Vector3.Dot(targetDir,verticalProj) > vertCircleProj && Vector3.Dot(targetDir,horizontalProj) > horCircleProj
            && Vector3.Distance(transform.position, targetObject.transform.position) < sightRange)
        {
            seeTarget= true;
        }
        else
        {
            seeTarget = false;
        }

        //These are to help visualize what's what in play mode, if you need it.
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
        Vector3 adjustedDir = new Vector3(dir.x, transform.position.y, dir.z);
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
        dir += Vector3.down * 2 * Time.deltaTime;
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


    //private void OnDrawGizmos()
    //{
    //    Vector3 lookDirV = lookDirObject.transform.position - transform.position;
    //    Vector3 targetV = targetObject.transform.position - transform.position;
    //    unitCircleProj = Mathf.Cos(Mathf.Deg2Rad * sightConeAngleHorizontal * 0.5f);

    //    // D_tick = ((V.cross(D)).cross(V)).normalize();
    //    // Z = cos(theta)*V + sin(theta)*D_tick;

    //    // I'm marking the "enemy" with a white dot here and drawing a line to the vision indicator gameobject (to find it easier).
    //    Gizmos.color = Color.white;
    //    Gizmos.DrawLine(lookDirObject.transform.position, transform.position);
    //    //Gizmos.DrawSphere(transform.position, 1f);
    //    Gizmos.color = Color.yellow;
    //    DrawSightCone(lookDirV, 5);


    //    targetAtRange = OwnMagnitude(targetV);

    //    lookDirV = OwnNormalize(lookDirV, OwnMagnitude(lookDirV));
    //    targetV = OwnNormalize(targetV, OwnMagnitude(targetV));

    //    scalarDot = targetV.x * lookDirV.x + targetV.z * lookDirV.z;


    //    // I did this check without the trigonometry I used elsewhere, because you wanted it so, I think?
    //    if (scalarDot > unitCircleProj && targetAtRange < sightRange)
    //    {
    //        Gizmos.color = Color.red;
    //    }
    //    else
    //    {
    //        Gizmos.color = Color.green;

    //    }

    //    Gizmos.DrawSphere(targetObject.transform.position, 1f);

    //}

    //// This draws the vision cone. It's just to make it look pretty. You don't need to understand what is happening here.
    //private void DrawSightCone(Vector3 dir, float steps)
    //{

    //    float srcAngles = Mathf.Rad2Deg * Mathf.Atan2(dir.z, dir.x);
    //    Vector3 initialPos = transform.position;
    //    Vector3 posA = initialPos;
    //    float stepAngles = sightConeAngleHorizontal / steps;
    //    float angle = srcAngles - sightConeAngleHorizontal / 2;
    //    for (int i = 0; i <= steps; i++)
    //    {
    //        // I'm sure with like half an hour of extra work I'd do this smarter, but this is how it's going to be now.
    //        float rad = Mathf.Deg2Rad * angle;
    //        Vector3 posB = initialPos;
    //        posB += new Vector3(sightRange * Mathf.Cos(rad), 0, sightRange * Mathf.Sin(rad));

    //        Gizmos.DrawLine(posA, posB);

    //        angle += stepAngles;
    //        posA = posB;
    //    }
    //    Gizmos.DrawLine(posA, initialPos);
    //}

    //// I wrote my own method for magnitude and normalize, because I don't know.
    //private float OwnMagnitude(Vector3 toMeasure)
    //{
    //    float magnit = Mathf.Sqrt(toMeasure.x * toMeasure.x + toMeasure.z * toMeasure.z);
    //    return magnit;
    //}

    //private Vector3 OwnNormalize(Vector3 unNormed, float magnit)
    //{
    //    unNormed.y = 0;
    //    return unNormed.normalized;
    //}




    //public void Initialize(Vector3 startPos, Vector3 endPos)
    //{
    //    m_startPos = startPos;
    //    m_endPos = endPos;
    //    m_isInitialized = true;
    //    GetComponent<TrailRenderer>().enabled = false;
    //}

    //if (m_elapsedTime > m_timeToTarget)
    //{
    //    //Explode();
    //    return;
    //}

    //m_elapsedTime += Time.deltaTime;

    //float t = m_elapsedTime / m_timeToTarget;

    //static float f(float x, float h) => -4 * h * x * x + 4 * h * x;
    //if (t < .5f)
    //{
    //    transform.position = new Vector3(m_startPos.x, f(t, m_arcHeight) + Mathf.Lerp(m_startPos.y, m_endPos.y, t), m_startPos.z);
    //}
    //else
    //{
    //    GetComponent<TrailRenderer>().enabled = true;
    //    Vector3 mid = Vector3.Lerp(m_startPos, m_endPos, (t - .5f) * (t - .5f) * (t - .5f) * 8);
    //    transform.position = new Vector3(mid.x, f(t, m_arcHeight) + Mathf.Lerp(m_startPos.y, m_endPos.y, t), mid.z);
    //}

    //public override void Explode()
    //{
    //    Instantiate(Exploder, transform.position + m_explosionOffset, Quaternion.identity);
    //    _ = SpawnExplosion(transform.position + m_explosionOffset);
    //    Destroy(gameObject);
    //}
}
