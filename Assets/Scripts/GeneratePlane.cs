using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratePlane : MonoBehaviour {

    [Range(1.0f, 1000.0f)]
    public float Size = 10.0f;

    [Range(2, 255)]
    public int Segments = 5;

    [Range(0.1f,20f)]
    public float NoiseFactor = 1;

    [Range(3,10)]
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

        // Delta between segments
        float delta = Size / (float)Segments;
        

        // Generate the vertices
        float x = 0.0f;
        float z = 0.0f;
        float y = 0.0f;

        //float x_cent = ((seg_x * delta) - (Size * .5f)) * DoubleInverseSize;
        //float z_cent = ((seg_z * delta) - (Size * .5f)) * DoubleInverseSize;

        //crater = Mathf.Max(x_cent * x_cent, z_cent * z_cent);

        //float noisevalue = Mathf.Clamp01(Mathf.PerlinNoise(x*.1f, z*.1f)) * NoiseFactor;

        for (int seg_x = 0; seg_x <= Segments; seg_x++) {
            x = (float)seg_x * delta;
            for (int seg_z = 0; seg_z <= Segments; seg_z++) {

                z = (float)seg_z * delta;

                // Generate the Perlin Noise
                //z = (Mathf.PerlinNoise(x, y);
                float y1 = (Mathf.PerlinNoise((x / DivFirst), (z / DivFirst)) *
                AmplitudeFirst);
                float y2 = (Mathf.PerlinNoise((x / DivSecond), (z / DivSecond)) *
                AmplitudeSecond);
                float y3 = (Mathf.PerlinNoise((x / DivThird), (z / DivThird)) *
                AmplitudeThird);
                y = y1 + y2 + y3;

                verts.Add(new Vector3(x, y, z));
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
