using UnityEngine;
using UnityEngine.UI;

public class Spawner : MonoBehaviour
{
    public GameObject spawnObject;

    public void SpawnObject()
    {
        Instantiate(spawnObject, transform.position, transform.rotation);
    }
}