using UnityEngine;
using UnityEngine.Splines;
using System;
using System.Collections;
using System.Collections.Generic;
using MagicLeap.Examples;

public class Connect : MonoBehaviour
{
    // Start is called before the first frame update
    public float max_distance = 1.0f;
    //<Connector1, Connector2, Connecting Mesh
    private List<Tuple<GameObject, GameObject, GameObject>> PairedConnectors = new List<Tuple<GameObject, GameObject, GameObject>>();
    //private List<Tuple<GameObject, GameObject>> PairedConnectors = new List<Tuple<GameObject, GameObject>>();


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
                    GameObject meshObject = this.GetComponent<Draw>().addConnectorSegment(NewConnector, Connector);

                    // Remember to reactivate the connector
                    PairedConnectors.Add(new Tuple<GameObject, GameObject, GameObject>(NewConnector, Connector, meshObject));
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
                foreach (Tuple<GameObject, GameObject, GameObject> Pair in PairedConnectors)
                //foreach (Tuple<GameObject, GameObject> Pair in PairedConnectors)
                {
                if (NewConnector == Pair.Item1 || NewConnector == Pair.Item2)
                {
                    Pair.Item1.SetActive(true);
                    Pair.Item2.SetActive(true);
                    Destroy(Pair.Item3);
                    PairedConnectors.Remove(Pair);
                    break;
                }
            }
        }
    }
}
