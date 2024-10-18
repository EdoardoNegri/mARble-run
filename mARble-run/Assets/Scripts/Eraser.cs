using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;

public class VRObjectDeleter : MonoBehaviour
{
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor rayInteractor;
    public InputHelpers.Button deleteButton = InputHelpers.Button.PrimaryButton;
    public float deleteButtonThreshold = 0.1f;

    private void Update()
    {
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            var xrController = rayInteractor.GetComponent<XRBaseController>();
            if (xrController != null)
            {
                var inputDevice = xrController.inputDevice;
                InputHelpers.IsPressed(inputDevice, deleteButton, out bool isPressed, deleteButtonThreshold);
                if (isPressed)
                {
                    TryDeleteObject(hit);
                }
            }
        }
    }

    private void TryDeleteObject(RaycastHit hit)
    {
        if (hit.collider.gameObject.CompareTag("Route"))
        {
            GameObject hitObject = hit.collider.gameObject;
            Destroy(hitObject);
        }
    }
}