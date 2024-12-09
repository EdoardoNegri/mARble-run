using UnityEngine;
using MagicLeap.OpenXR.Features;

namespace MagicLeap.Examples{
//this needs to be attached to the object you want to delete
    public class Eraser : MonoBehaviour
    {
        private bool isInteracting = false; // Tracks if the object is currently interactable
        private MagicLeapController controller;
        void Start()
        {
            // Register the object as interactable
            controller = MagicLeapController.Instance;
        }
        void Update()
        {
            // Check for button press while the object is interactable
            if (isInteracting && )
            {
                isInteracting = false;
                Destroy(gameObject);
            }
        }
    }
}