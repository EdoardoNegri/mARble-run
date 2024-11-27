using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;
using UnityEngine.Splines;


public class MeshGeneration : MonoBehaviour
{
    // Start is called before the first frame update
    //private List<Spline> segments = new List<Spline>();
    float width = 1.0f;
    int steps_per_spline = 10;
    List<Mesh> meshes = new List<Mesh>();


    public void buildTrack(List<Spline> newsegments)
    {
        

        for(int i = 0; i < newsegments.Count; i++)
        {   
            float3 position;
            float3 tangent;
            float3 upVector;

            List<Vector3> verticesP1 = new List<Vector3>();
            List<Vector3> verticesP2 = new List<Vector3>();
            List<Vector3> verticesP3 = new List<Vector3>();


            for (float t = 0; t < 1.0; t = t + 1 / steps_per_spline)
            {
                newsegments[i].Evaluate(t, out position, out tangent, out upVector);
                float3 right = Vector3.Cross(tangent, upVector).normalized;
                float3 p1 = position + (right * width);
                float3 p2 = position + (-right * width);
                float3 p3 = position + (-upVector * width);

                verticesP1.Add(p1);
                verticesP2.Add(p2);
                verticesP3.Add(p3);
                
            }

            Mesh m = new Mesh();
            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();
           

            for(int j = 1; j < verticesP1.Count; j++)
            {
                Vector3 p1 = verticesP1[j-1];
                Vector3 p2 = verticesP2[j-1];
                Vector3 p3 = verticesP3[j-1];
                Vector3 p1_1 = verticesP1[j];
                Vector3 p2_1 = verticesP2[j];
                Vector3 p3_1 = verticesP3[j];

                int offset1 = 8 * (i - 1);
                int offset2 = offset1 + 4;

                int t1_1 = offset1 + 0;
                int t2_1 = offset1 + 2;
                int t3_1 = offset1 + 3;

                int t4_1 = offset1 + 3;
                int t5_1 = offset1 + 1;
                int t6_1 = offset1 + 0;

                int t1_2 = offset2 + 0;
                int t2_2 = offset2 + 2;
                int t3_2 = offset2 + 3;

                int t4_2 = offset2 + 3;
                int t5_2 = offset2 + 1;
                int t6_2 = offset2 + 0;



                verts.AddRange(new List<Vector3> { p1, p3, p1_1, p3_1 });
                tris.AddRange(new List<int> { t1_1, t2_1, t3_1, t4_1, t5_1, t6_1 });
                verts.AddRange(new List<Vector3> { p2, p3, p2_1, p3_1 });
                tris.AddRange(new List<int> { t1_2, t2_2, t3_2, t4_2, t5_2, t6_2 });

            }

            m.SetVertices(verts);
            m.SetTriangles(tris, 0);
            meshes.Add(m);
            

        }

    }
}
