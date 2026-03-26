using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SimpleRandomWalkDungeonGenerator dungeonGenerator;
    [SerializeField] private Michael michael;
    [SerializeField] private Transform playerTransform;

    [Header("Prefabs To Spawn")]
    [SerializeField] private List<GameObject> prefabsToSpawn = new List<GameObject>();
    [SerializeField] [Min(1)] private int spawnCount = 10;

    [Header("Placement")]
    [SerializeField] private bool avoidDuplicateSpawnTiles = true;
    [SerializeField] private int maxPlacementAttemptsPerObject = 20;
    [SerializeField] private float spawnYOffset = 0f;

    private readonly List<GameObject> spawnedObjects = new List<GameObject>();
    private IReadOnlyCollection<Vector2> pendingFloorPositions;
    private bool hasSpawnedForCurrentRoom;

    private void OnEnable()
    {
        ResolveReferences();
        SubscribeToGenerator();
    }

    private void Start()
    {
        ResolveReferences();
        CacheExistingGeneratedRoom();
        TrySpawnWhenReady();
    }

    private void Update()
    {
        if (hasSpawnedForCurrentRoom)
        {
            return;
        }

        if (playerTransform == null)
        {
            ResolvePlayerReference();
        }

        TrySpawnWhenReady();
    }

    private void OnDisable()
    {
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
        if (michael == null)
        {
            michael = FindFirstObjectByType<Michael>();
        }

        if (michael != null)
        {
            playerTransform = michael.transform;
            return;
        }

        if (playerTransform == null)
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
        hasSpawnedForCurrentRoom = false;
        pendingFloorPositions = floorPositions;
        TrySpawnWhenReady();
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

        SpawnPrefabsInRoom(pendingFloorPositions);
        hasSpawnedForCurrentRoom = true;
    }

    private void SpawnPrefabsInRoom(IReadOnlyCollection<Vector2> floorPositions)
    {
        List<Vector2> floorList = floorPositions.ToList();
        if (floorList.Count == 0)
        {
            return;
        }

        HashSet<Vector2> blockedPositions = new HashSet<Vector2>();
        if (avoidDuplicateSpawnTiles)
        {
            Vector2 playerTile = new Vector2(playerTransform.position.x, playerTransform.position.y);
            blockedPositions.Add(playerTile);
        }

        int availableTiles = floorList.Count;
        int targetSpawnCount = avoidDuplicateSpawnTiles
            ? Mathf.Min(spawnCount, availableTiles)
            : spawnCount;

        for (int i = 0; i < targetSpawnCount; i++)
        {
            if (!TryGetSpawnPosition(floorList, blockedPositions, out Vector2 spawnPosition))
            {
                break;
            }

            GameObject prefab = prefabsToSpawn[Random.Range(0, prefabsToSpawn.Count)];
            if (prefab == null)
            {
                continue;
            }

            Vector3 worldPosition = new Vector3(spawnPosition.x, spawnPosition.y + spawnYOffset, 0f);
            GameObject spawned = Instantiate(prefab, worldPosition, Quaternion.identity, transform);
            spawnedObjects.Add(spawned);

            if (avoidDuplicateSpawnTiles)
            {
                blockedPositions.Add(spawnPosition);
            }
        }
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
}
