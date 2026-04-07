using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] private int lightningCount = 10;

    [Header("Phase Timing")]
    [SerializeField] private float shieldWindowSeconds = 5f;
    [SerializeField] private float actionWindowSeconds = 5f;

    [Header("Thresholds (% of max HP damage during shield window)")]
    [SerializeField][Range(0f, 1f)] private float mirrorsThreshold = 0.20f;
    [SerializeField][Range(0f, 1f)] private float lightningThreshold = 0.40f;
    [SerializeField][Range(0f, 1f)] private float speedThreshold = 0.80f;

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

        // First hit starts encounter + shield window immediately
        if (!encounterStarted)
        {
            encounterStarted = true;
            StartShieldWindow();
            savedDamage += amount; // first hit counts into shield damage window
            return;
        }

        if (isInvincible)
        {
            savedDamage += amount;
            return;
        }

        base.TakeDamage(amount);
    }

    private void StartShieldWindow()
    {
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

        float damageRatio = (float)savedDamage / maxHealthAtStart;

        bool enableMirrors = damageRatio >= mirrorsThreshold;
        bool doLightning = damageRatio >= lightningThreshold;
        bool doSpeed = damageRatio >= speedThreshold;

        StartActionWindow(enableMirrors, doLightning, doSpeed);
    }

    private void StartActionWindow(bool enableMirrors, bool doLightning, bool doSpeed)
    {
        currentPhase = BossPhase.ActionWindow;
        isInvincible = false;
        mirrorsEnabledThisAction = enableMirrors;
        throwTimer = 0f;

        RemoveOrbs();

        if (doLightning)
        {
            SummonLightnings();
        }

        speed = doSpeed ? boostedSpeed : defaultSpeed;

        if (phaseRoutine != null) StopCoroutine(phaseRoutine);
        phaseRoutine = StartCoroutine(ActionWindowRoutine());
    }

    private IEnumerator ActionWindowRoutine()
    {
        yield return new WaitForSeconds(actionWindowSeconds);

        // after next 5 seconds revert to default state with shields. speed -> 0
        speed = defaultSpeed;
        StartShieldWindow();
        isThrowing = false;
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

    private void SummonLightnings()
    {
        if (lightningSpawner == null) ResolveLightningSpawner();

        if (lightningSpawner == null)
        {
            Debug.LogWarning("EmilBoss: lightningSpawner is null, cannot summon lightning.");
            return;
        }

        for (int i = 0; i < lightningCount; i++)
        {
            lightningSpawner.SpawnOneInDiamond();
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
}