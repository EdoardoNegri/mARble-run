using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;
using UnityEngine.Splines;
using Unity.Mathematics;
using UnityEngine.UI;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.NativeTypes;
using MagicLeap.OpenXR.Features;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Linq;

namespace MagicLeap.Examples{
public class VRPathDrawerReloaded : MonoBehaviour
{
    public float minDistance = 0.1f; // Minimum distance between points and construction points
    public float maxDistToConnect = 0.02f;
    private Spline spline;
    private Vector3 lastPoint;
    private bool isDrawing = false;
    //private List<GameObject> segments = new List<GameObject>();
    private GameObject segment;

    private Vector3 currStartPoint;
    private Vector3 currEndPoint;


    //Mesh Generation stuff

    private List<GameObject> meshObjectList = new List<GameObject>();
    private List<GameObject> connectionMeshObjectList = new List<GameObject>();
    private List<GameObject> drawingMeshObjectList = new List<GameObject>();

    private List<GameObject> connectorsStart = new List<GameObject>();
    private List<GameObject> connectorsEnd = new List<GameObject>();
    
    public Material mat;
    public Material connectorMat;
    public Material drawingMat; 

    //start and end point stores for connecting pieces
    //private List<int> spline_startpoint_indices = new List<int>();
    //private List<int> spline_endpoint_indices = new List<int>();
    //private List<Vector3> spline_startpoint_points = new List<Vector3>();
    //private List<Vector3> spline_endpoint_points = new List<Vector3>();

    private MagicLeapController controller;

    //holds the mesh elements
    private List<List<Vector3>> vertice_segments = new List<List<Vector3>>();
    

    //number of vertices around each single point (dont change atm)
    private int numVertsPerPoint = 9;
    private int middlePointVertexOffset = 3;
    //distance of vertices generated from the spline
    private float width = 0.05f;

    private MagicLeapRenderingExtensionsFeature rendering;

    void Start()
    {
        
        controller = MagicLeapController.Instance;

        rendering = OpenXRSettings.Instance.GetFeature<MagicLeapRenderingExtensionsFeature>();
        rendering.BlendMode = XrEnvironmentBlendMode.Additive;
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

            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            drawingMeshObjectList.Add(sphere);
            sphere.transform.position = currentPoint;
            sphere.GetComponent<MeshRenderer>().material = drawingMat;
            sphere.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);


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
        for (int i = 0; i < drawingMeshObjectList.Count; i++)
        {
            Destroy(drawingMeshObjectList[i]);
        }
        drawingMeshObjectList.Clear();
    }

    // void eraseSegment(Vector3 position)
    // {
    //     int seg_to_remove = -1;
    //     float dist = float.MaxValue;
    //     for (int i = 0; i < vertice_segments.Count; i++)
    //     {
    //         for (int j = 0; j < vertice_segments[i].Count; j++)
    //         {
    //             if (((float)Vector3.Distance(position, vertice_segments[i][j])) <= maxDistToErase)
    //             {
    //                 if (((float)Vector3.Distance(position, vertice_segments[i][j])) < dist)
    //                 {
    //                     dist = ((float)Vector3.Distance(position, vertice_segments[i][j]));
    //                     seg_to_remove = i;
                        

    //                 }
    //             }
    //         }
    //     }
        
    //     if (seg_to_remove >= 0)
    //     {
    //         vertice_segments.RemoveAt(seg_to_remove);
    //         ConstructRoute();
            
    //     }
    // }


    void ConnectEndpoints()
    {
            for (int s = 0; s < vertice_segments.Count; s++)
            {
                float dist = float.MaxValue;
                Vector3 start_point = vertice_segments[s][0 + middlePointVertexOffset];
                int end_connection_index = -1;
                for (int e = 0; e < vertice_segments.Count; e++)
                {
                    if (e != s)
                    {
                        Vector3 end_point = vertice_segments[e][vertice_segments[e].Count - numVertsPerPoint + middlePointVertexOffset];
                        if (((float)Vector3.Distance(start_point, end_point)) <= maxDistToConnect)
                        {

                            if (((float)Vector3.Distance(start_point, end_point)) < dist)
                            {
                                dist = ((float)Vector3.Distance(start_point, end_point));
                                end_connection_index = e;

                            }
                        }
                    }
                }
                if (end_connection_index >= 0)
                {
                    Mesh mesh = new Mesh();

                    List<Vector3> vertices = new List<Vector3>();
                    List<int> tris = new List<int>();




                    for (int i = 0; i < numVertsPerPoint; i++)
                    {
                        vertices.Add(vertice_segments[end_connection_index][vertice_segments[end_connection_index].Count - numVertsPerPoint + i]);
                    }
                    for (int i = 0; i < numVertsPerPoint; i++)
                    {
                        vertices.Add(vertice_segments[s][i]);
                    }


                    FillFaces_single_step(0, numVertsPerPoint, tris);
                    GameObject meshObject = new GameObject("Mesh Object", typeof(MeshRenderer), typeof(MeshFilter), typeof(Rigidbody), typeof(MeshCollider));
                    connectionMeshObjectList.Add(meshObject);
                    mesh.SetVertices(vertices.ToArray());
                    mesh.SetTriangles(tris.ToArray(), 0);

                    mesh.RecalculateNormals();

                    meshObject.GetComponent<MeshFilter>().mesh = mesh;
                    meshObject.GetComponent<MeshRenderer>().material = connectorMat;
                    meshObject.GetComponent<Rigidbody>().isKinematic = true;
                    meshObject.GetComponent<MeshCollider>().sharedMesh = mesh;


                }

            }

        }


    void addVerticeSegment()
    {
        if (spline.Knots.Count() < 2)
                return;

        List<Vector3> curr_vertice_segment = new List<Vector3>();
        float percentage = 0.05f;
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
            for (int i = 0; i < meshObjectList.Count; i++)
            {
                Destroy(meshObjectList[i]);
            }
            for (int i = 0; i < connectionMeshObjectList.Count; i++)
            {
                Destroy(connectionMeshObjectList[i]);
            }
            meshObjectList.Clear();
            connectionMeshObjectList.Clear();
            //meshes.Clear();



           // spline_startpoint_indices = new List<int>();
            //spline_endpoint_indices = new List<int>();
            //spline_startpoint_points = new List<Vector3>();
            //spline_endpoint_points = new List<Vector3>();
            for (int seg_ind = 0; seg_ind < vertice_segments.Count; seg_ind++)
            {
                List<Vector3> vertices = vertice_segments[seg_ind];
                //int first = 0;
                //int last = vertice_segments[seg_ind].Count;
                List<int> tris = new List<int>();

                FillFaces_along_spline(vertices, tris);




                GameObject meshObject = new GameObject("Mesh Object", typeof(MeshRenderer), typeof(MeshFilter), typeof(Rigidbody), typeof(MeshCollider));
                meshObjectList.Add(meshObject);
                Mesh mesh = new Mesh();


                mesh.SetVertices(vertices.ToArray());
                mesh.SetTriangles(tris.ToArray(), 0);

                mesh.RecalculateNormals();

                meshObject.GetComponent<MeshFilter>().mesh = mesh;
                meshObject.GetComponent<MeshRenderer>().material = mat;
                meshObject.GetComponent<Rigidbody>().isKinematic = true;
                meshObject.GetComponent<MeshCollider>().sharedMesh = mesh;


            }
            ConnectEndpoints();
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