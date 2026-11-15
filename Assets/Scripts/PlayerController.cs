using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public sealed class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField, Min(0f)] private float moveSpeed = 7f;
    [SerializeField, Min(0f)] private float boostSpeed = 11f;
    [SerializeField, Min(0f)] private float acceleration = 18f;
    [SerializeField, Min(0f)] private float deceleration = 12f;
    [SerializeField, Min(0f)] private float airControl = 0.3f;
    [SerializeField, Min(0f)] private float rollTorque = 8f;
    [SerializeField] private LayerMask groundMask = ~0;

    [Header("Jump and ground detection")]
    [SerializeField, Min(0f)] private float jumpForce = 6f;
    [SerializeField, Min(0f)] private float groundCheckDistance = 0.08f;
    [SerializeField, Range(0f, 89f)] private float maximumGroundAngle = 55f;

    private Rigidbody body;
    private SphereCollider sphereCollider;
    private Vector2 moveInput;
    private float movementReferenceYaw;
    private bool hasMovementReference;
    private bool boostHeld;
    private bool jumpRequested;
    private bool isGrounded;
    private Vector3 groundNormal = Vector3.up;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        body.maxAngularVelocity = 20f;
    }

    private void Update()
    {
        if (!CanReceiveInput())
        {
            moveInput = Vector2.zero;
            hasMovementReference = false;
            boostHeld = false;
            jumpRequested = false;
            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            moveInput = Vector2.zero;
            hasMovementReference = false;
            boostHeld = false;
            jumpRequested = false;
            return;
        }

        float horizontal = 0f;
        float vertical = 0f;
        horizontal += keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? -1f : 0f;
        horizontal += keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f;
        vertical += keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? -1f : 0f;
        vertical += keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1f : 0f;

        Vector2 nextMoveInput = Vector2.ClampMagnitude(new Vector2(horizontal, vertical), 1f);
        if (nextMoveInput.sqrMagnitude <= 0f)
        {
            hasMovementReference = false;
        }
        else if (!hasMovementReference)
        {
            float cameraYaw = cameraTransform != null
                ? cameraTransform.eulerAngles.y
                : transform.eulerAngles.y;
            movementReferenceYaw = cameraYaw;
            hasMovementReference = true;
        }

        moveInput = nextMoveInput;
        boostHeld = keyboard.leftShiftKey.isPressed;
        jumpRequested = keyboard.spaceKey.wasPressedThisFrame;
    }

    private void FixedUpdate()
    {
        UpdateGroundedState();
        Move();

        if (jumpRequested && isGrounded && CanReceiveInput())
        {
            body.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }

        jumpRequested = false;
    }

    private void UpdateGroundedState()
    {
        Vector3 worldCenter = sphereCollider.transform.TransformPoint(sphereCollider.center);
        Vector3 scale = sphereCollider.transform.lossyScale;
        float radius = sphereCollider.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
        Vector3 origin = worldCenter + Vector3.up * (radius + 0.02f);
        float probeRadius = radius * 0.9f;
        float probeDistance = radius * 1.1f + groundCheckDistance;

        if (Physics.SphereCast(
                origin,
                probeRadius,
                Vector3.down,
                out RaycastHit hit,
                probeDistance,
                groundMask,
                QueryTriggerInteraction.Ignore) &&
            Vector3.Angle(hit.normal, Vector3.up) <= maximumGroundAngle)
        {
            isGrounded = true;
            groundNormal = hit.normal;
        }
        else
        {
            isGrounded = false;
            groundNormal = Vector3.up;
        }
    }

    private void Move()
    {
        float movementYaw = hasMovementReference
            ? movementReferenceYaw
            : cameraTransform != null
                ? cameraTransform.eulerAngles.y
                : transform.eulerAngles.y;
        Quaternion movementRotation = Quaternion.Euler(0f, movementYaw, 0f);
        Vector3 forward = movementRotation * Vector3.forward;
        Vector3 right = movementRotation * Vector3.right;

        Vector3 moveDirection = forward * moveInput.y + right * moveInput.x;
        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }

        Vector3 movementNormal = isGrounded ? groundNormal : Vector3.up;
        if (isGrounded && moveDirection.sqrMagnitude > 0f)
        {
            moveDirection = Vector3.ProjectOnPlane(moveDirection, groundNormal).normalized;
        }

        float maximumSpeed = boostHeld && moveInput.sqrMagnitude > 0f ? boostSpeed : moveSpeed;
        Vector3 currentPlanarVelocity = Vector3.ProjectOnPlane(body.linearVelocity, movementNormal);
        Vector3 targetVelocity = moveDirection * maximumSpeed;
        float response = moveInput.sqrMagnitude > 0f ? acceleration : deceleration;
        if (!isGrounded)
        {
            response *= airControl;
        }

        Vector3 accelerationToApply = Vector3.ClampMagnitude(
            (targetVelocity - currentPlanarVelocity) * response,
            response);
        body.AddForce(accelerationToApply, ForceMode.Acceleration);

        if (moveDirection.sqrMagnitude > 0f)
        {
            Vector3 torqueAxis = Vector3.Cross(Vector3.up, moveDirection);
            float torqueScale = isGrounded ? 1f : airControl;
            body.AddTorque(torqueAxis * rollTorque * torqueScale, ForceMode.Acceleration);
        }
    }

    private bool CanReceiveInput()
    {
        return GameManager.Instance == null || GameManager.Instance.State == GameState.Running;
    }
}
