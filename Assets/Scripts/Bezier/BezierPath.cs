using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEditor;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class BezierPath : MonoBehaviour
{

    [SerializeField]
    Mesh2D road2D;

    [Range(2, 100)]
    public int Segments = 10;

    [Range(0.0f, 1f)]
    public float TValue = 0.0f;

    [Range(.01f, 2f)]
    public float meshScale = .2f;

    public BezierPoint[] points; // For storing all bezier points
        
    private Mesh mesh;

    [SerializeField]
    private bool showFrame = true; // Hides the frame (currently always a closed path)

    [SerializeField]
    private bool closedPath = true; // Closes the Bezier path into a loop.

    private void OnDrawGizmosSelected()
    {

        // Loop from beginning of array until the 2nd to last element (because i+1)
        for (int i = 0; i < points.Length - 1; i++)
        {
            Handles.DrawBezier(points[i].Anchor.position,
                               points[i + 1].Anchor.position,
                               points[i].control1.position,
                               points[i + 1].control0.position,
                               Color.magenta, default, 2f);
        }

        // Last part of the path from last bezier point to first bezier point
        Handles.DrawBezier(points[points.Length - 1].Anchor.position,
                           points[0].Anchor.position,
                           points[points.Length - 1].control1.position,
                           points[0].control0.position,
                           Color.magenta, default, 2f);


        float roadSpot = TValue * points.Length;
        int roadSpotFloor = Mathf.FloorToInt(roadSpot);
        int nextPoint = roadSpotFloor + 1;

        if (roadSpotFloor >= points.Length - 1)
        {
            roadSpotFloor = points.Length - 1;
            nextPoint = 0;
        }

        // Get the point from bezier curve that corresponds our t-value
        
        Vector3 tPos = GetBezierPosition(roadSpot - roadSpotFloor, points[roadSpotFloor], points[nextPoint]);
        Vector3 tDir = GetBezierDirection(roadSpot - roadSpotFloor, points[roadSpotFloor], points[nextPoint]);

        // Draw the position on the curve
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(tPos, 0.25f);
        

        // Try to get the rotation
        Quaternion rot = Quaternion.LookRotation(tDir);
        Handles.PositionHandle(tPos, rot);
        
        if (showFrame)
        {
            for (int i = 0; i < points.Length - 1; i++)
            {
                DrawWireFrame(i, i + 1);
            }
            DrawWireFrame(points.Length - 1, 0);
        }
    }

    private void Awake()
    {

        GenerateMesh();

        GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    private void OnValidate()
    {
        
        GenerateMesh();
    }

    void GenerateMesh()
    {
        // vertices
        List<Vector3> verts = new List<Vector3>();
        // uvs
        List<Vector2> uvs = new List<Vector2>();
        // tri_indices
        List<int> tri_indices = new List<int>();

        // Go through each Bezier point
        for (int p = 0; p < points.Length - 1; p++)
        {
            // Go through each segment
            for (int n = 0; n <= Segments; n++)
            {
                // Compute the t-value for the current segment
                float t = n / (float)Segments;

                // Get the point from bezier curve that corresponds our t-value
                Vector3 tPos = GetBezierPosition(t, points[p], points[p+1]);
                Vector3 tDir = GetBezierDirection(t, points[p], points[p+1]);
                Quaternion rot = Quaternion.LookRotation(tDir);

                // Loop through our road slice
                for (int index = 0; index < road2D.vertices.Length; index++)
                {
                    // Local point
                    Vector3 roadpoint = road2D.vertices[index].point;
                    // Local to World-transform
                    Vector3 worldpoint = tPos + rot * roadpoint * meshScale;
                    verts.Add(worldpoint);

                    // uv-coord hack thing
                    uvs.Add(new Vector2(roadpoint.x / 10f + .5f, t));
                }
            }
        }

        // Close the loop, if closedPath has been checked
        if (closedPath)
        // Go through each segment
        for (int n = 0; n <= Segments; n++)
        {
            // Compute the t-value for the current segment
            float t = n / (float)Segments;

            // Get the point from bezier curve that corresponds our t-value
            Vector3 tPos = GetBezierPosition(t, points[points.Length-1], points[0]);
            Vector3 tDir = GetBezierDirection(t, points[points.Length-1], points[0]);
            Quaternion rot = Quaternion.LookRotation(tDir);

            // Loop through our road slice
            for (int index = 0; index < road2D.vertices.Length; index++)
            {
                // Local point
                Vector3 roadpoint = road2D.vertices[index].point;
                // Local to World-transform
                Vector3 worldpoint = tPos + rot * roadpoint * meshScale;
                verts.Add(worldpoint);

                // uv-coord hack thing
                uvs.Add(new Vector2(roadpoint.x / 10f + .5f, t));
            }
        }

        // triangles
        // how many lines:
        int num_lines = road2D.lineIndices.Length / 2;

        int pathPoints = points.Length;
        if (!closedPath)
        {
            pathPoints--;
        }

        // Go through each Bezier point
        for (int p = 0; p < pathPoints; p++)
        {
            // Go through each segment
            for (int n = 0; n < Segments; n++)
            {
                for (int line = 0; line < num_lines; line++)
                {

                    // current "slice"
                    int curr_first = p * (Segments+1) * road2D.vertices.Length + 
                        n * road2D.vertices.Length + road2D.lineIndices[2 * line];
                    int curr_second = p * (Segments+1) * road2D.vertices.Length + 
                        n * road2D.vertices.Length + road2D.lineIndices[2 * line + 1];

                    // next "slice"
                    int next_first = road2D.vertices.Length + curr_first;
                    int next_second = road2D.vertices.Length + curr_second;


                    // First triangle
                    tri_indices.Add(curr_first);
                    tri_indices.Add(next_first);
                    tri_indices.Add(curr_second);

                    // Second triangle
                    tri_indices.Add(curr_second);
                    tri_indices.Add(next_first);
                    tri_indices.Add(next_second);

                }
            }
        }
        

        // Clear the mesh
        if (mesh != null)
            mesh.Clear();
        else
            mesh = new Mesh();

        // Set everything!!!
        mesh.SetVertices(verts);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(tri_indices, 0);
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().sharedMesh = mesh;

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

    void DrawWireFrame(int startBezierPoint, int endBezierPoint)
    {
        Vector3 tPos = new();
        Vector3 tDir = new();

        // Try to get the rotation
        Quaternion rot = new();

        for (int n = 0; n < Segments; n++)
        {
            float t = n / (float)Segments;
            // Get the point from bezier curve that corresponds our t-value
            tPos = GetBezierPosition(t, points[startBezierPoint], points[endBezierPoint]);
            tDir = GetBezierDirection(t, points[startBezierPoint], points[endBezierPoint]);
            rot = Quaternion.LookRotation(tDir);

            float tNext = (n + 1) / (float)Segments;
            // Get the point from bezier curve that corresponds our t-value
            Vector3 tPosNext = GetBezierPosition(tNext, points[startBezierPoint], points[endBezierPoint]);
            Vector3 tDirNext = GetBezierDirection(tNext, points[startBezierPoint], points[endBezierPoint]);
            Quaternion rotNext = Quaternion.LookRotation(tDirNext);

            for (int i = 0; i < road2D.vertices.Length; i++)
            {
                Vector3 roadpoint = road2D.vertices[i].point;
                Gizmos.color = Color.blue;
                //Gizmos.DrawSphere(tPos + rot * roadpoint, 0.15f);
                //Gizmos.DrawSphere(tPosNext + rotNext * roadpoint, 0.15f);
                Gizmos.color = Color.white;
                Gizmos.DrawLine(tPos + rot * roadpoint * meshScale, tPosNext + rotNext * roadpoint * meshScale);

            }

            for (int i = 0; i < road2D.vertices.Length - 1; i++)
            {
                Vector3 roadpoint = road2D.vertices[i].point;
                Vector3 roadpointNext = road2D.vertices[i + 1].point;

                Gizmos.DrawLine(tPos + rot * roadpoint * meshScale, tPos + rot * roadpointNext * meshScale);
            }
            Vector3 pointLast = road2D.vertices[road2D.vertices.Length - 1].point;
            Vector3 pointFirst = road2D.vertices[0].point;

            Gizmos.DrawLine(tPos + rot * pointLast * meshScale, tPos + rot * pointFirst * meshScale);

        }
    }


}
