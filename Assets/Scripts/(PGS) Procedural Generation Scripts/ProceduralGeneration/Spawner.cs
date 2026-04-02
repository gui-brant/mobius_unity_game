using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public static int RoomClearCount { get; private set; }

    [Header("References")]
    [SerializeField] private SimpleRandomWalkDungeonGenerator dungeonGenerator;
    [SerializeField] private Michael michael;
    [SerializeField] private Transform playerTransform;

    [Header("Room Prefab Pool")]
    [SerializeField] private List<GameObject> prefabsToSpawn = new List<GameObject>();
    [SerializeField] private GameObject objectiveItemPrefab;

    [Header("Per-Room Spawn Rules")]
    [SerializeField] [Min(1)] private int minRoomSpawnCap = 10;
    [SerializeField] [Min(1)] private int maxRoomSpawnCap = 20;
    [SerializeField] [Min(0)] private int minWorldObjectSpawns = 5;
    [SerializeField] [Min(0)] private int maxWorldObjectSpawns = 15;
    [SerializeField] [Min(1)] private int roomsBeforeReturningToHub = 3;
        
    [Header("Placement")]
    [SerializeField] private bool avoidDuplicateSpawnTiles = true;
    [SerializeField] private int maxPlacementAttemptsPerObject = 20;
    [SerializeField] private float spawnYOffset = 0f;

    private readonly List<GameObject> spawnedObjects = new List<GameObject>();
    private readonly List<Enemy> activeRoomEnemies = new List<Enemy>();

    private IReadOnlyCollection<Vector2> pendingFloorPositions;
    private bool hasSpawnedForCurrentRoom;
    private bool objectiveCollectedForCurrentRoom;
    private bool isTransitioningRoom;
    private int currentRoomSpawnCap;

    public static void ResetRoomClearProgress()
    {
        RoomClearCount = 0;
    }

    private void OnEnable()
    {
            ResolveReferences();
            SubscribeToGenerator();
            ObjectiveItem.Collected += HandleObjectiveCollected;
    }

    private void Start()
    {
        NormalizeSpawnConfiguration();
        ResolveReferences();
        CacheExistingGeneratedRoom();

        if ((pendingFloorPositions == null || pendingFloorPositions.Count == 0) && dungeonGenerator != null)
        {
            dungeonGenerator.GenerateDungeon();
        }

        TrySpawnWhenReady();
    }

    private void Update()
    {
        ResolvePlayerReference();

        if (!hasSpawnedForCurrentRoom)
        {
            TrySpawnWhenReady();
            return;
        }

        CleanupDefeatedEnemies();
        TryAdvanceToNextRoom();
    }

    private void OnDisable()
    {
        ObjectiveItem.Collected -= HandleObjectiveCollected;
        UnsubscribeFromGenerator();
    }

    private void ResolveReferences()
    {
        if (dungeonGenerator == null)
        {
            dungeonGenerator = FindFirstObjectByType<SimpleRandomWalkDungeonGenerator>();
        }

        ResolvePlayerReference();
    }

    private void ResolvePlayerReference()
    {
        if (michael == null || !michael.gameObject.activeInHierarchy || michael.IsDead)
        {
            Michael[] candidates = FindObjectsByType<Michael>(FindObjectsSortMode.None);
            Michael fallback = null;
            for (int i = 0; i < candidates.Length; i++)
            {
                Michael candidate = candidates[i];
                if (candidate == null || !candidate.gameObject.activeInHierarchy) continue;
                if (!candidate.IsDead)
                {
                    michael = candidate;
                    break;
                }

                if (fallback == null)
                {
                    fallback = candidate;
                }
            }

            if (michael == null)
            {
                michael = fallback;
            }
        }

        if (michael != null)
        {
            playerTransform = michael.transform;
            return;
        }

        if (playerTransform == null || !playerTransform.gameObject.activeInHierarchy)
        {
            GameObject playerByTag = GameObject.FindGameObjectWithTag("Player");
            if (playerByTag != null)
            {
                playerTransform = playerByTag.transform;
            }
        }
    }

    private void SubscribeToGenerator()
    {
        if (dungeonGenerator != null)
        {
            dungeonGenerator.DungeonGenerated += HandleDungeonGenerated;
        }
    }

    private void UnsubscribeFromGenerator()
    {
        if (dungeonGenerator != null)
        {
            dungeonGenerator.DungeonGenerated -= HandleDungeonGenerated;
        }
    }

    private void CacheExistingGeneratedRoom()
    {
        if (dungeonGenerator == null)
        {
            return;
        }

        if (dungeonGenerator.LastGeneratedFloorPositions != null && dungeonGenerator.LastGeneratedFloorPositions.Count > 0)
        {
            pendingFloorPositions = dungeonGenerator.LastGeneratedFloorPositions;
        }
    }

    private void HandleDungeonGenerated(IReadOnlyCollection<Vector2> floorPositions)
    {
        ClearPreviouslySpawnedObjects();
        activeRoomEnemies.Clear();

        hasSpawnedForCurrentRoom = false;
        objectiveCollectedForCurrentRoom = false;
        isTransitioningRoom = false;

        pendingFloorPositions = floorPositions;
        PlacePlayerInRoom(floorPositions);
        TrySpawnWhenReady();
    }

    private void HandleObjectiveCollected(ObjectiveItem objectiveItem, Michael collectedBy)
    {
        if (objectiveItem == null || collectedBy == null || collectedBy.IsDead)
        {
            return;
        }

        if (!objectiveItem.transform.IsChildOf(transform))
        {
            return;
        }

        if (objectiveItem is not Map)
        {
            return;
        }

        objectiveCollectedForCurrentRoom = true;
        TryAdvanceToNextRoom();
    }

    private void TrySpawnWhenReady()
    {
        if (hasSpawnedForCurrentRoom)
        {
            return;
        }

        if (playerTransform == null || pendingFloorPositions == null || pendingFloorPositions.Count == 0)
        {
            return;
        }

        if (prefabsToSpawn == null || prefabsToSpawn.Count == 0)
        {
            return;
        }

        currentRoomSpawnCap = Random.Range(
            Mathf.Min(minRoomSpawnCap, maxRoomSpawnCap),
            Mathf.Max(minRoomSpawnCap, maxRoomSpawnCap) + 1
        );

        SpawnPrefabsInRoom(pendingFloorPositions);
        hasSpawnedForCurrentRoom = true;
        TryAdvanceToNextRoom();
    }

    private void SpawnPrefabsInRoom(IReadOnlyCollection<Vector2> floorPositions)
    {
        List<Vector2> floorList = floorPositions.ToList();
        if (floorList.Count == 0)
        {
            return;
        }

        HashSet<Vector2> blockedPositions = new HashSet<Vector2>();
        List<WorldObject> spawnedWorldObjects = new List<WorldObject>();
        if (avoidDuplicateSpawnTiles && playerTransform != null)
        {
            Vector2 playerTile = new Vector2(
                Mathf.Round(playerTransform.position.x),
                Mathf.Round(playerTransform.position.y)
            );
            blockedPositions.Add(playerTile);
        }

        int remainingCapacity = avoidDuplicateSpawnTiles
            ? Mathf.Min(currentRoomSpawnCap, floorList.Count)
            : currentRoomSpawnCap;

        if (remainingCapacity <= 0)
        {
            return;
        }

        List<GameObject> healingPrefabs = prefabsToSpawn.Where(IsHealingItemPrefab).ToList();
        List<GameObject> armorPrefabs = prefabsToSpawn.Where(IsArmorItemPrefab).ToList();
        List<GameObject> worldObjectPrefabs = prefabsToSpawn.Where(IsWorldObjectPrefab).ToList();
        List<GameObject> enemyPrefabs = prefabsToSpawn.Where(IsEnemyPrefab).ToList();
        List<GameObject> otherPrefabs = prefabsToSpawn.Where(p =>
            p != null &&
            !IsWeaponPrefab(p) &&
            !IsHealingItemPrefab(p) &&
            !IsArmorItemPrefab(p) &&
            !IsWorldObjectPrefab(p) &&
            !IsEnemyPrefab(p)
        ).ToList();

        remainingCapacity = TrySpawnOneFromPool(healingPrefabs, floorList, blockedPositions, remainingCapacity);
        remainingCapacity = TrySpawnOneFromPool(armorPrefabs, floorList, blockedPositions, remainingCapacity);

        if (remainingCapacity <= 0)
        {
            return;
        }

        int requestedWorldObjectCount = Random.Range(
            Mathf.Min(minWorldObjectSpawns, maxWorldObjectSpawns),
            Mathf.Max(minWorldObjectSpawns, maxWorldObjectSpawns) + 1
        );
        int worldObjectCount = Mathf.Min(requestedWorldObjectCount, remainingCapacity);

        for (int i = 0; i < worldObjectCount; i++)
        {
            if (TrySpawnFromPool(worldObjectPrefabs, floorList, blockedPositions, out GameObject spawnedWorldObject))
            {
                remainingCapacity--;
                if (spawnedWorldObject != null &&
                    spawnedWorldObject.TryGetComponent<WorldObject>(out WorldObject worldObject))
                {
                    spawnedWorldObjects.Add(worldObject);
                }
            }
        }

        if (remainingCapacity <= 0)
        {
            return;
        }

        int enemyCount = enemyPrefabs.Count > 0
            ? Random.Range(1, remainingCapacity + 1)
            : 0;

        for (int i = 0; i < enemyCount; i++)
        {
            if (TrySpawnFromPool(enemyPrefabs, floorList, blockedPositions, out _))
            {
                remainingCapacity--;
            }
        }

        for (int i = 0; i < remainingCapacity; i++)
        {
            if (!TrySpawnFromPool(otherPrefabs, floorList, blockedPositions, out _))
            {
                break;
            }
        }

        ConfigureSingleObjectiveMapDrop(spawnedWorldObjects, floorList, blockedPositions);
    }

    private int TrySpawnOneFromPool(
        List<GameObject> prefabPool,
        List<Vector2> floorList,
        HashSet<Vector2> blockedPositions,
        int remainingCapacity)
    {
        if (remainingCapacity <= 0)
        {
            return remainingCapacity;
        }

        if (TrySpawnFromPool(prefabPool, floorList, blockedPositions, out _))
        {
            return remainingCapacity - 1;
        }

        return remainingCapacity;
    }

    private bool TrySpawnFromPool(
        List<GameObject> prefabPool,
        List<Vector2> floorList,
        HashSet<Vector2> blockedPositions,
        out GameObject spawnedObject)
    {
        spawnedObject = null;

        if (prefabPool == null || prefabPool.Count == 0)
        {
            return false;
        }

        if (!TryGetSpawnPosition(floorList, blockedPositions, out Vector2 spawnPosition))
        {
            return false;
        }

        GameObject prefab = prefabPool[Random.Range(0, prefabPool.Count)];
        if (prefab == null)
        {
            return false;
        }

        Vector3 worldPosition = new Vector3(spawnPosition.x, spawnPosition.y + spawnYOffset, 0f);
        GameObject spawned = Instantiate(prefab, worldPosition, Quaternion.identity, transform);
        spawnedObjects.Add(spawned);
        spawnedObject = spawned;

        if (spawned.TryGetComponent<Enemy>(out Enemy enemy))
        {
            activeRoomEnemies.Add(enemy);
        }

        if (avoidDuplicateSpawnTiles)
        {
            blockedPositions.Add(spawnPosition);
        }

        return true;
    }

    private bool TryGetSpawnPosition(List<Vector2> floorList, HashSet<Vector2> blockedPositions, out Vector2 spawnPosition)
    {
        spawnPosition = Vector2.zero;

        for (int attempt = 0; attempt < maxPlacementAttemptsPerObject; attempt++)
        {
            Vector2 candidate = floorList[Random.Range(0, floorList.Count)];

            if (avoidDuplicateSpawnTiles && blockedPositions.Contains(candidate))
            {
                continue;
            }

            spawnPosition = candidate;
            return true;
        }

        return false;
    }

    private void CleanupDefeatedEnemies()
    {
        for (int i = activeRoomEnemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = activeRoomEnemies[i];
            if (enemy == null || enemy.IsDead)
            {
                activeRoomEnemies.RemoveAt(i);
            }
        }
    }

    private void TryAdvanceToNextRoom()
    {
        if (!hasSpawnedForCurrentRoom || isTransitioningRoom)
        {
            return;
        }

        bool allEnemiesDefeated = activeRoomEnemies.Count == 0;
        if (!allEnemiesDefeated || !objectiveCollectedForCurrentRoom)
        {
            return;
        }

        isTransitioningRoom = true;
        RoomClearCount++;

        if (RoomClearCount >= roomsBeforeReturningToHub)
        {
            Debug.Log("You win");
            ResetRoomClearProgress();
            MoveScene moveScene = FindFirstObjectByType<MoveScene>();
            if (moveScene != null)
            {
                moveScene.StartCoroutine(moveScene.MoveBackToSample());
            }
            return;
        }

        if (dungeonGenerator != null)
        {
            dungeonGenerator.GenerateDungeon();
        }
        else
        {
            isTransitioningRoom = false;
        }
    }

    private void PlacePlayerInRoom(IReadOnlyCollection<Vector2> floorPositions)
    {
        if (playerTransform == null || floorPositions == null || floorPositions.Count == 0)
        {
            return;
        }

        List<Vector2> floorList = floorPositions.ToList();
        Vector2 spawnTile = floorList[Random.Range(0, floorList.Count)];
        Vector3 playerPosition = playerTransform.position;
        playerTransform.position = new Vector3(spawnTile.x, spawnTile.y, playerPosition.z);
    }

    private bool IsWeaponPrefab(GameObject prefab)
    {
        return prefab != null && prefab.GetComponent<Weapon>() != null;
    }

    private bool IsHealingItemPrefab(GameObject prefab)
    {
        return prefab != null && prefab.GetComponent<HealingItem>() != null;
    }

    private bool IsArmorItemPrefab(GameObject prefab)
    {
        return prefab != null && prefab.GetComponent<ArmorItem>() != null;
    }

    private bool IsWorldObjectPrefab(GameObject prefab)
    {
        return prefab != null && prefab.GetComponent<WorldObject>() != null;
    }

    private bool IsEnemyPrefab(GameObject prefab)
    {
        return prefab != null && prefab.GetComponent<Enemy>() != null;
    }

    private void ClearPreviouslySpawnedObjects()
    {
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (spawnedObjects[i] != null)
            {
                Destroy(spawnedObjects[i]);
            }
        }

        spawnedObjects.Clear();
    }

    private void OnValidate()
    {
        NormalizeSpawnConfiguration();
    }

    private void NormalizeSpawnConfiguration()
    {
        minWorldObjectSpawns = Mathf.Clamp(minWorldObjectSpawns, 5, 15);
        maxWorldObjectSpawns = Mathf.Clamp(maxWorldObjectSpawns, 5, 15);
        if (maxWorldObjectSpawns < minWorldObjectSpawns)
        {
            maxWorldObjectSpawns = minWorldObjectSpawns;
        }

        // Room cap must support: 1 healing + 1 armor + at least 5 world objects + at least 1 enemy.
        minRoomSpawnCap = Mathf.Max(8, minRoomSpawnCap);
        maxRoomSpawnCap = Mathf.Max(minRoomSpawnCap, maxRoomSpawnCap);
        roomsBeforeReturningToHub = Mathf.Max(1, roomsBeforeReturningToHub);
    }

    private void ConfigureSingleObjectiveMapDrop(
        List<WorldObject> spawnedWorldObjects,
        List<Vector2> floorList,
        HashSet<Vector2> blockedPositions)
    {
        if (objectiveItemPrefab == null)
        {
            return;
        }

        if (spawnedWorldObjects != null && spawnedWorldObjects.Count > 0)
        {
            int selectedIndex = Random.Range(0, spawnedWorldObjects.Count);
            for (int i = 0; i < spawnedWorldObjects.Count; i++)
            {
                if (spawnedWorldObjects[i] == null) continue;
                spawnedWorldObjects[i].ConfigureObjectiveDrop(i == selectedIndex ? objectiveItemPrefab : null);
            }
            return;
        }

        if (!TryGetSpawnPosition(floorList, blockedPositions, out Vector2 spawnPosition))
        {
            return;
        }

        Vector3 worldPosition = new Vector3(spawnPosition.x, spawnPosition.y + spawnYOffset, 0f);
        GameObject objective = Instantiate(objectiveItemPrefab, worldPosition, Quaternion.identity, transform);
        spawnedObjects.Add(objective);

        if (avoidDuplicateSpawnTiles)
        {
            blockedPositions.Add(spawnPosition);
        }
    }
}
