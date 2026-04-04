using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class HybrizoPuzzleController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HybrizoBoss bossEnemy;
    [SerializeField] private Michael michael;
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private GameObject barrelPrefab;
    [SerializeField] private BossObjectiveItem bossObjectiveItemPrefab;
    [Tooltip("Optional parent for spawned barrels and drops. Leave empty to parent spawns to this controller.")]
    [SerializeField] private Transform spawnParent;

    [Header("Puzzle Cycle")]
    [SerializeField] [Min(2)] private int barrelsPerCycle = 6;   // x
    [SerializeField] [Min(1)] private int dropsPerCycle = 3;     // y
    [SerializeField] [Min(0f)] private float weakWindowDurationSeconds = 8f;

    [Header("Placement")]
    [SerializeField] private bool avoidDuplicateSpawnTiles = true;
    [SerializeField] [Min(1)] private int maxPlacementAttemptsPerBarrel = 30;
    [SerializeField] private float spawnYOffset = 0f;

    private readonly List<GameObject> spawnedBarrels = new List<GameObject>();
    private Coroutine cycleRoutine;
    private bool puzzleIsActive;
    private int collectedDropsThisCycle;
    private int requiredDropsThisCycle;

    private void OnEnable()
    {
        ResolveReferences();
        BossObjectiveItem.BossCollected += HandleBossObjectiveCollected;
    }

    private void Start()
    {
        ResolveReferences();
        StartPuzzleCycle();
    }

    private void OnDisable()
    {
        BossObjectiveItem.BossCollected -= HandleBossObjectiveCollected;
    }

    private void OnValidate()
    {
        barrelsPerCycle = Mathf.Max(2, barrelsPerCycle);
        dropsPerCycle = Mathf.Clamp(dropsPerCycle, 1, barrelsPerCycle - 1);
        maxPlacementAttemptsPerBarrel = Mathf.Max(1, maxPlacementAttemptsPerBarrel);
        weakWindowDurationSeconds = Mathf.Max(0f, weakWindowDurationSeconds);
    }

    private void HandleBossObjectiveCollected(BossObjectiveItem collectedItem, Michael collectedBy)
    {
        if (!puzzleIsActive || collectedItem == null || collectedBy == null || collectedBy.IsDead)
        {
            return;
        }

        Transform root = spawnParent != null ? spawnParent : transform;
        if (!collectedItem.transform.IsChildOf(root))
        {
            return;
        }

        collectedDropsThisCycle++;
        if (collectedDropsThisCycle < requiredDropsThisCycle)
        {
            return;
        }

        CompletePuzzleCycle();
    }

    private void CompletePuzzleCycle()
    {
        if (!puzzleIsActive)
        {
            return;
        }

        puzzleIsActive = false;
        DestroySpawnedBarrels();

        if (bossEnemy != null)
        {
            bossEnemy.OnPuzzleSolved(weakWindowDurationSeconds);
        }

        if (cycleRoutine != null)
        {
            StopCoroutine(cycleRoutine);
        }

        cycleRoutine = StartCoroutine(BeginNextCycleAfterCooldown());
    }

    private IEnumerator BeginNextCycleAfterCooldown()
    {
        yield return new WaitForSeconds(weakWindowDurationSeconds);
        StartPuzzleCycle();
        cycleRoutine = null;
    }

    private void StartPuzzleCycle()
    {
        ClearAllBossDrops();
        DestroySpawnedBarrels();

        collectedDropsThisCycle = 0;
        requiredDropsThisCycle = 0;
        puzzleIsActive = true;

        if (bossEnemy != null)
        {
            bossEnemy.EnterPuzzleActiveState();
        }

        SpawnPuzzleBarrels();
    }

    private void SpawnPuzzleBarrels()
    {
        if (barrelPrefab == null || bossObjectiveItemPrefab == null)
        {
            Debug.LogWarning("HybrizoPuzzleController missing barrel or BossObjectiveItem prefab.");
            return;
        }

        List<Vector2> floorPositions = GetFloorPositionsForSpawning();
        if (floorPositions.Count == 0)
        {
            Debug.LogWarning("HybrizoPuzzleController could not resolve any floor positions for barrel spawning.");
            return;
        }

        HashSet<Vector2> blockedPositions = new HashSet<Vector2>();
        if (avoidDuplicateSpawnTiles && michael != null)
        {
            Vector2 blocked = michael.transform.position;
            if (groundTilemap != null)
            {
                Vector3Int playerCell = groundTilemap.WorldToCell(michael.transform.position);
                blocked = groundTilemap.GetCellCenterWorld(playerCell);
            }

            blockedPositions.Add(blocked);
        }

        List<WorldObject> spawnedWorldObjects = new List<WorldObject>();
        Transform parent = spawnParent != null ? spawnParent : transform;
        int toSpawn = Mathf.Min(barrelsPerCycle, floorPositions.Count);

        for (int i = 0; i < toSpawn; i++)
        {
            if (!TryGetSpawnPosition(floorPositions, blockedPositions, out Vector2 spawnPosition))
            {
                break;
            }

            Vector3 worldPosition = new Vector3(spawnPosition.x, spawnPosition.y + spawnYOffset, 0f);
            GameObject spawned = Instantiate(barrelPrefab, worldPosition, Quaternion.identity, parent);
            spawnedBarrels.Add(spawned);

            if (spawned.TryGetComponent<WorldObject>(out WorldObject worldObject))
            {
                spawnedWorldObjects.Add(worldObject);
            }

            if (avoidDuplicateSpawnTiles)
            {
                blockedPositions.Add(spawnPosition);
            }
        }

        ConfigurePuzzleDrops(spawnedWorldObjects);
    }

    private void ConfigurePuzzleDrops(List<WorldObject> spawnedWorldObjects)
    {
        if (spawnedWorldObjects == null || spawnedWorldObjects.Count == 0)
        {
            requiredDropsThisCycle = 0;
            return;
        }

        int dropCount = Mathf.Clamp(dropsPerCycle, 1, Mathf.Max(1, spawnedWorldObjects.Count - 1));
        List<int> indices = Enumerable.Range(0, spawnedWorldObjects.Count).ToList();

        // Fisher-Yates shuffle
        for (int i = indices.Count - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);
            int temp = indices[i];
            indices[i] = indices[swapIndex];
            indices[swapIndex] = temp;
        }

        HashSet<int> dropIndices = new HashSet<int>(indices.Take(dropCount));
        for (int i = 0; i < spawnedWorldObjects.Count; i++)
        {
            if (spawnedWorldObjects[i] == null) continue;
            spawnedWorldObjects[i].ConfigureObjectiveDrop(dropIndices.Contains(i) ? bossObjectiveItemPrefab.gameObject : null);
        }

        requiredDropsThisCycle = dropIndices.Count;
    }

    private bool TryGetSpawnPosition(List<Vector2> floorPositions, HashSet<Vector2> blockedPositions, out Vector2 spawnPosition)
    {
        spawnPosition = Vector2.zero;
        if (floorPositions == null || floorPositions.Count == 0)
        {
            return false;
        }

        for (int attempt = 0; attempt < maxPlacementAttemptsPerBarrel; attempt++)
        {
            Vector2 candidate = floorPositions[Random.Range(0, floorPositions.Count)];
            if (avoidDuplicateSpawnTiles && blockedPositions.Contains(candidate))
            {
                continue;
            }

            spawnPosition = candidate;
            return true;
        }

        return false;
    }

    private List<Vector2> GetFloorPositionsForSpawning()
    {
        if (groundTilemap != null)
        {
            List<Vector2> tilePositions = new List<Vector2>();
            BoundsInt bounds = groundTilemap.cellBounds;

            foreach (Vector3Int cellPosition in bounds.allPositionsWithin)
            {
                if (!groundTilemap.HasTile(cellPosition))
                {
                    continue;
                }

                Vector3 worldCenter = groundTilemap.GetCellCenterWorld(cellPosition);
                tilePositions.Add(new Vector2(worldCenter.x, worldCenter.y));
            }

            if (tilePositions.Count > 0)
            {
                return tilePositions;
            }
        }

        // Fallback in case the tilemap is not wired yet.
        Vector2 center = bossEnemy != null ? (Vector2)bossEnemy.transform.position : (Vector2)transform.position;
        List<Vector2> fallback = new List<Vector2>();
        for (int x = -3; x <= 3; x++)
        {
            for (int y = -3; y <= 3; y++)
            {
                fallback.Add(new Vector2(Mathf.Round(center.x) + x, Mathf.Round(center.y) + y));
            }
        }

        return fallback;
    }

    private void ResolveReferences()
    {
        if (bossEnemy == null)
        {
            bossEnemy = FindFirstObjectByType<HybrizoBoss>();
        }

        if (michael == null)
        {
            michael = FindFirstObjectByType<Michael>();
        }

        if (groundTilemap == null)
        {
            Tilemap[] tilemaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
            for (int i = 0; i < tilemaps.Length; i++)
            {
                Tilemap candidate = tilemaps[i];
                if (candidate == null) continue;
                if (candidate.name.Contains("Ground"))
                {
                    groundTilemap = candidate;
                    break;
                }
            }
        }
    }

    private void DestroySpawnedBarrels()
    {
        for (int i = spawnedBarrels.Count - 1; i >= 0; i--)
        {
            if (spawnedBarrels[i] != null)
            {
                Destroy(spawnedBarrels[i]);
            }
        }

        spawnedBarrels.Clear();
    }

    private void ClearAllBossDrops()
    {
        Transform root = spawnParent != null ? spawnParent : transform;
        List<BossObjectiveItem> activeDrops = root.GetComponentsInChildren<BossObjectiveItem>(includeInactive: true).ToList();
        for (int i = 0; i < activeDrops.Count; i++)
        {
            if (activeDrops[i] != null)
            {
                Destroy(activeDrops[i].gameObject);
            }
        }
    }
}
