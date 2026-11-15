using UnityEngine;

public sealed class Obstacle : MonoBehaviour
{
    public enum MotionMode
    {
        None,
        Move,
        Rotate,
        FallOnTouch
    }

    [SerializeField] private MotionMode motionMode;
    [SerializeField] private Vector3 movementOffset = new Vector3(0f, 0f, 3f);
    [SerializeField, Min(0f)] private float movementSpeed = 1f;
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private bool lethalOnContact;

    private Vector3 startingPosition;
    private Quaternion startingRotation;
    private Rigidbody obstacleBody;
    private bool hasFallen;

    private void Awake()
    {
        startingPosition = transform.position;
        startingRotation = transform.rotation;
        obstacleBody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (motionMode == MotionMode.Move)
        {
            float movement = Mathf.PingPong(Time.time * movementSpeed, 1f);
            Vector3 position = Vector3.Lerp(startingPosition, startingPosition + movementOffset, movement);
            if (obstacleBody != null && obstacleBody.isKinematic)
            {
                obstacleBody.MovePosition(position);
            }
            else if (obstacleBody == null)
            {
                transform.position = position;
            }
        }
        else if (motionMode == MotionMode.Rotate && rotationAxis.sqrMagnitude > 0f)
        {
            Quaternion rotation = startingRotation *
                                  Quaternion.AngleAxis(rotationSpeed * Time.time, rotationAxis.normalized);
            if (obstacleBody != null && obstacleBody.isKinematic)
            {
                obstacleBody.MoveRotation(rotation);
            }
            else if (obstacleBody == null)
            {
                transform.rotation = rotation;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleContact(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleContact(other);
    }

    private void HandleContact(Collider other)
    {
        if (other.GetComponentInParent<PlayerController>() == null)
        {
            return;
        }

        if (lethalOnContact)
        {
            GameManager.Instance?.TryLose();
        }

        if (motionMode == MotionMode.FallOnTouch && !hasFallen)
        {
            if (obstacleBody == null)
            {
                Debug.LogError("FallOnTouch requires a Rigidbody on the obstacle.", this);
                return;
            }

            hasFallen = true;
            obstacleBody.isKinematic = false;
            obstacleBody.useGravity = true;
        }
    }
}
