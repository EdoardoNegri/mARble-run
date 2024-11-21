using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;
using UnityEngine.Splines;
using Unity.Mathematics;

public class VRPathDrawer : MonoBehaviour
{
    public XRNode controllerNode = XRNode.RightHand; // Choose which controller to use
    public float minDistance = 0.01f; // Minimum distance between points and construction points
    public float maxDistToConnect = 0.02f;
    //public GameObject rampPrefab; // Prefab for the line renderer

    private Spline spline;
    private Vector3 lastPoint;
    private bool isDrawing = false;
    //private List<GameObject> segments = new List<GameObject>();
    private GameObject segment;

    private Vector3 currStartPoint;
    private Vector3 currEndPoint;


    //Mesh Generation stuff

    private GameObject meshObject;
    private Mesh mesh;
    public Material mat;

    //start and end point stores for connecting pieces
    private List<int> spline_startpoint_indices = new List<int>();
    private List<int> spline_endpoint_indices = new List<int>();
    private List<Vector3> spline_startpoint_points = new List<Vector3>();
    private List<Vector3> spline_endpoint_points = new List<Vector3>();

    //holds the mesh elements
    private List<Vector3> vertices = new List<Vector3>();
    private List<Vector2> uvs = new List<Vector2>();
    private List<int> tris = new List<int>();

    //number of vertices around each single point (dont change atm)
    private int numVertsPerPoint = 4;
    //distance of vertices generated from the spline
    private float width = 1.0f;

    void Start()
    {
        mesh = new Mesh();
        meshObject = new GameObject("Mesh Object", typeof(MeshRenderer), typeof(MeshFilter));
        meshObject.GetComponent<MeshFilter>().mesh = mesh;
    }

    void Update()
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(controllerNode);
        bool triggerValue;

        if (device.TryGetFeatureValue(CommonUsages.triggerButton, out triggerValue))
        {
            if (triggerValue && !isDrawing)
            {
                StartDrawing();
            }
            else if (triggerValue && isDrawing)
            {
                ContinueDrawing();
            }
            else if (!triggerValue && isDrawing)
            {
                StopDrawing();
            }
        }
    }

    void StartDrawing()
    {
        spline.Clear();
        isDrawing = true;
        currStartPoint = transform.position;
        lastPoint = currStartPoint;
        //create connector
    }

    void ContinueDrawing()
    {
        //maybe add an indicator of where you are drawing
        Vector3 currentPoint = transform.position;
        if (Vector3.Distance(currentPoint, lastPoint) > minDistance)
        {
            spline.Add(new BezierKnot(currentPoint));
            lastPoint = currentPoint;
        }
    }

    void StopDrawing()
    {
        isDrawing = false;
        currEndPoint = lastPoint;

        //Create a mesh along for this spline (All vertices are created here)
        ConstructRoute();
        //save the start end endpoints for connections
        spline_startpoint_points.Add(currStartPoint);
        spline_endpoint_points.Add(currEndPoint);
        //Add Connecting Faces
        ConnectEndpoints();

        mesh.SetVertices(vertices.ToArray());
        mesh.SetTriangles(tris.ToArray(), 0);

    }


    void ConnectEndpoints()
    {
        //segment = new GameObject("Segment");
        //segment.tag = "Route";
        //Instantiate(new GameObject("Connector01"), currStartPoint, Quaternion.identity, segment.transform);
        //Instantiate(new GameObject("Connector02"), currEndPoint, Quaternion.identity , segment.transform);


        //We take the endpoints of the newly generated spline
        Vector3 new_startpoint = spline_startpoint_points[spline_startpoint_points.Count - 1];
        int new_startpoint_index = spline_startpoint_indices[spline_startpoint_indices.Count - 1];
        Vector3 new_endpoint = spline_endpoint_points[spline_endpoint_points.Count - 1];
        int new_endpoint_index = spline_endpoint_indices[spline_endpoint_indices.Count - 1];

        int start_connection_index = -1;
        int end_connection_index = -1;

        //find another startpoint to connect new endpoint to (always start->end)
        float dist = float.MaxValue;

        for (int i = 0; i < spline_startpoint_points.Count - 1; i++)
        {
            if (Vector3.Distance(new_endpoint, spline_startpoint_points[i]) <= maxDistToConnect)
            {
                if (Vector3.Distance(new_endpoint, spline_startpoint_points[i]) < dist)
                {
                    start_connection_index = spline_startpoint_indices[i];
                }
            }


        }

        //find another endpoint to connect new startpoint to (always start->end)
        dist = float.MaxValue;

        for (int i = 0; i < spline_endpoint_points.Count - 1; i++)
        {
            if (Vector3.Distance(new_startpoint, spline_endpoint_points[i]) <= maxDistToConnect)
            {
                if (Vector3.Distance(new_startpoint, spline_endpoint_points[i]) < dist)
                {
                    end_connection_index = spline_endpoint_indices[i];
                }
            }
        }

        //make the additional faces
        if (start_connection_index >= 0)
        {
            FillFaces_single_step(new_endpoint_index, start_connection_index);
        }
        if (end_connection_index >= 0)
        {
            FillFaces_single_step(end_connection_index, new_startpoint_index);
        }

    }




    void ConstructRoute()
    {
        //float percentage = minDistance / spline.GetLength();
        float percentage = 0.1f;
        int startpoint_index = vertices.Count;
        int new_point_counter = 0;
        for (float t = 0f; t <= 1; t += percentage)
        {

            if (SplineUtility.Evaluate(spline, t, out float3 position, out float3 tangent, out float3 upVector))
            {
                new_point_counter++;
                //Instantiate(rampPrefab, position, Quaternion.LookRotation(tangent), segment.transform);


                //atm Generate 4 vertices around the points in shape in a V shape


                Vector3 right = Vector3.Cross(tangent, upVector).normalized;

                vertices.Add(((Vector3)position) + (right * width));
                vertices.Add(((Vector3)position) + (-((Vector3)upVector) * width));
                vertices.Add(((Vector3)position) + (-right * width));
                vertices.Add(((Vector3)position) + (-((Vector3)upVector) * width * 1.5f));
            }
        }
        //save the start/endpoints for connection later
        spline_startpoint_indices.Add(startpoint_index);
        spline_endpoint_indices.Add(startpoint_index + (numVertsPerPoint * (new_point_counter - 1)));
        FillFaces_along_spline(startpoint_index, startpoint_index + (numVertsPerPoint * (new_point_counter - 1)));
        for (int i = 0; i < spline_startpoint_indices.Count; i++)
        {
            print(spline_startpoint_indices[i]);
        }
        for (int i = 0; i < spline_endpoint_indices.Count; i++)
        {
            print(spline_endpoint_indices[i]);
        }
    }


    //go around a spline and connect subsequent points)
    void FillFaces_along_spline(int startpoint_offset, int endpoint_offset)
    {
        for (int i = startpoint_offset; i < endpoint_offset; i += numVertsPerPoint)
        {
            FillFaces_single_step(i, i + numVertsPerPoint);
        }

    }

    //connect vertices of 2 points
    void FillFaces_single_step(int offset1, int offset2)
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
}