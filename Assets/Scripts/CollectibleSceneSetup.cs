using System;
using UnityEngine;

public sealed class CollectibleSceneSetup : MonoBehaviour
{
    [SerializeField] private string coinObjectNamePrefix = "coin-silver";
    [SerializeField, Min(1f)] private float triggerRadiusMultiplier = 1.1f;

    private void Start()
    {
        Transform[] sceneTransforms = FindObjectsByType<Transform>(FindObjectsSortMode.None);
        for (int i = 0; i < sceneTransforms.Length; i++)
        {
            Transform coin = sceneTransforms[i];
            if (!coin.gameObject.scene.IsValid() ||
                !coin.name.StartsWith(coinObjectNamePrefix, StringComparison.OrdinalIgnoreCase) ||
                (coin.parent != null &&
                 coin.parent.name.StartsWith(coinObjectNamePrefix, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            ConfigureCollectible(coin);
        }
    }

    private void ConfigureCollectible(Transform coin)
    {
        Renderer coinRenderer = coin.GetComponentInChildren<Renderer>();
        if (coinRenderer == null)
        {
            Debug.LogWarning("Cannot configure collectible without a Renderer.", coin);
            return;
        }

        SphereCollider trigger = coin.GetComponent<SphereCollider>();
        if (trigger == null)
        {
            trigger = coin.gameObject.AddComponent<SphereCollider>();
        }

        Bounds bounds = coinRenderer.bounds;
        Vector3 scale = coin.lossyScale;
        float largestScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
        trigger.center = coin.InverseTransformPoint(bounds.center);
        trigger.radius = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z) *
                         triggerRadiusMultiplier / Mathf.Max(largestScale, 0.0001f);
        trigger.isTrigger = true;

        if (coin.GetComponent<Collectible>() == null)
        {
            coin.gameObject.AddComponent<Collectible>();
        }

        if (coin.CompareTag("Untagged"))
        {
            coin.tag = "Collectible";
        }
    }
}
