using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class SpringPad : MonoBehaviour
{
    [SerializeField, Min(0f)] private float bounceForce = 14f;

    private readonly HashSet<int> playerBodiesInside = new HashSet<int>();

    private void Awake()
    {
        Collider springCollider = GetComponent<Collider>();
        if (!springCollider.isTrigger)
        {
            Debug.LogError("SpringPad requires a trigger Collider.", this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null)
        {
            return;
        }

        Rigidbody playerBody = other.attachedRigidbody != null
            ? other.attachedRigidbody
            : player.GetComponent<Rigidbody>();
        if (playerBody == null)
        {
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Running)
        {
            return;
        }

        if (!playerBodiesInside.Add(playerBody.GetInstanceID()))
        {
            return;
        }

        Vector3 velocity = playerBody.linearVelocity;
        if (velocity.y < 0f)
        {
            velocity.y = 0f;
            playerBody.linearVelocity = velocity;
        }

        playerBody.AddForce(Vector3.up * bounceForce, ForceMode.Impulse);
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();
        Rigidbody playerBody = other.attachedRigidbody;
        if (playerBody == null && player != null)
        {
            playerBody = player.GetComponent<Rigidbody>();
        }

        if (playerBody != null)
        {
            playerBodiesInside.Remove(playerBody.GetInstanceID());
        }
    }
}
