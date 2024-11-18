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
    public GameObject rampPrefab; // Prefab for the line renderer
    
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

    //start and end point stores for connecting pieces
    List<int> spline_startpoint_indices = new List<int>();
    List<int> spline_endpoint_indices = new List<int>();
    List<Vector3> spline_startpoint_points = new List<Vector3>;
    List<Vector3> spline_endpoint_points = new List<Vector3>;

    //holds the mesh elements
    List<Vector3> vertices = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> tris = new List<int>();

    //number of vertices around each single point (dont change atm)
    int num_vertices_per_point = 4;
    //distance of vertices generated from the spline
    float width = 1.0;

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
        
    }


    void ConnectEndpoints(){
        //segment = new GameObject("Segment");
        //segment.tag = "Route";
        //Instantiate(new GameObject("Connector01"), currStartPoint, Quaternion.identity, segment.transform);
        //Instantiate(new GameObject("Connector02"), currEndPoint, Quaternion.identity , segment.transform);


        //We take the endpoints of the newly generated spline
        Vector3 new_startpoint = spline_startpoint_points[spline_startpoint_points.Count-1];
        int new_startpoint_index = spline_startpoint_indices.Last[spline_startpoint_indices.Count - 1];
        Vector3 new_endpoint = spline_endpoint_points[spline_endpoint_points.Count - 1];
        int new_endpoint_index = spline_endpoint_indices.Last[spline_endpoint_indices.Count - 1];

        int start_connection_index = -1;
        int end_connection_index = -1;

        //find another startpoint to connect new endpoint to (always start->end)
        float dist = float.MaxValue;

        for(int i = 0; i < spline_startpoint_points.Count-1; i++)
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
        if(start_connection_index >= 0)
        {
            FillFaces_single_step(start_connection_index, new_endpoint_index);
        }
        if (end_connection_index >= 0)
        {
            FillFaces_single_step(new_startpoint_index, end_connection_index);
        }


        /*
        //
        
        
        
        i need to have also a direction for the first point we want to connect, i can save two last points
        bool doubleConnect = false;
        foreach (var segment in segments)
        {
            GameObject startPoint = segment.transform.Find(startPoint);
            GameObject endPoint = segment.transform.Find(endPoint);

            if (Vector3.Distance(currStartPoint, startPoint.transform) <= maxDistToConnect)
            {
                spline.Add(new BezierKnot(startPoint.transform));
                startPoint.transform = currEndPoint;
                currSegment = segment;
                doubleConnect = true;
                break;
            }
            if (Vector3.Distance(currStartPoint, endPoint.transform) <= maxDistToConnect)
            {
                spline.Add(new BezierKnot(endPoint.transform));
                endPoint.transform = currEndPoint;
                currSegment = segment;
                doubleConnect = true;
                break;
            }
        }
        foreach (var pair in endPoints)
        {
            //if double connect then skip the currSegment
            if (Vector3.Distance(currEndPoint, startPoint.transform) <= maxDistToConnect)
            {
                spline.Add(new BezierKnot(startPoint.transform));
                startPoint.transform = currStartPoint;
                if(doubleConnect){
                    //transfer all the object instantiate in the other segment into this new one
                }
                currSegment = segment;
                break;
            }
            if (Vector3.Distance(currEndPoint, endPoint.transform) <= maxDistToConnect)
            {
                spline.Add(new BezierKnot(endPoint.transform));
                endPoint.transform = currStartPoint;
                currSegment = segment;
                if(doubleConnect){
                    //
                }
                currSegment = segment;
                break;
            }
        }
        */
        //if not connection is found then a new segment is created with an empty game objects named startPoint, endPoint.
    }


    

    void ConstructRoute(){
        float percentage = minDistance / spline.GetLength();
        int startpoint_index = vertices.size();
        int new_point_counter = 0;
        for (float t = 0f; t <= 1; t += percentage)
        {
            new_point_counter++;
            if(SplineUtility.Evaluate(spline, t, out float3 position, out float3 tangent, out float3 upVector))
            {   
                //Instantiate(rampPrefab, position, Quaternion.LookRotation(tangent), segment.transform);


                //atm Generate 4 vertices around the points in shape in a V shape
              
                Vector3 right = Vector3.Cross(tangent, upVector).normalized;

                vertices.Add(position + (right * width);
                vertices.Add(position + (-right * width);
                vertices.Add(position + (-upVector * width);
                vertices.Add(position + (-upVector * width * 1.5);

            }
        }
        //save the start/endpoints for connection later
        spline_startpoint_indices.Add(startpoint_index);
        spline_endpoint_indices.Add(startpoint_index + (num_vertices_per_point * new_point_counter);
    }


    //go around a spline and connect subsequent points)
    void FillFaces_along_spline(int startpoint_offset, int endpoint_offset)
    {
        for(int i = startpoint_offset; i < endpoint_offset; i += num_vertices_per_point)
        {
            FillFaces_single_step(i, i + 1);
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
            tris.Add(offset2 + i + 1);
            tris.Add(offset2 + i);
        }
        tris.Add(offset1 + numVertsPerPoint - 1);
        tris.Add(offset2 + numVertsPerPoint - 1);
        tris.Add(offset1);

        tris.Add(offset1);
        tris.Add(offset2);
        tris.Add(offset2 + numVertsPerPoint - 1);
    }
}