using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class meshCreationStructure : MonoBehaviour


{

    Vector3 startpoint = new Vector3(3,3,2);
    Vector3 endpoint = new Vector3(-3, -3, 2);

    private Vector3[] vertices = new Vector3[6];
    private Vector2[] uv = new Vector2[6];
    private int[] triangles = new int[12];

    private GameObject meshObject;
    private Mesh mesh;
    private float width = 2.0f;
    // Start is called before the first frame update
    void Start()
    {   
        GenerateMeshData();
        mesh = new Mesh();
        meshObject = new GameObject("Mesh Object", typeof(MeshRenderer), typeof(MeshFilter));
        meshObject.GetComponent<MeshFilter>().mesh = mesh;

        // generate vertices
        mesh.vertices = vertices;
        mesh.uv = uv;   
        mesh.triangles = triangles; 
    }

    private void GenerateMeshData()
    {
        Vector3 direction = (startpoint - endpoint).normalized;
        Vector3 upVector = new Vector3(0.0f, 1.0f, 0.0f);

        Vector3 right = Vector3.Cross(direction, upVector).normalized;

        Vector3 p1_s = startpoint + (right * width);
        Vector3 p2_s = startpoint + (-right * width);
        Vector3 p3_s = startpoint + (-upVector * width);

        Vector3 p1_e = endpoint + (right * width);
        Vector3 p2_e = endpoint + (-right * width);
        Vector3 p3_e = endpoint + (-upVector * width);

        vertices[0] = p1_s;
        vertices[1] = p2_s;
        vertices[2] = p3_s;
        vertices[3] = p1_e;
        vertices[4] = p2_e;
        vertices[5] = p3_e;
       


        triangles[0] = 0;
        triangles[1] = 2;
        triangles[2] = 3;
        
        triangles[3] = 2;
        triangles[4] = 5;
        triangles[5] = 3;

        triangles[6] = 1;
        triangles[7] = 2;
        triangles[8] = 4;

        triangles[9] = 2;
        triangles[10] = 5;
        triangles[11] = 4;

        
    }
}
