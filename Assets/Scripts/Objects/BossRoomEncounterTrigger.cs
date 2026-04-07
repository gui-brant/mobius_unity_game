using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BossRoomEncounterTrigger : MonoBehaviour
{
    [Header("Player Setup")]
    [SerializeField] private Michael playerInScene;
    [SerializeField] private Michael playerPrefab;
    [SerializeField] private Transform playerSpawnPoint;

    [Header("Boss Setup")]
    [SerializeField] private Boss bossInScene;
    [SerializeField] private Boss bossPrefab;
    [SerializeField] private Transform bossSpawnPoint;
    [SerializeField] private HybrizoPuzzleController puzzleController;

    [Header("Behavior")]
    [SerializeField] private bool oneShot = true;

    private bool encounterStarted;

    private void Awake()
    {
        Collider2D triggerCollider = GetComponent<Collider2D>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (encounterStarted && oneShot)
        {
            return;
        }

        if (other == null || !other.TryGetComponent<Michael>(out Michael enteringPlayer))
        {
            return;
        }

        StartEncounter(enteringPlayer);
    }

    public void StartEncounter(Michael enteringPlayer)
    {
        if (encounterStarted && oneShot)
        {
            return;
        }

        Michael resolvedPlayer = ResolvePlayer(enteringPlayer);
        Boss resolvedBoss = ResolveBoss();

        PositionCharacter(resolvedPlayer, playerSpawnPoint);
        PositionCharacter(resolvedBoss, bossSpawnPoint);

        if (resolvedPlayer != null)
        {
            resolvedPlayer.gameObject.SetActive(true);
        }

        if (resolvedBoss != null)
        {
            resolvedBoss.SetTargetMichael(resolvedPlayer);
            resolvedBoss.gameObject.SetActive(true);
            resolvedBoss.OnEncounterStart();
        }

        if (puzzleController != null)
        {
            puzzleController.enabled = true;
        }

        encounterStarted = true;
    }

    private Michael ResolvePlayer(Michael enteringPlayer)
    {
        if (enteringPlayer != null)
        {
            playerInScene = enteringPlayer;
            return playerInScene;
        }

        if (playerInScene != null)
        {
            return playerInScene;
        }

        if (playerPrefab != null)
        {
            Vector3 spawnPosition = playerSpawnPoint != null ? playerSpawnPoint.position : transform.position;
            playerInScene = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            return playerInScene;
        }

        playerInScene = FindFirstObjectByType<Michael>();
        return playerInScene;
    }

    private Boss ResolveBoss()
    {
        if (bossInScene != null)
        {
            return bossInScene;
        }

        if (bossPrefab != null)
        {
            Vector3 spawnPosition = bossSpawnPoint != null ? bossSpawnPoint.position : transform.position;
            bossInScene = Instantiate(bossPrefab, spawnPosition, Quaternion.identity);
            return bossInScene;
        }

        bossInScene = FindFirstObjectByType<Boss>();
        return bossInScene;
    }

    private static void PositionCharacter(Component character, Transform spawnPoint)
    {
        if (character == null || spawnPoint == null)
        {
            return;
        }

        character.transform.position = spawnPoint.position;
    }
}
