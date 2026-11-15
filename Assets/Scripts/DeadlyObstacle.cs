using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class DeadlyObstacle : MonoBehaviour
{
    private bool hasTriggered;

    private void OnCollisionEnter(Collision collision)
    {
        HandlePlayerContact(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        HandlePlayerContact(other);
    }

    private void HandlePlayerContact(Collider playerCollider)
    {
        if (hasTriggered)
        {
            return;
        }

        if (!playerCollider.CompareTag("Player") &&
            playerCollider.GetComponentInParent<PlayerController>() == null)
        {
            return;
        }

        GameManager manager = GameManager.Instance;
        if (manager == null)
        {
            Debug.LogError("DeadlyObstacle requires a GameManager in the active scene.", this);
            return;
        }

        if (manager.State != GameState.Running)
        {
            return;
        }

        hasTriggered = true;
        manager.TryLose();
    }
}
