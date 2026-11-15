using UnityEngine;

public sealed class Collectible : MonoBehaviour
{
    [SerializeField, Min(0)] private int scoreValue = 1;
    [SerializeField] private bool animate = true;
    [SerializeField, Min(0f)] private float rotationSpeed = 90f;
    [SerializeField, Min(0f)] private float floatAmplitude = 0.15f;
    [SerializeField, Min(0f)] private float floatFrequency = 2f;
    [SerializeField] private AudioClip collectSound;

    private Vector3 initialLocalPosition;
    private bool hasBeenCollected;

    private void OnEnable()
    {
        initialLocalPosition = transform.localPosition;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterCollectible(this);
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterCollectible(this);
        }
    }

    private void Update()
    {
        if (!animate || hasBeenCollected)
        {
            return;
        }

        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        Vector3 position = initialLocalPosition;
        position.y += Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.localPosition = position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenCollected || other.GetComponentInParent<PlayerController>() == null)
        {
            return;
        }

        GameManager manager = GameManager.Instance;
        if (manager == null || !manager.TryCollect(this, scoreValue))
        {
            return;
        }

        hasBeenCollected = true;
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }

        gameObject.SetActive(false);
    }
}
