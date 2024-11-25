using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;
using UnityEngine.Splines;
using Unity.Mathematics;

namespace MagicLeap.Examples{
public class VRPathDrawer : MonoBehaviour
{
    public XRNode controllerNode = XRNode.RightHand; // Choose which controller to use
    public float minDistance = 0.01f; // Minimum distance between points and construction points
    public float maxDistToConnect = 0.02f;

    public float maxDistToErase = 0.02f;
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

    private MagicLeapController controller;

    //holds the mesh elements
    private List<List<Vector3>> vertice_segments = new List<List<Vector3>>();
    private List<int> tris = new List<int>();

    //number of vertices around each single point (dont change atm)
    private int numVertsPerPoint = 9;
    private int middlePointVertexOffset = 3;
    //distance of vertices generated from the spline
    private float width = 0.05f;

    void Start()
    {
        mesh = new Mesh();
        meshObject = new GameObject("Mesh Object", typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider));
        meshObject.GetComponent<MeshFilter>().mesh = mesh;
        meshObject.GetComponent<MeshRenderer>().material = mat;
        meshObject.GetComponent<MeshCollider>().sharedMesh = mesh;


        controller = MagicLeapController.Instance;
    }

    void Update()
    {
        if (controller.IsTracked)
        {
            // bool triggerValue = (controller.TriggerValue > 0.2f);
            bool bumperValue = controller.BumperIsPressed;
            bool buttonCondition = bumperValue;

            if (buttonCondition && !isDrawing)
            {
                StartDrawing();
            }
            else if (buttonCondition && isDrawing)
            {
                ContinueDrawing();
            }
            else if (!buttonCondition && isDrawing)
            {
                StopDrawing();
            }
        }
    }

    Vector3 GetControllerPosition()
    {
        return controller.Position;
    }

    void StartDrawing()
    {
        spline = new Spline();
        isDrawing = true;
        currStartPoint = GetControllerPosition();
        lastPoint = currStartPoint;
        Debug.Log($"Start Position: {currStartPoint}");

        //create connector
    }

    void ContinueDrawing()
    {
        //maybe add an indicator of where you are drawing
        Vector3 currentPoint = GetControllerPosition();
        Debug.Log($"Position: {currentPoint}");
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
        addVerticeSegment();
        /*foreach (var item in vertice_segments[vertice_segments.Count - 1])
        {   
            Debug.Log($"vertice Position: {item}");
        }
        foreach (var item in spline)
        {
            Debug.Log($"Spline Position: {item}");
        }*/
        spline.Clear();
    }

    void eraseSegment(Vector3 position)
    {
        int seg_to_remove = -1;
        float dist = float.MaxValue;
        for (int i = 0; i < vertice_segments.Count; i++)
        {
            for (int j = 0; j < vertice_segments[i].Count; j++)
            {
                if (((float)Vector3.Distance(position, vertice_segments[i][j])) <= maxDistToErase)
                {
                    if (((float)Vector3.Distance(position, vertice_segments[i][j])) < dist)
                    {
                        dist = ((float)Vector3.Distance(position, vertice_segments[i][j]));
                        seg_to_remove = i;
                        

                    }
                }
            }
        }
        
        if (seg_to_remove >= 0)
        {
            vertice_segments.RemoveAt(seg_to_remove);
            ConstructRoute();
            
        }
    }


    void ConnectEndpoints()
    {
        /*bool[] start_isconnected = new bool[spline_startpoint_indices.Count];
        bool[] end_isconnected = new bool[spline_endpoint_indices.Count];

        for (int s = 0; s < spline_startpoint_indices.Count; s++)
        {
            float dist = float.MaxValue;
            int start_connection_index = s;
            int end_connection_index = -1;
            for (int e = 0; e < spline_endpoint_indices.Count; e++)
            {
                if (((float)Vector3.Distance(spline_startpoint_points[s], spline_endpoint_points[e])) <= maxDistToConnect)
                {

                    if (((float)Vector3.Distance(spline_startpoint_points[s], spline_endpoint_points[e])) < dist)
                    {
                        dist = ((float)Vector3.Distance(spline_startpoint_points[s], spline_endpoint_points[e]));
                        end_connection_index = e;

                    }
                }
            }
            if (end_connection_index >= 0)
            {
                FillFaces_single_step(spline_endpoint_indices[end_connection_index], spline_startpoint_indices[start_connection_index]);
                start_isconnected[start_connection_index] = true;
                end_isconnected[end_connection_index] = true;

            }

        }

        for (int i = 0; i < start_isconnected.Length; i++)
        {
            if (!start_isconnected[i])
            {
                FillFaces_stops_start(spline_startpoint_indices[i]);
            }
        }
        for (int i = 0; i < end_isconnected.Length; i++)
        {
            if (!end_isconnected[i])
            {
                FillFaces_stops_end(spline_endpoint_indices[i]);
            }
        }*/

    }


    void addVerticeSegment()
    {

        List<Vector3> curr_vertice_segment = new List<Vector3>();
        float percentage = 0.2f;
        for (float t = 0f; t <= 1; t += percentage)
        {

            if (SplineUtility.Evaluate(spline, t, out float3 position, out float3 tangent, out float3 upVector))
            {
                Vector3 right = Vector3.Cross(tangent, upVector).normalized;
                Vector3 left = -right;
                Vector3 down = -upVector;

                //the (((Vector3)upVector)*width) is just there to move all points up a bit without redoing everything
                curr_vertice_segment.Add(((Vector3)position) + (((Vector3)upVector) * width) + (right * width * 1.2f));
                curr_vertice_segment.Add(((Vector3)position) + (((Vector3)upVector) * width) + (right * width));
                curr_vertice_segment.Add(((Vector3)position) + (((Vector3)upVector) * width) + ((right + down).normalized * width));

                curr_vertice_segment.Add(((Vector3)position) + (((Vector3)upVector) * width) + (down * width));
                curr_vertice_segment.Add(((Vector3)position) + (((Vector3)upVector) * width) + ((left + down).normalized * width));
                curr_vertice_segment.Add(((Vector3)position) + (((Vector3)upVector) * width) + (left * width));

                curr_vertice_segment.Add(((Vector3)position) + (((Vector3)upVector) * width) + (left * width * 1.2f));
                curr_vertice_segment.Add(((Vector3)position) + (((Vector3)upVector) * width) + ((left * width) * 1.2f) + (down * width * 1.2f));
                curr_vertice_segment.Add(((Vector3)position) + (((Vector3)upVector) * width) + ((right * width) * 1.2f) + (down * width * 1.2f));


            }
        }

        vertice_segments.Add(curr_vertice_segment);
        ConstructRoute();
    }


    void ConstructRoute()
    {
        mesh.Clear();
        int vertex_counter = 0;
        List<Vector3> vertices = new List<Vector3>();
        tris.Clear();
        spline_startpoint_indices = new List<int>();
        spline_endpoint_indices = new List<int>();
        spline_startpoint_points = new List<Vector3>();
        spline_endpoint_points = new List<Vector3>();
        for (int seg_ind = 0; seg_ind < vertice_segments.Count; seg_ind++)
        {
            int first = 0;
            int last = vertice_segments[seg_ind].Count - numVertsPerPoint;
            spline_startpoint_indices.Add(vertex_counter);
            spline_endpoint_indices.Add(vertex_counter + last);
            spline_startpoint_points.Add(vertice_segments[seg_ind][first + middlePointVertexOffset]);
            spline_endpoint_points.Add(vertice_segments[seg_ind][last + middlePointVertexOffset]);
            vertices.AddRange(vertice_segments[seg_ind]);
            FillFaces_along_spline(vertex_counter + first, vertex_counter + last);

            vertex_counter += vertice_segments[seg_ind].Count;

        }
        ConnectEndpoints();
        mesh.SetVertices(vertices.ToArray());
        mesh.SetTriangles(tris.ToArray(), 0);
        mesh.RecalculateBounds();
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
    void FillFaces_stops_start(int offset)
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

    void FillFaces_stops_end(int offset)
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