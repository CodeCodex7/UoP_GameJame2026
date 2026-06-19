using UnityEngine;

public class PlaneCameraController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private float shiftMoveMultiplier = 2f;
    [SerializeField] private float rotationStep = 90f;
    [SerializeField] private float rotationSpeed = 540f;
    [SerializeField] private float zoomSpeed = 30f;
    [SerializeField] private float minZoomHeight = 6f;
    [SerializeField] private float maxZoomHeight = 40f;
    [SerializeField] private bool useBounds = true;
    [SerializeField] private Vector2 xBounds = new Vector2(-50f, 50f);
    [SerializeField] private Vector2 zBounds = new Vector2(-50f, 50f);

    private float targetYaw;

    private void Awake()
    {
        targetYaw = transform.eulerAngles.y;
    }

    private void Update()
    {
        HandleMovement();
        HandleZoom();
        HandleRotationInput();
        RotateTowardsTarget();
        ClampToBounds();
    }

    private void HandleMovement()
    {
        var input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

        if (input.sqrMagnitude <= 0f)
        {
            return;
        }

        input.Normalize();

        var forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        var right = transform.right;
        right.y = 0f;
        right.Normalize();

        var speedMultiplier = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)
            ? shiftMoveMultiplier
            : 1f;

        var movement = (right * input.x + forward * input.z) * moveSpeed * speedMultiplier * Time.deltaTime;
        transform.position += movement;
    }

    private void HandleRotationInput()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            targetYaw -= rotationStep;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            targetYaw += rotationStep;
        }
    }

    private void HandleZoom()
    {
        var scroll = Input.mouseScrollDelta.y;

        if (Mathf.Approximately(scroll, 0f))
        {
            return;
        }

        var nextPosition = transform.position + transform.forward * (scroll * zoomSpeed * Time.deltaTime);
        nextPosition.y = Mathf.Clamp(nextPosition.y, minZoomHeight, maxZoomHeight);
        transform.position = nextPosition;
    }

    private void ClampToBounds()
    {
        var position = transform.position;
        position.y = Mathf.Clamp(position.y, minZoomHeight, maxZoomHeight);

        if (useBounds)
        {
            position.x = Mathf.Clamp(position.x, xBounds.x, xBounds.y);
            position.z = Mathf.Clamp(position.z, zBounds.x, zBounds.y);
        }

        transform.position = position;
    }

    private void RotateTowardsTarget()
    {
        var currentRotation = transform.rotation;
        var targetRotation = Quaternion.Euler(transform.eulerAngles.x, targetYaw, transform.eulerAngles.z);
        transform.rotation = Quaternion.RotateTowards(currentRotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}
