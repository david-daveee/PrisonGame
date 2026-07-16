using UnityEngine;

public class WorldItemDropper : MonoBehaviour
{
    [SerializeField] private Transform dropPoint;
    [SerializeField, Min(0.75f)] private float forwardDistance = 1.25f;
    [SerializeField, Min(0.005f)] private float floorOffset = 0.02f;
    [SerializeField, Min(0.5f)] private float groundProbeDistance = 4f;
    [SerializeField] private LayerMask groundLayers = ~0;
    [SerializeField] private LayerMask blockingLayers = ~0;

    private const int MaxOverlapResolutionSteps = 6;
    private const float CollisionSkin = 0.01f;

    public bool TrySpawn(
        InventoryItem inventoryItem,
        out WorldItem spawnedWorldItem)
    {
        spawnedWorldItem = null;

        if (inventoryItem?.ItemData == null)
        {
            Debug.LogError("Cannot drop an invalid InventoryItem.", this);
            return false;
        }

        WorldItem prefab = inventoryItem.ItemData.WorldPrefab;

        if (prefab == null)
        {
            Debug.LogWarning(
                $"ItemData '{inventoryItem.ItemData.name}' has no WorldPrefab.",
                inventoryItem.ItemData
            );
            return false;
        }

        if (dropPoint == null)
        {
            Debug.LogError(
                $"WorldItemDropper on '{name}' has no Drop Point assigned.",
                this
            );
            return false;
        }

        Vector3 forward = Vector3.ProjectOnPlane(
            dropPoint.forward,
            Vector3.up
        ).normalized;

        if (forward.sqrMagnitude < 0.01f)
        {
            forward = Vector3.ProjectOnPlane(
                transform.forward,
                Vector3.up
            ).normalized;
        }

        Vector3 probeOrigin = GetUnobstructedProbeOrigin(forward);

        if (!Physics.Raycast(
            probeOrigin,
            Vector3.down,
            out RaycastHit hit,
            groundProbeDistance,
            groundLayers,
            QueryTriggerInteraction.Ignore))
        {
            Debug.LogWarning(
                $"Could not find a surface below the drop point for " +
                $"'{inventoryItem.ItemData.DisplayName}'. The drop was cancelled.",
                this
            );
            return false;
        }

        Vector3 spawnPosition = hit.point + Vector3.up * floorOffset;
        Quaternion rotation = Quaternion.LookRotation(-forward, Vector3.up);
        WorldItem instance = Instantiate(prefab, spawnPosition, rotation);

        if (instance == null)
        {
            Debug.LogError("WorldItem prefab could not be instantiated.", this);
            return false;
        }

        if (!instance.Initialize(inventoryItem))
        {
            Destroy(instance.gameObject);
            Debug.LogError(
                $"Prefab '{prefab.name}' could not initialize its WorldItem.",
                prefab
            );
            return false;
        }

        SnapBottomToSurface(instance.gameObject, hit.point.y);

        if (!TryResolveBlockingOverlaps(instance.gameObject))
        {
            Destroy(instance.gameObject);
            Debug.LogWarning(
                $"Could not find enough free space to drop " +
                $"'{inventoryItem.ItemData.DisplayName}'. The drop was cancelled.",
                this
            );
            return false;
        }

        spawnedWorldItem = instance;
        return true;
    }

    public void DestroySpawned(WorldItem worldItem)
    {
        if (worldItem != null)
        {
            Destroy(worldItem.gameObject);
        }
    }

    private void OnValidate()
    {
        if (dropPoint == null)
        {
            Debug.LogWarning(
                $"WorldItemDropper on '{name}' needs a Drop Point.",
                this
            );
        }
    }

    private void SnapBottomToSurface(
        GameObject worldItem,
        float surfaceHeight)
    {
        if (!TryGetWorldBounds(worldItem, out Bounds bounds))
        {
            Debug.LogWarning(
                $"Dropped WorldItem '{worldItem.name}' has no enabled " +
                "Collider or Renderer for surface alignment.",
                worldItem
            );
            return;
        }

        float desiredBottom = surfaceHeight + floorOffset;
        float verticalCorrection = desiredBottom - bounds.min.y;
        worldItem.transform.position +=
            Vector3.up * verticalCorrection;
    }

    private Vector3 GetUnobstructedProbeOrigin(Vector3 forward)
    {
        Vector3 origin = dropPoint.position;

        if (!Physics.Raycast(
            origin,
            forward,
            out RaycastHit obstruction,
            forwardDistance,
            blockingLayers,
            QueryTriggerInteraction.Ignore))
        {
            return origin + forward * forwardDistance;
        }

        return obstruction.point - forward * CollisionSkin;
    }

    private bool TryResolveBlockingOverlaps(GameObject worldItem)
    {
        Collider itemCollider = GetPlacementCollider(worldItem);

        if (itemCollider == null)
        {
            return true;
        }

        for (int step = 0; step < MaxOverlapResolutionSteps; step++)
        {
            Physics.SyncTransforms();

            if (!TryGetHorizontalOverlapCorrection(
                worldItem,
                itemCollider,
                out Vector3 correction,
                out bool hasOverlap))
            {
                return false;
            }

            if (!hasOverlap)
            {
                return true;
            }

            worldItem.transform.position += correction;
        }

        Physics.SyncTransforms();
        return TryGetHorizontalOverlapCorrection(
            worldItem,
            itemCollider,
            out _,
            out bool remainingOverlap) &&
            !remainingOverlap;
    }

    private bool TryGetHorizontalOverlapCorrection(
        GameObject worldItem,
        Collider itemCollider,
        out Vector3 correction,
        out bool hasOverlap)
    {
        correction = Vector3.zero;
        hasOverlap = false;
        Bounds bounds = itemCollider.bounds;
        Collider[] overlaps = Physics.OverlapBox(
            bounds.center,
            bounds.extents,
            Quaternion.identity,
            blockingLayers,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider obstacle in overlaps)
        {
            if (obstacle == null ||
                obstacle.transform.IsChildOf(worldItem.transform))
            {
                continue;
            }

            if (!Physics.ComputePenetration(
                itemCollider,
                itemCollider.transform.position,
                itemCollider.transform.rotation,
                obstacle,
                obstacle.transform.position,
                obstacle.transform.rotation,
                out Vector3 separationDirection,
                out float separationDistance))
            {
                continue;
            }

            hasOverlap = true;
            Vector3 horizontalDirection = Vector3.ProjectOnPlane(
                separationDirection,
                Vector3.up
            );

            if (horizontalDirection.sqrMagnitude < 0.0001f)
            {
                return false;
            }

            correction = horizontalDirection.normalized *
                (separationDistance + CollisionSkin);
            return true;
        }

        return true;
    }

    private static Collider GetPlacementCollider(GameObject worldItem)
    {
        Collider[] colliders =
            worldItem.GetComponentsInChildren<Collider>();

        foreach (Collider currentCollider in colliders)
        {
            if (currentCollider.enabled && !currentCollider.isTrigger)
            {
                return currentCollider;
            }
        }

        return null;
    }

    private static bool TryGetWorldBounds(
        GameObject worldItem,
        out Bounds bounds)
    {
        Collider[] colliders =
            worldItem.GetComponentsInChildren<Collider>();

        foreach (Collider currentCollider in colliders)
        {
            if (!currentCollider.enabled || currentCollider.isTrigger)
            {
                continue;
            }

            bounds = currentCollider.bounds;

            foreach (Collider otherCollider in colliders)
            {
                if (otherCollider != currentCollider &&
                    otherCollider.enabled &&
                    !otherCollider.isTrigger)
                {
                    bounds.Encapsulate(otherCollider.bounds);
                }
            }

            return true;
        }

        Renderer[] renderers =
            worldItem.GetComponentsInChildren<Renderer>();

        foreach (Renderer currentRenderer in renderers)
        {
            if (!currentRenderer.enabled)
            {
                continue;
            }

            bounds = currentRenderer.bounds;

            foreach (Renderer otherRenderer in renderers)
            {
                if (otherRenderer != currentRenderer &&
                    otherRenderer.enabled)
                {
                    bounds.Encapsulate(otherRenderer.bounds);
                }
            }

            return true;
        }

        bounds = default;
        return false;
    }
}
