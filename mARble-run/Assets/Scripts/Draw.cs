using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Linq;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.NativeTypes;
using System.Collections;
using MagicLeap.OpenXR.Features;

namespace MagicLeap.Examples
{
public class Draw : MonoBehaviour
{
    public const float minDistance = 0.1f; // Minimum distance between points and construction points
    public Material mat;
    public GameObject sphere;

    //const values for mesh generation
    private const int numVertsPerPoint = 9;
    private const float width = 0.05f;
    private float spline_total_length = 0.0f;
    private const float spline_step_size = width;

    private Spline spline = new Spline();
    private MagicLeapController controller;
    private Vector3 currStartPoint;
    private Vector3 lastPoint;
    private GameObject sphereParent;

 void Start()
{
    //this is done such that when you are selecting something it doesn't draw
    controller = MagicLeapController.Instance;

    StartCoroutine(canDraw());
    // Set up dimming
    MagicLeapRenderingExtensionsFeature rendering = OpenXRSettings.Instance.GetFeature<MagicLeapRenderingExtensionsFeature>();
    rendering.BlendMode = XrEnvironmentBlendMode.Additive;
}


public IEnumerator canDraw()
{
    while (true)
    {
        if (controller.BumperIsPressed)
        {
            StartDrawing();
            yield return StartCoroutine(HandleDrawing()); // Coroutine waits until finished
            StopDrawing();
        }
        yield return null; // Wait for the next frame
    }
}

void StartDrawing()
{
    currStartPoint = controller.Position;
    spline.Add(new BezierKnot(currStartPoint));
    lastPoint = currStartPoint;
    
    sphereParent = new GameObject("SphereParent");
    Instantiate(sphere, currStartPoint, Quaternion.identity, sphereParent.transform);
}

private IEnumerator HandleDrawing()
{
    while (controller.BumperIsPressed) // Continue drawing while bumper is pressed
    {
        Vector3 currentPoint = controller.Position;
        if (Vector3.Distance(currentPoint, lastPoint) > minDistance)
        {
            spline_total_length += Vector3.Distance(currentPoint, lastPoint);

            spline.Add(new BezierKnot(currentPoint));
            Instantiate(sphere, currentPoint, Quaternion.identity, sphereParent.transform);
            lastPoint = currentPoint;
        }
        yield return null; // Pause until the next frame
    }
}

void StopDrawing()
{
    Destroy(sphereParent);
    // Create a mesh for this spline (All vertices are created here)
    addMeshSegment();
    spline.Clear();
    spline_total_length = 0.0f;
}

    public void addMeshSegment()
    {
        if (spline.Knots.Count() < 2)
            return;

        List<Vector3> curr_vertice_segment = new List<Vector3>();
        List<int> curr_tris = new List<int>();
        float percentage = (spline_total_length / spline_step_size);
        for (float t = 0f; t <= 1; t += percentage)
        {

            if (SplineUtility.Evaluate(spline, t, out float3 position, out float3 tangent, out float3 upVector))
            {
                Vector3 tang = ((Vector3)tangent).normalized;

                Vector3 forward = (new Vector3(tangent.x, 0f, tangent.z)).normalized;
                Vector3 up = ((Vector3) upVector).normalized;
                Vector3 right = Vector3.Cross(forward, up).normalized;
                Vector3 left = -right;
                Vector3 down = -up;

                //the (((Vector3)upVector)*width) is just there to move all points up a bit without redoing everything
                /*curr_vertice_segment.Add(((Vector3)position) + (((Vector3)upVector) * width) + (right * width * 1.2f));
                curr_vertice_segment.Add(((Vector3)position) + (((Vector3)upVector) * width) + (right * width));
                curr_vertice_segment.Add(((Vector3)position) + (((Vector3)upVector) * width) + ((right + down).normalized * width));

                curr_vertice_segment.Add(((Vector3)position) + (((Vector3)upVector) * width) + (down * width));
                curr_vertice_segment.Add(((Vector3)position) + (((Vector3)upVector) * width) + ((left + down).normalized * width));
                curr_vertice_segment.Add(((Vector3)position) + (((Vector3)upVector) * width) + (left * width));

                curr_vertice_segment.Add(((Vector3)position) + (((Vector3)upVector) * width) + (left * width * 1.2f));
                curr_vertice_segment.Add(((Vector3)position) + (((Vector3)upVector) * width) + ((left * width) * 1.2f) + (down * width * 1.2f));
                curr_vertice_segment.Add(((Vector3)position) + (((Vector3)upVector) * width) + ((right * width) * 1.2f) + (down * width * 1.2f));
                */
                curr_vertice_segment.Add(((Vector3)position) + (((Vector3)up) * width) + (right * width * 1.2f));
                curr_vertice_segment.Add(((Vector3)position) + (((Vector3)up) * width) + (right * width));
                curr_vertice_segment.Add(((Vector3)position) + (((Vector3)up) * width) + ((right + down).normalized * width));

                curr_vertice_segment.Add(((Vector3)position) + (((Vector3)up) * width) + (down * width));
                curr_vertice_segment.Add(((Vector3)position) + (((Vector3)up) * width) + ((left + down).normalized * width));
                curr_vertice_segment.Add(((Vector3)position) + (((Vector3)up) * width) + (left * width));

                curr_vertice_segment.Add(((Vector3)position) + (((Vector3)up) * width) + (left * width * 1.2f));
                curr_vertice_segment.Add(((Vector3)position) + (((Vector3)up) * width) + ((left * width) * 1.2f) + (down * width * 1.2f));
                curr_vertice_segment.Add(((Vector3)position) + (((Vector3)up) * width) + ((right * width) * 1.2f) + (down * width * 1.2f));


            }
        }
        FillFaces_along_spline(curr_vertice_segment, curr_tris);
        GameObject meshObject = new GameObject("Mesh Object", typeof(MeshRenderer), typeof(MeshFilter), typeof(Rigidbody), typeof(MeshCollider));

        GameObject connector1 = new GameObject("connector1");
        connector1.tag = "Connector";
        connector1.transform.position = currStartPoint;
        connector1.transform.SetParent(meshObject.transform);

        GameObject connector2 = new GameObject("connector2");
        connector2.tag = "Connector";
        connector2.transform.position = lastPoint;
        connector2.transform.SetParent(meshObject.transform);

        Mesh mesh = new Mesh();


        mesh.SetVertices(curr_vertice_segment.ToArray());
        mesh.SetTriangles(curr_tris.ToArray(), 0);

        mesh.RecalculateNormals();

        meshObject.GetComponent<MeshFilter>().mesh = mesh;
        meshObject.GetComponent<MeshRenderer>().material = mat;
        meshObject.GetComponent<Rigidbody>().isKinematic = true;
        meshObject.GetComponent<MeshCollider>().sharedMesh = mesh;

        this.GetComponent<Connect>().ConnectPoints(meshObject);

    }

    public GameObject addConnectorSegment(GameObject connector1, GameObject connector2)
    {
        spline.Clear();
        spline.Add(new BezierKnot(connector1.transform.position));
        spline.Add(new BezierKnot(connector2.transform.position));


        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        for (float t = 0f; t <= 1; t += 1)
        {

            if (SplineUtility.Evaluate(spline, t, out float3 position, out float3 tangent, out float3 upVector))
            {
                Vector3 right = Vector3.Cross(tangent, upVector).normalized;
                Vector3 left = -right;
                Vector3 down = -upVector;

                //the (((Vector3)upVector)*width) is just there to move all points up a bit without redoing everything
                verts.Add(((Vector3)position) + (((Vector3)upVector) * width) + (right * width * 1.2f));
                verts.Add(((Vector3)position) + (((Vector3)upVector) * width) + (right * width));
                verts.Add(((Vector3)position) + (((Vector3)upVector) * width) + ((right + down).normalized * width));

                verts.Add(((Vector3)position) + (((Vector3)upVector) * width) + (down * width));
                verts.Add(((Vector3)position) + (((Vector3)upVector) * width) + ((left + down).normalized * width));
                verts.Add(((Vector3)position) + (((Vector3)upVector) * width) + (left * width));

                verts.Add(((Vector3)position) + (((Vector3)upVector) * width) + (left * width * 1.2f));
                verts.Add(((Vector3)position) + (((Vector3)upVector) * width) + ((left * width) * 1.2f) + (down * width * 1.2f));
                verts.Add(((Vector3)position) + (((Vector3)upVector) * width) + ((right * width) * 1.2f) + (down * width * 1.2f));


            }
        }
        FillFaces_along_spline(verts, tris);
        GameObject meshObject = new GameObject("Mesh Object", typeof(MeshRenderer), typeof(MeshFilter), typeof(Rigidbody), typeof(MeshCollider));
        Mesh mesh = new Mesh();

        mesh.SetVertices(verts.ToArray());
        mesh.SetTriangles(tris.ToArray(), 0);

        mesh.RecalculateNormals();

        meshObject.GetComponent<MeshFilter>().mesh = mesh;
        meshObject.GetComponent<MeshRenderer>().material = mat;
        meshObject.GetComponent<Rigidbody>().isKinematic = true;
        meshObject.GetComponent<MeshCollider>().sharedMesh = mesh;


        return meshObject;
    }


    //go around a spline and connect subsequent points)
    void FillFaces_along_spline(List<Vector3> vertices, List<int> tris)
    {
        for (int i = 0; i < vertices.Count - numVertsPerPoint; i += numVertsPerPoint)
        {
            FillFaces_single_step(i, i + numVertsPerPoint, tris);
        }
        FillFaces_stops_start(0, tris);
        FillFaces_stops_end(vertices.Count - numVertsPerPoint, tris);


    }

    //connect vertices of 2 points
    void FillFaces_single_step(int offset1, int offset2, List<int> tris)
    {
        for (int i = 0; i < numVertsPerPoint - 1; i++)
        {
            tris.Add(offset1 + i);
            tris.Add(offset2 + i);
            tris.Add(offset1 + i + 1);


            tris.Add(offset1 + i + 1);
            tris.Add(offset2 + i);
            tris.Add(offset2 + i + 1);
        }

        tris.Add(offset1 + numVertsPerPoint - 1);
        tris.Add(offset2 + numVertsPerPoint - 1);
        tris.Add(offset1);

        tris.Add(offset2);
        tris.Add(offset1);
        tris.Add(offset2 + numVertsPerPoint - 1);


    }
    void FillFaces_stops_start(int offset, List<int> tris)
    {
        tris.Add(offset + 0);
        tris.Add(offset + 1);
        tris.Add(offset + 8);

        tris.Add(offset + 1);
        tris.Add(offset + 2);
        tris.Add(offset + 8);

        tris.Add(offset + 2);
        tris.Add(offset + 3);
        tris.Add(offset + 8);

        tris.Add(offset + 3);
        tris.Add(offset + 7);
        tris.Add(offset + 8);

        tris.Add(offset + 3);
        tris.Add(offset + 4);
        tris.Add(offset + 7);

        tris.Add(offset + 4);
        tris.Add(offset + 5);
        tris.Add(offset + 7);

        tris.Add(offset + 5);
        tris.Add(offset + 6);
        tris.Add(offset + 7);
    }

    void FillFaces_stops_end(int offset, List<int> tris)
    {
        tris.Add(offset + 8);
        tris.Add(offset + 1);
        tris.Add(offset + 0);

        tris.Add(offset + 8);
        tris.Add(offset + 2);
        tris.Add(offset + 1);

        tris.Add(offset + 8);
        tris.Add(offset + 3);
        tris.Add(offset + 2);

        tris.Add(offset + 8);
        tris.Add(offset + 7);
        tris.Add(offset + 3);

        tris.Add(offset + 7);
        tris.Add(offset + 4);
        tris.Add(offset + 3);

        tris.Add(offset + 7);
        tris.Add(offset + 5);
        tris.Add(offset + 4);

        tris.Add(offset + 7);
        tris.Add(offset + 6);
        tris.Add(offset + 5);

    }
}
}