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
    [SerializeField] private bool enableRightClickDrag = true;
    [SerializeField] private float dragPlaneHeight = 0f;
    [SerializeField] private bool useBounds = true;
    [SerializeField] private Vector2 xBounds = new Vector2(-50f, 50f);
    [SerializeField] private Vector2 zBounds = new Vector2(-50f, 50f);

    private float targetYaw;
    private Vector3 dragAnchorWorldPosition;
    private bool isRightDraggingCamera;

    private void Awake()
    {
        targetYaw = transform.eulerAngles.y;
    }

    private void Update()
    {
        HandleRightClickDrag();
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

    private void HandleRightClickDrag()
    {
        if (!enableRightClickDrag)
        {
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            isRightDraggingCamera = TryGetMousePointOnDragPlane(out dragAnchorWorldPosition);
            return;
        }

        if (Input.GetMouseButtonUp(1))
        {
            isRightDraggingCamera = false;
            return;
        }

        if (!isRightDraggingCamera || !Input.GetMouseButton(1))
        {
            return;
        }

        if (!TryGetMousePointOnDragPlane(out var currentWorldPosition))
        {
            return;
        }

        var movement = dragAnchorWorldPosition - currentWorldPosition;
        movement.y = 0f;
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

    private bool TryGetMousePointOnDragPlane(out Vector3 point)
    {
        var dragPlane = new Plane(Vector3.up, new Vector3(0f, dragPlaneHeight, 0f));
        var ray = Camera.main != null
            ? Camera.main.ScreenPointToRay(Input.mousePosition)
            : new Ray(transform.position, transform.forward);

        if (dragPlane.Raycast(ray, out var enter))
        {
            point = ray.GetPoint(enter);
            return true;
        }

        point = default;
        return false;
    }
}
