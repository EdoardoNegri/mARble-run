using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;
using UnityEngine.Splines;
using Unity.Mathematics;

public class VRPathDrawer : MonoBehaviour
{
    public XRNode controllerNode = XRNode.RightHand; // Choose which controller to use
    public float minDistance = 0.01f; // Minimum distance between points and construction points
    public float maxAngle = 45.0f; // Maximum angle between points and construction points
    public float maxDistToConnect = 0.02f;
    public GameObject rampPrefab; // Prefab for the line renderer
    
    private Spline spline;
    private Vector3 lastPoint;
    private Vector3 lastDirection;
    private bool isDrawing = false;
    //private List<GameObject> segments = new List<GameObject>();
    private GameObject segment;
    private Vector3 currStartPoint;
    private Vector3 currEndPoint;


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
        lastDirection = Vector3(0,0,0);
        //create connector
    }

    void ContinueDrawing()
    {
        //maybe add an indicator of where you are drawing
        Vector3 currentPoint = transform.position;
        if (Vector3.Distance(currentPoint, lastPoint) > minDistance)
        {
            Vector3 currdir = currentPoint - lastPoint;
            if (lastDirection == Vector(0,0,0) || Vector3.Angle(lastDirection, currdir) <= maxAngle) {
                spline.Add(new BezierKnot(currentPoint));
                lastPoint = currentPoint;
                lastDirection = currdir;
            }
        }
    }

    void StopDrawing()
    {
        isDrawing = false;
        currEndPoint = lastPoint;
        ConnectEndpoints();
        ConstructRoute();
    }


    void ConnectEndpoints(){
        segment = new GameObject("Segment");
        segment.tag = "Route";
        Instantiate(new GameObject("Connector01"), currStartPoint, Quaternion.identity, segment.transform);
        Instantiate(new GameObject("Connector02"), currEndPoint, Quaternion.identity , segment.transform);
        /*
        //i need to have also a direction for the first point we want to connect, i can save two last points
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
        //(Maybe) if the rate of change (second derivative) is bigger make the gap smaller to have a smoother curve
        float percentage = minDistance / spline.GetLength();
        for (float t = 0f; t <= 1; t += percentage)
        {
            if(SplineUtility.Evaluate(spline, t, out float3 position, out float3 tangent, out float3 upVector))
            {
                Instantiate(rampPrefab, position, Quaternion.LookRotation(tangent), segment.transform);
            }
        }
    }
}
