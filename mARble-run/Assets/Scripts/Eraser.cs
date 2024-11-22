using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

//this needs to be attached to the object you want to delete
public class VRObjectDeleter : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;
    public string buttonName = "Fire1"; // Replace with your input button name (e.g., "Submit" or "Fire1")
    private bool isInteracting = false; // Tracks if the object is currently interactable

    void Awake()
    {
        interactable.hoverEntered.AddListener(OnHoverEntered);
        interactable.hoverExited.AddListener(OnHoverExited);
    }

    void Update()
    {
        // Check for button press while the object is interactable
        if (isInteracting && Input.GetButtonDown(buttonName))
        {
            interactable.hoverEntered.RemoveListener(OnHoverEntered);
            interactable.hoverExited.RemoveListener(OnHoverExited);
            Destroy(gameObject);
        }
    }

    // Event Handlers
    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        isInteracting = true;
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        isInteracting = false;
    }
}