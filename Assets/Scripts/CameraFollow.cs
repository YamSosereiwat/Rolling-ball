using UnityEngine;

public sealed class CameraFollow : MonoBehaviour
{
    private readonly RaycastHit[] collisionHits = new RaycastHit[32];

    [SerializeField] private Transform target;
    [SerializeField, Min(0f)] private float distance = 6f;
    [SerializeField, Min(0f)] private float height = 2f;
    [SerializeField, Range(-10f, 45f)] private float pitch = 12f;
    [SerializeField] private float yaw;
    [SerializeField, Min(0f)] private float positionSmoothTime = 0.12f;
    [SerializeField, Min(0f)] private float rotationSmoothSpeed = 7f;
    [SerializeField, Min(0f)] private float movementDirectionThreshold = 0.2f;
    [SerializeField, Min(0f)] private float lookHeight = 0.5f;
    [SerializeField, Min(0.01f)] private float collisionRadius = 0.2f;
    [SerializeField] private LayerMask collisionMask = ~0;

    private Rigidbody targetBody;
    private Vector3 positionVelocity;
    private float currentYaw;
    private float desiredYaw;
    private float yawVelocity;

    private void Awake()
    {
        if (target == null)
        {
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                target = player.transform;
            }
        }

        if (target != null)
        {
            targetBody = target.GetComponent<Rigidbody>();
        }

        currentYaw = yaw;
        desiredYaw = yaw;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        if (targetBody == null)
        {
            targetBody = target.GetComponent<Rigidbody>();
        }

        if (targetBody != null)
        {
            Vector3 movement = Vector3.ProjectOnPlane(targetBody.linearVelocity, Vector3.up);
            if (movement.sqrMagnitude >= movementDirectionThreshold * movementDirectionThreshold)
            {
                desiredYaw = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg;
            }
        }

        float yawSmoothTime = 1f / Mathf.Max(rotationSmoothSpeed, 0.01f);
        currentYaw = Mathf.SmoothDampAngle(
            currentYaw,
            desiredYaw,
            ref yawVelocity,
            yawSmoothTime,
            Mathf.Infinity,
            Time.deltaTime);

        Vector3 focus = target.position + Vector3.up * lookHeight;
        Quaternion orbitRotation = Quaternion.Euler(pitch, currentYaw, 0f);
        Vector3 desiredOffset = orbitRotation * new Vector3(0f, 0f, -distance) + Vector3.up * height;
        Vector3 desiredPosition = focus + desiredOffset;
        Vector3 cameraToTarget = desiredPosition - focus;
        float desiredDistance = cameraToTarget.magnitude;
        bool obstructed = false;

        if (desiredDistance > 0f)
        {
            Vector3 direction = cameraToTarget / desiredDistance;
            int hitCount = Physics.SphereCastNonAlloc(
                focus,
                collisionRadius,
                direction,
                collisionHits,
                desiredDistance,
                collisionMask,
                QueryTriggerInteraction.Ignore);

            float closestDistance = desiredDistance;
            for (int i = 0; i < hitCount; i++)
            {
                if (collisionHits[i].transform == target || collisionHits[i].transform.IsChildOf(target))
                {
                    continue;
                }

                closestDistance = Mathf.Min(closestDistance, collisionHits[i].distance - collisionRadius);
            }

            obstructed = closestDistance < desiredDistance;
            desiredPosition = focus + direction * Mathf.Max(0.1f, closestDistance);
        }

        if (obstructed)
        {
            transform.position = desiredPosition;
            positionVelocity = Vector3.zero;
        }
        else
        {
            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref positionVelocity,
                positionSmoothTime);
        }

        Quaternion desiredRotation = Quaternion.LookRotation(focus - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            desiredRotation,
            rotationSmoothSpeed * Time.deltaTime);
    }
}
