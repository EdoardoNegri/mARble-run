using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target;
    public float xOffset;
    public float yOffsetMultiplier;
    public float zOffset;
    public Vector3 rotation;

    void Start()
    {
        Vector3 offset = new Vector3(xOffset, 0, zOffset);
        offset = target.TransformDirection(offset);
        xOffset = offset.x;
        zOffset = offset.z;
        transform.rotation = Quaternion.Euler(rotation);
    }
    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(
            target.position.x + xOffset,
            target.position.y * yOffsetMultiplier,
            target.position.z + zOffset
        );
    }
}
