using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seesaw : MonoBehaviour
{
    private HingeJoint hinge;
    void Start()
    {
         HingeJoint hinge = GetComponent<HingeJoint>();
    }
    public void Disable()
    {
        //hinge.enabled = false;
    }

    public void Enable()
    {
        //hinge.enabled = true;  
    }
}
