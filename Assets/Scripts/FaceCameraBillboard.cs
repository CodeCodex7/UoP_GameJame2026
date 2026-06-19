using UnityEngine;

public class FaceCameraBillboard : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private bool lockToYAxis = true;
    [SerializeField] private bool flipForward = true;

    private void LateUpdate()
    {
        var cameraToUse = targetCamera != null ? targetCamera : Camera.main;

        if (cameraToUse == null)
        {
            return;
        }

        var direction = transform.position - cameraToUse.transform.position;

        if (lockToYAxis)
        {
            direction.y = 0f;
        }

        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        if (flipForward)
        {
            direction = -direction;
        }

        transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
    }
}
