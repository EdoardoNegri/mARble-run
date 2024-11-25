using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seesaw : MonoBehaviour
{
    public void Disable()
    {
        Destroy(GetComponent<HingeJoint>());       
    }

    public void Enable()
    {
        HingeJoint hinge = gameObject.AddComponent<HingeJoint>();
        hinge.anchor = new Vector3(0, 0.0675f, 0);
        hinge.connectedAnchor = new Vector3(0, 0.00675f, 0);
        hinge.useLimits = true;
        JointLimits limits = hinge.limits;
        limits.min = -45f;
        limits.max = 45f;
        limits.bounciness = 0.5f;
        hinge.limits = limits;
        hinge.enableCollision = true;
    }
}
