using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Processors;

public class EmilBoss : Enemy
{
    private enum BossPhase
    {
        Dormant,       // before first hit
        ShieldWindow,  // invulnerable, collecting damage
        ActionWindow   // mirrors/lightning/speed based on collected damage
    }

    [Header("Projectile Throw")]
    [SerializeField] private ProjectileSpawner spawner;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform spriteTransform;
    [SerializeField] private float shiftForThrow = 2f;
    [SerializeField] private float throwInterval = 1f;

    [Header("Shield / Orb")]
    [SerializeField] private GameObject orbPrefab;
    [SerializeField] private float orbitRadius = 2f;
    [SerializeField] private int orbCount = 8;

    [Header("Lightning")]
    [SerializeField] private DiamondSpawner lightningSpawner;
    [SerializeField] private float lightningSpawnInterval = 0.5f; // was hardcoded
    [SerializeField] private int lightningsPerBurst = 3;          // N per tick

    [Header("Phase Timing")]
    [SerializeField] private float shieldWindowSeconds = 5f;
    [SerializeField] private float actionWindowSeconds = 5f;

    [Header("Thresholds (hits during shield window)")]
    
    [SerializeField] private int mirrorsHitThreshold = 3;
    [SerializeField] private int lightningHitThreshold = 6;
    [SerializeField] private int speedHitThreshold = 10;
    [SerializeField] private int savedHits = 0;
    [Header("Speed")]
    [SerializeField] private float boostedSpeed = 3f;
    [SerializeField] private float defaultSpeed = 0f; // requested revert value

    [Header("Debug State")]
    [SerializeField] private bool isInvincible = false;
    [SerializeField] private bool isThrowing = false;
    [SerializeField] private int savedDamage = 0;
    [SerializeField] private bool mirrorsEnabledThisAction = false;

    private readonly List<GameObject> activeOrbs = new List<GameObject>();

    private BossPhase currentPhase = BossPhase.Dormant;
    private bool encounterStarted = false;
    private bool throwRoutineRunning = false;

    private int maxHealthAtStart;
    private float throwTimer = 0f;
    private Coroutine phaseRoutine;
    private MoveScene moveScene;
    private bool pgrTriggeredOnDeath = false;
    
    protected override void Awake()
    {
        base.Awake();
        animator = GetComponentInChildren<Animator>();

        maxHealthAtStart = Mathf.Max(1, health);
        speed = defaultSpeed; // boss starts idle
        ResolveLightningSpawner();
    }

    protected override void Update()
    {
        if (health <= 0) { Die(); }
        if (isDead) return;

        // "until not attacked -> nothing"
        if (!encounterStarted)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }

        base.Update();

        // Mirrors are only thrown during action window when threshold was met
        if (currentPhase == BossPhase.ActionWindow && mirrorsEnabledThisAction)
        {
            throwTimer += Time.deltaTime;
            if (throwTimer >= throwInterval && !throwRoutineRunning)
            {
                throwTimer = 0f;
                StartCoroutine(ThrowCoroutine());
            }
        }
    }

    public override void TakeDamage(int amount)
    {
        if (isDead || amount <= 0) return;

        if (!encounterStarted)
        {
            encounterStarted = true;
            StartShieldWindow();
            savedHits++; // first hit counts
            return;
        }

        if (isInvincible)
        {
            savedHits++; // each successful hit while shields are up
            return;
        }

        base.TakeDamage(amount);
    }

    private void StartShieldWindow()
    {
        savedHits = 0;
        currentPhase = BossPhase.ShieldWindow;
        isInvincible = true;
        mirrorsEnabledThisAction = false;
        isThrowing = false;
        throwRoutineRunning = false;
        throwTimer = 0f;
        speed = defaultSpeed; // speed revert to 0 in default/shield state

        SpawnOrbs();

        if (phaseRoutine != null) StopCoroutine(phaseRoutine);
        phaseRoutine = StartCoroutine(ShieldWindowRoutine());
    }

    private IEnumerator ShieldWindowRoutine()
    {
        savedDamage = 0;
        yield return new WaitForSeconds(shieldWindowSeconds);

        bool enableMirrors = savedHits >= mirrorsHitThreshold;
        bool doLightning = savedHits >= lightningHitThreshold;
        bool doSpeed = savedHits >= speedHitThreshold;
        StartActionWindow(enableMirrors, doLightning, doSpeed);
    }

    private void StartActionWindow(bool enableMirrors, bool doLightning, bool doSpeed)
    {
        currentPhase = BossPhase.ActionWindow;
        isInvincible = false;
        mirrorsEnabledThisAction = enableMirrors;
        throwTimer = 0f;

        RemoveOrbs();

        speed = doSpeed ? boostedSpeed : defaultSpeed;

        if (phaseRoutine != null) StopCoroutine(phaseRoutine);
        phaseRoutine = StartCoroutine(ActionWindowRoutine(doLightning)); // pass flag
    }

    private IEnumerator ActionWindowRoutine(bool doLightning)
    {
        float elapsed = 0f;
        float lightningTimer = 0f;

        while (elapsed < actionWindowSeconds)
        {
            float dt = Time.deltaTime;
            elapsed += dt;

            if (doLightning)
            {
                lightningTimer += dt;
                while (lightningTimer >= lightningSpawnInterval)
                {
                    lightningTimer -= lightningSpawnInterval;
                    SummonLightningBurst();
                }
            }

            yield return null;
        }

        speed = defaultSpeed;
        StartShieldWindow();
        isThrowing = false;
    }
    private void SummonLightningBurst()
    {
        if (lightningSpawner == null) ResolveLightningSpawner();
        if (lightningSpawner == null)
        {
            Debug.LogWarning("EmilBoss: lightningSpawner is null, cannot summon lightning.");
            return;
        }

        int count = Mathf.Max(1, lightningsPerBurst);
        for (int i = 0; i < count; i++)
        {
            lightningSpawner.SpawnOneInDiamond();
        }
    }
    private void SpawnOrbs()
    {
        RemoveOrbs();

        if (orbPrefab == null)
        {
            Debug.LogWarning("EmilBoss: orbPrefab is null.");
            return;
        }

        for (int i = 0; i < orbCount; i++)
        {
            float angle = i * (360f / orbCount);
            float rad = angle * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * orbitRadius;
            Vector3 spawnPos = transform.position + (Vector3)offset;

            GameObject orb = Instantiate(orbPrefab, spawnPos, Quaternion.identity);
            OrbitalProjectile orbital = orb.GetComponent<OrbitalProjectile>();
            if (orbital != null)
            {
                orbital.Initialize(transform, angle);
            }

            activeOrbs.Add(orb);
        }
    }

    private void RemoveOrbs()
    {
        for (int i = 0; i < activeOrbs.Count; i++)
        {
            if (activeOrbs[i] != null) Destroy(activeOrbs[i]);
        }
        activeOrbs.Clear();
    }

    private void ResolveLightningSpawner()
    {
        if (lightningSpawner != null) return;

        DiamondSpawner[] spawners = FindObjectsByType<DiamondSpawner>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        if (spawners.Length > 0)
        {
            lightningSpawner = spawners[0];
        }
    }

    

    protected override void UpdateAnimator()
    {
        int dir = GetFacingDirection();
        if (animator == null) return;

        if (isThrowing)
        {
            
            PlayAnimation("Throw " + dir);
            if (spriteTransform != null)
            {
                spriteTransform.localPosition = new Vector3(0f, shiftForThrow, 0f);
            }
            return;
        }

        if (spriteTransform != null)
        {
            spriteTransform.localPosition = Vector3.zero;
        }
        PlayAnimation("Idle" + dir);



    }

    private IEnumerator ThrowCoroutine()
    {
        if (targetMichael == null || spawner == null || projectilePrefab == null || spawnPoint == null)
        {
            yield break;
        }

        throwRoutineRunning = true;
        isThrowing = true;
        currentAnimation = "";

        yield return new WaitForSeconds(0.5f);

        Fire();
        FireOffset(Random.Range(-20f, 20f));
        FireOffset(Random.Range(-20f, 20f));

        
        currentAnimation = "";
        throwRoutineRunning = false;
    }

    private void Fire()
    {
        Vector2 toPlayer = (targetMichael.transform.position - transform.position).normalized;
        spawner.SpawnProjectile(
            projectilePrefab,
            spawnPoint,
            gameObject,
            CombatTeam.Projectile,
            directionOverride: toPlayer
        );
    }

    private void FireOffset(float angleOffset)
    {
        Vector2 toPlayer = (targetMichael.transform.position - transform.position).normalized;
        float angle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;

        Vector2 offsetDir = new Vector2(
            Mathf.Cos((angle + angleOffset) * Mathf.Deg2Rad),
            Mathf.Sin((angle + angleOffset) * Mathf.Deg2Rad)
        );

        spawner.SpawnProjectile(
            projectilePrefab,
            spawnPoint,
            gameObject,
            CombatTeam.Projectile,
            directionOverride: offsetDir
        );
    }

    private int GetFacingDirection()
    {
        if (targetMichael == null) return GetLastDirection();

        Vector2 toPlayer = (targetMichael.transform.position - transform.position).normalized;
        float angle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
        if (angle < 0f) angle += 360f;
        return (Mathf.RoundToInt(angle / 45f)) % 8;
    }
    public override void Die()
    {
        // Step 1: Avoid double death
        if (isDead) return;

        // Step 2: Call base death logic (damage, animation, etc.)
        base.Die();
        isDead = true;

        // Step 3: Prevent multiple scene triggers
        if (pgrTriggeredOnDeath) return;
        pgrTriggeredOnDeath = true;

        // Step 4: Find MoveScene instance if we don't already have it
        if (moveScene == null)
            moveScene = FindFirstObjectByType<MoveScene>();

        // Step 5: Trigger scene transition if MoveScene exists and is active
        if (moveScene != null)
        {
            moveScene.StartCoroutine(moveScene.MoveToPGR());
        }
        else
        {
            Debug.LogWarning("EmilBoss died, but no active MoveScene instance was found.");
        }

        // Step 6: Destroy the boss so it doesn't linger in the old scene
        Destroy(gameObject);

        // Optional: log for debugging
        Debug.Log("EmilBoss died and scene transition triggered.");
    }
}