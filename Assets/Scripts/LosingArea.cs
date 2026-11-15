using UnityEngine;

public sealed class LosingArea : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerController>() == null)
        {
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("LosingArea requires a GameManager in the active scene.", this);
            return;
        }

        GameManager.Instance.TryLose();
    }
}
