using UnityEngine;
using UnityEngine.Splines;
using System;
using System.Collections;
using System.Collections.Generic;

public class Connect : MonoBehaviour
{
    // Start is called before the first frame update
    public const float max_distance = 1.0f;
    private List<Tuple<GameObject, GameObject>> PairedConnectors = new List<Tuple<GameObject, GameObject>>();

    public void ConnectPoints(GameObject obj)
    {
        // Find objects with tag "Connector" in the parent object
        List<GameObject> NewConnectors = new List<GameObject>();
        foreach (Transform child in obj.transform)
        {
            if (child.CompareTag("Connector"))
                NewConnectors.Add(child.gameObject);
        }

        // Find free connectors
        GameObject[] FreeConnectors = GameObject.FindGameObjectsWithTag("Connector");

        // Iterate over new connectors and check for pairs
        foreach (GameObject NewConnector in NewConnectors)
        {
            foreach (GameObject Connector in FreeConnectors)
            {
                float distance = Vector3.Distance(NewConnector.transform.position, Connector.transform.position);
                if (0 < distance && distance <= max_distance)
                {
                    // Initialize a new spline here
                    Spline spline = new Spline();
                    spline.Add(new BezierKnot(NewConnector.transform.position));
                    spline.Add(new BezierKnot(Connector.transform.position));
                    //MeshGeneration.buildTrack(new List<Spline>(spline));

                    // Remember to reactivate the connector
                    PairedConnectors.Add(new Tuple<GameObject, GameObject>(NewConnector, Connector));
                    NewConnector.SetActive(false);
                    Connector.SetActive(false);
                    break;
                }
            }
        }
    }

    public void DisconnectPoints(GameObject obj)
    {
        List<GameObject> NewConnectors = new List<GameObject>();
        foreach (Transform child in obj.transform)
        {
            if (child.CompareTag("Connector") && !child.gameObject.activeSelf)
                NewConnectors.Add(child.gameObject);
        }

        foreach (GameObject NewConnector in NewConnectors)
        {
            foreach (Tuple<GameObject, GameObject> Pair in PairedConnectors)
            {
                if (NewConnector == Pair.Item1 || NewConnector == Pair.Item2)
                {
                    Pair.Item1.SetActive(true);
                    Pair.Item2.SetActive(true);
                    PairedConnectors.Remove(Pair);
                    break;
                }
            }
        }
    }
}
