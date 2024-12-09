using UnityEngine;
using UnityEngine.UI;


public class Spawner : MonoBehaviour
{
    public GameObject spawnObject; // The object to instantiate

    public void SpawnObject()
    {
        Instantiate(spawnObject, transform.position + (transform.forward * 0.2f), transform.rotation);
    }
}