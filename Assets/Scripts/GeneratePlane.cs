using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratePlane : MonoBehaviour {

    [Range(1.0f, 1000.0f)]
    public float Size = 10.0f;

    [Range(2, 255)]
    public int Segments = 5;

    // Generation variables
    [Range(0f,10000f)]
    public float offsetSeed = 0;
    [Range(3, 10)]
    public float AmplitudeFirst = 5;
    [Range(3, 10)]
    public float DivFirst = 10;
    [Range(1, 3)]
    public float AmplitudeSecond = 2;
    [Range(1, 3)]
    public float DivSecond = 3;
    [Range(.1f, 1)]
    public float AmplitudeThird = 1;
    [Range(.1f, 1)]
    public float DivThird = 1;
    [Range(0f,5f)]
    public float platouLevel = 1;
    [Range(0, 5)]
    public float AmplitudeRock = 1;
    [Range(1f,10f)]
    public float DivRock1 = 4;
    [Range(1f, 4f)]
    public float DivRockCut1 = 2;
    [Range(0.01f, 5f)]
    public float DivRockBottom1 = .1f;

    // For the shader
    public Gradient terrainGradient;

    private Mesh mesh = null;

    private void OnEnable() {
        if (mesh == null) {
            mesh = new Mesh();
            gameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
        }

        GenerateMesh();
    }

    private void GenerateMesh() {
        mesh.Clear();
        // vertices
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Color> colors = new List<Color>();

        // Delta between segments
        float delta = Size / (float)Segments;
        

        // Generate the vertices
        float x = 0.0f;
        float z = 0.0f;
        float y = 0.0f;



        for (int seg_x = 0; seg_x <= Segments; seg_x++) {
            x = (float)seg_x * delta + offsetSeed;
            for (int seg_z = 0; seg_z <= Segments; seg_z++) {

                z = (float)seg_z * delta + offsetSeed;

                // Generate the Perlin Noise
                float y1 = (Mathf.PerlinNoise(x / DivFirst, z / DivFirst) *
                AmplitudeFirst);
                float y2 = (Mathf.PerlinNoise(x / DivSecond, z / DivSecond) *
                AmplitudeSecond);
                float y3 = (Mathf.PerlinNoise(x / DivThird, z / DivThird) *
                AmplitudeThird);

                // Generate rocks
                float y_rock1 = (Mathf.Max(Mathf.PerlinNoise(x / DivRock1, z / DivRock1), 0.7f) - 0.7f) * 10f;
                float y_rock2 = (Mathf.Max(Mathf.PerlinNoise(x / DivRockCut1, z / DivRockCut1), 0.5f) - 0.5f) * 8f;
                float y_rock3 = Mathf.PerlinNoise(x / DivRockBottom1, z / DivRockBottom1) * 2f;
                float y_rock = Mathf.Max(0, Mathf.Min( y_rock1 - y_rock2, y_rock3)) * AmplitudeRock;

                y = Mathf.Max(platouLevel , y1 + y2 + y3) + y_rock;

                verts.Add(new Vector3((float)seg_x * delta, y, (float)seg_z * delta));
                if (y_rock > 0.01)
                {
                    colors.Add(terrainGradient.Evaluate(Mathf.Clamp01(y_rock)* .25f + .75f));
                }
                else
                {
                    float height = Mathf.InverseLerp(platouLevel, AmplitudeFirst + AmplitudeSecond + AmplitudeThird, y) * .7f;
                    colors.Add(terrainGradient.Evaluate(height));
                }
            }
        }

        // Generate the triangle indices
        for (int seg_x = 0; seg_x < Segments; seg_x++) {
            for (int seg_z = 0; seg_z < Segments; seg_z++) {
                // Total count of vertices per row & col is: Segments + 1
                int index = seg_x * (Segments+1) + seg_z;
                int index_lower = index + 1;
                int index_next_col = index + (Segments+1);
                int index_next_col_lower = index_next_col + 1;
                
                tris.Add(index);
                tris.Add(index_lower);
                tris.Add(index_next_col);

                tris.Add(index_next_col);
                tris.Add(index_lower);
                tris.Add(index_next_col_lower);
            }
        }
        
        mesh.SetVertices(verts);
        mesh.SetColors(colors);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
    }

    private void OnValidate() {
        if (mesh == null) {
            mesh = new Mesh();
            gameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
        }

        GenerateMesh();
    }

    // Start is called before the first frame update
    void Start() {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
