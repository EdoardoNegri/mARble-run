using UnityEngine;

public class LowerLimit : MonoBehaviour
{
    public float size;
    public float y_position;

    void Awake()
    {
        GameObject lowerLimit = new GameObject("LowerLimit");

        lowerLimit.transform.position = new Vector3(0, y_position, 0);
        lowerLimit.transform.localScale = new Vector3(size, 1f, size);

        lowerLimit.AddComponent<DestroyOnTouch>();
        lowerLimit.AddComponent<BoxCollider>();
    }
}
