using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class OutGlider : MonoBehaviour
{

    public float sightRange = 15;
    public GameObject lookDirObject;
    public GameObject targetObject;
    public float sightConeAngle = 60;
    private float unitCircleProj;

    [Range(2f,20f)]
    public float lifeFuel = 10;
    public float activationTimer = 3;
    private bool active = false;

    // Pathing
    [SerializeField]
    public Collider pathingBox;
    private Vector3 movingTo;
    public float acceleration = 4;
    public Vector3 dir = Vector3.zero;

    private void Start()
    {
        Vector3 position = pathingBox.bounds.center;
        Vector3 extents = pathingBox.bounds.extents;
        position.y += Random.Range(-extents.y, extents.y);
        movingTo = new Vector3(targetObject.transform.position.x, position.y, targetObject.transform.position.z);
        GetComponent<TrailRenderer>().enabled = false;
    }

    private void Update()
    {
        Debug.DrawRay(transform.position, (lookDirObject.transform.position - transform.position) * 5, Color.red);

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
