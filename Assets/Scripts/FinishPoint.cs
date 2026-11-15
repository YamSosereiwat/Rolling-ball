using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class FinishPoint : MonoBehaviour
{
    private bool hasTriggered;

    private void Awake()
    {
        Collider finishCollider = GetComponent<Collider>();
        if (!CompareTag("Finish") || !finishCollider.enabled || !finishCollider.isTrigger)
        {
            Debug.LogError("FinishPoint requires the Finish tag and an enabled trigger Collider.", this);
            enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !CompareTag("Finish") ||
            other.GetComponentInParent<PlayerController>() == null)
        {
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("FinishPoint requires a GameManager in the active scene.", this);
            return;
        }

        if (GameManager.Instance.State != GameState.Running)
        {
            return;
        }

        hasTriggered = true;
        GameManager.Instance.TryWin();
    }
}
