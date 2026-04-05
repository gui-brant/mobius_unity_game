using System.Collections.Generic;
using UnityEngine;

public class HybrizoBoss : Boss
{
    private enum BossState
    {
        PuzzleActive = 0,
        WeakWindow = 1
    }

    private enum AnimationMode
    {
        RelocatingRun = 0,
        StationaryShooting = 1,
        Dead = 2
    }

    [Header("References")]
    [SerializeField] private ProjectileSpawner projectileSpawner;
    [SerializeField] private Transform projectileOrigin;
    [SerializeField] private HybrizoProjectile bossProjectilePrefab;
    [SerializeField] [Min(0f)] private float projectileSpawnForwardOffset = 0.35f;

    [Header("Relocation")]
    [SerializeField] [Min(0.1f)] private float relocationIntervalSeconds = 5f;
    [SerializeField] [Min(0.1f)] private float relocationSpeed = 8f;
    [SerializeField] [Min(0f)] private float relocationStopDistance = 0.1f;
    [SerializeField] [Min(0f)] private float michaelExclusionRadius = 3f;
    [SerializeField] private List<Transform> relocationPoints = new List<Transform>();
    [SerializeField] [Min(0f)] private float relocationGraceSeconds = 0.2f;
    [SerializeField] [Min(0f)] private float nearTargetRadius = 0.35f;
    [SerializeField] [Min(0f)] private float nearTargetExtensionSeconds = 0.2f;
    [SerializeField] [Min(0f)] private float relocationSettleRadius = 0.15f;
    [SerializeField] [Min(0.1f)] private float relocationStuckGraceSeconds = 0.45f;
    [SerializeField] [Min(0.0001f)] private float relocationProgressEpsilon = 0.02f;
    [SerializeField] [Min(0f)] private float relocationArrivalLockSeconds = 0.12f;

    [Header("Debug")]
    [SerializeField] private bool enableAimDebug;
    [SerializeField] [Min(0.05f)] private float aimDebugDurationSeconds = 0.2f;
    [SerializeField] [Min(0.01f)] private float runAnimationSpeed = 1f;
    [SerializeField] [Min(0f)] private float shootCycleToleranceSeconds = 0.1f;
    [SerializeField] private bool invertDirectionalAnimation = true;

    [Header("Projectile Stats - Puzzle Active")]
    [SerializeField] [Min(0.01f)] private float puzzleActiveFireInterval = 0.75f;
    [SerializeField] [Min(0f)] private float puzzleActiveProjectileSpeed = 12f;
    [SerializeField] private int puzzleActiveProjectileDamage = 12;

    [Header("Projectile Stats - Weak Window")]
    [SerializeField] [Min(0.01f)] private float weakWindowFireInterval = 1.25f;
    [SerializeField] [Min(0f)] private float weakWindowProjectileSpeed = 8f;
    [SerializeField] private int weakWindowProjectileDamage = 6;

    private BossState currentState = BossState.PuzzleActive;
    private bool isRelocating;
    private Vector2 relocationDestination;
    private float fireTimer;
    private float relocationTimer;
    private float weakWindowTimer;
    private bool pendingWeakStateAfterRelocation;
    private float pendingWeakWindowDuration;
    private float relocationTravelTimer;
    private float relocationStuckTimer;
    private float lastRelocationDistance;
    private float previousRelocationDistance;
    private float relocationInitialDistance;
    private Vector2 relocationStartPosition;
    private Vector2 relocationTravelDirection;
    private bool usedNearTargetExtension;
    private float relocationArrivalLockTimer;
    private bool warnedMissingProjectilePrefab;
    private bool warnedMissingProjectileOrigin;
    private bool warnedMissingRelocationPoints;
    private bool warnedMissingTargetMichael;
    private bool warnedRetargetedMichael;
    private bool warnedMissingAnimator;
    private bool warnedMissingController;
    private bool warnedInvalidAnimatorLayer;
    private readonly HashSet<string> warnedMissingAnimationStates = new HashSet<string>();
    private readonly Dictionary<string, float> clipLengthLookup = new Dictionary<string, float>();

    private bool isShootCycleActive;
    private float shootCycleTimer;
    private float shootCycleElapsed;
    private float shootCycleDuration;
    private int queuedProjectileDamage;
    private float queuedProjectileSpeed;
    private Vector2 queuedShotDirection = Vector2.right;
    private int lastFacingDirectionIndex;
    private int animatorLayerIndex = -2;
    private string lastRequestedAnimationState;
    private string lastResolvedAnimationState;
    private float lastKnownRelocationDistance;
    private AnimationMode currentAnimationMode = AnimationMode.StationaryShooting;
    private int currentAnimationDirectionIndex;
    private float currentRunTravelDuration;
    private float currentRunAnimationSpeed = 1f;
    private float currentShootAnimationSpeed = 1f;

    public bool DebugIsRelocating => isRelocating;
    public float DebugRemainingDistance => lastKnownRelocationDistance;
    public Vector2 DebugRelocationDestination => relocationDestination;
    public string DebugRequestedAnimationState => lastRequestedAnimationState;
    public string DebugResolvedAnimationState => lastResolvedAnimationState;
    public string DebugAnimationMode => currentAnimationMode.ToString();
    public int DebugDirectionIndex => currentAnimationDirectionIndex;
    public float DebugRunTravelDuration => currentRunTravelDuration;
    public float DebugRunAnimationSpeed => currentRunAnimationSpeed;
    public float DebugShootCycleDuration => shootCycleDuration;
    public float DebugShootCycleElapsed => shootCycleElapsed;
    public float DebugShootAnimationSpeed => currentShootAnimationSpeed;

    public bool IsWeakWindowActive => currentState == BossState.WeakWindow;

    protected override void Awake()
    {
        base.Awake();
        EnsurePhysicsSafety();
        CacheAnimationClipDurations();
        ResolveLiveTargetMichael();
        WarnIfSetupLooksIncomplete();
        EnterPuzzleActiveState();
    }

    protected override void Update()
    {
        if (IsDead)
        {
            base.Update();
            return;
        }

        ResolveLiveTargetMichael();

        if (isRelocating)
        {
            UpdateRelocationMovement();
            UpdateAnimator();
            return;
        }

        if (currentState == BossState.WeakWindow)
        {
            HandleWeakWindowState();
            UpdateAnimator();
            return;
        }

        HandlePuzzleActiveState();
        UpdateAnimator();
    }

    public override void TakeDamage(int amount)
    {
        if (IsDead || amount <= 0)
        {
            return;
        }

        if (currentState != BossState.WeakWindow)
        {
            return;
        }

        health -= amount;
        if (health <= 0)
        {
            health = 0;
            Die();
        }
    }

    public void OnPuzzleSolved(float weakWindowDurationSeconds)
    {
        if (IsDead)
        {
            return;
        }

        pendingWeakWindowDuration = Mathf.Max(0f, weakWindowDurationSeconds);
        pendingWeakStateAfterRelocation = true;
        TryStartRelocation(forceRelocation: true);

        if (!isRelocating && pendingWeakStateAfterRelocation)
        {
            EnterWeakWindowState(pendingWeakWindowDuration);
            pendingWeakStateAfterRelocation = false;
        }
    }

    public void EnterPuzzleActiveState()
    {
        currentState = BossState.PuzzleActive;
        weakWindowTimer = 0f;
        fireTimer = 0f;
        relocationArrivalLockTimer = 0f;
        ResetShootCycleState();
        relocationTimer = Mathf.Max(0.1f, relocationIntervalSeconds);
        pendingWeakStateAfterRelocation = false;
    }

    private void HandlePuzzleActiveState()
    {
        SetMovement(Vector2.zero);
        speed = Mathf.Max(0f, relocationSpeed);

        relocationTimer -= Time.deltaTime;
        fireTimer -= Time.deltaTime;
        if (relocationArrivalLockTimer > 0f)
        {
            relocationArrivalLockTimer -= Time.deltaTime;
        }

        if (relocationTimer <= 0f && relocationArrivalLockTimer <= 0f)
        {
            TryStartRelocation(forceRelocation: false);
        }

        if (isRelocating)
        {
            return;
        }

        ProcessShootingCycle(
            Mathf.Max(0, puzzleActiveProjectileDamage),
            Mathf.Max(0f, puzzleActiveProjectileSpeed),
            Mathf.Max(0.01f, puzzleActiveFireInterval));
    }

    private void HandleWeakWindowState()
    {
        SetMovement(Vector2.zero);
        weakWindowTimer -= Time.deltaTime;
        fireTimer -= Time.deltaTime;

        ProcessShootingCycle(
            Mathf.Max(0, weakWindowProjectileDamage),
            Mathf.Max(0f, weakWindowProjectileSpeed),
            Mathf.Max(0.01f, weakWindowFireInterval));

        if (weakWindowTimer <= 0f)
        {
            EnterPuzzleActiveState();
        }
    }

    private void EnterWeakWindowState(float durationSeconds)
    {
        currentState = BossState.WeakWindow;
        weakWindowTimer = Mathf.Max(0f, durationSeconds);
        fireTimer = 0f;
        relocationArrivalLockTimer = 0f;
        relocationTimer = Mathf.Max(0.1f, relocationIntervalSeconds);
        ResetShootCycleState();
        SetMovement(Vector2.zero);
    }

    protected override void UpdateAnimator()
    {
        if (animator == null)
        {
            return;
        }

        if (IsDead)
        {
            currentAnimationMode = AnimationMode.Dead;
            currentAnimationDirectionIndex = lastFacingDirectionIndex;
            animator.speed = 1f;
            currentRunAnimationSpeed = 1f;
            currentShootAnimationSpeed = 1f;
            TryPlayAnimationState("die", "die");
            return;
        }

        if (isRelocating)
        {
            currentAnimationMode = AnimationMode.RelocatingRun;
            Vector2 runDirection = relocationDestination - (Vector2)transform.position;
            int runDirectionIndex = ResolveDirectionIndex(runDirection, lastFacingDirectionIndex);
            lastFacingDirectionIndex = runDirectionIndex;
            currentAnimationDirectionIndex = runDirectionIndex;
            string runStateName = GetRunStateName(runDirectionIndex);
            currentRunAnimationSpeed = GetRunAnimationSpeed(runStateName);
            animator.speed = currentRunAnimationSpeed;
            currentShootAnimationSpeed = 1f;
            TryPlayAnimationState(runStateName, "run0_right");
            return;
        }

        currentAnimationMode = AnimationMode.StationaryShooting;
        int shootDirectionIndex = ResolveShootDirectionIndex();
        lastFacingDirectionIndex = shootDirectionIndex;
        currentAnimationDirectionIndex = shootDirectionIndex;
        string shootState = GetShootStateName(shootDirectionIndex);
        float shootInterval = isShootCycleActive
            ? Mathf.Max(0.01f, shootCycleDuration)
            : Mathf.Max(0.01f, GetCurrentFireInterval());
        currentShootAnimationSpeed = GetAnimationSpeedForDuration(shootState, shootInterval);
        animator.speed = currentShootAnimationSpeed;
        currentRunAnimationSpeed = 1f;
        TryPlayAnimationState(shootState, "shooting0_right");
    }

    private void ProcessShootingCycle(int projectileDamage, float projectileSpeed, float fireInterval)
    {
        if (!IsValidTarget(targetMichael) || bossProjectilePrefab == null)
        {
            ResetShootCycleState();
            return;
        }

        if (isShootCycleActive)
        {
            shootCycleTimer -= Time.deltaTime;
            shootCycleElapsed += Time.deltaTime;

            float failSafeLimit = Mathf.Max(0.01f, shootCycleDuration + Mathf.Max(0f, shootCycleToleranceSeconds));
            if (shootCycleTimer <= 0f || shootCycleElapsed >= failSafeLimit)
            {
                FireQueuedProjectile();
                ResetShootCycleState();
                fireTimer = 0f;
            }

            return;
        }

        if (fireTimer > 0f)
        {
            return;
        }

        StartShootCycle(projectileDamage, projectileSpeed, fireInterval);
    }

    private void StartShootCycle(int projectileDamage, float projectileSpeed, float fireInterval)
    {
        Transform shootOrigin = projectileOrigin != null ? projectileOrigin : transform;
        Vector2 toMichael = (Vector2)targetMichael.transform.position - (Vector2)shootOrigin.position;
        if (toMichael.sqrMagnitude <= Mathf.Epsilon)
        {
            toMichael = Vector2.right;
        }

        queuedShotDirection = toMichael.normalized;
        queuedProjectileDamage = projectileDamage;
        queuedProjectileSpeed = projectileSpeed;
        shootCycleDuration = Mathf.Max(0.01f, fireInterval);
        shootCycleTimer = shootCycleDuration;
        shootCycleElapsed = 0f;
        isShootCycleActive = true;
        FaceShootDirection(queuedShotDirection);
    }

    private void FireQueuedProjectile()
    {
        ProjectileSpawner spawner = ResolveProjectileSpawner();
        if (spawner == null || !IsValidTarget(targetMichael) || bossProjectilePrefab == null)
        {
            return;
        }

        Transform shootOrigin = projectileOrigin != null ? projectileOrigin : transform;
        Vector2 toMichael = (Vector2)targetMichael.transform.position - (Vector2)shootOrigin.position;
        if (toMichael.sqrMagnitude > Mathf.Epsilon)
        {
            queuedShotDirection = toMichael.normalized;
        }

        if (projectileOrigin != null && projectileOrigin != transform)
        {
            projectileOrigin.right = new Vector3(queuedShotDirection.x, queuedShotDirection.y, 0f);
        }

        FaceShootDirection(queuedShotDirection);
        Vector2 spawnPosition = (Vector2)shootOrigin.position
            + (queuedShotDirection * Mathf.Max(0f, projectileSpawnForwardOffset));
        Projectile spawned = spawner.SpawnProjectile(
            bossProjectilePrefab.gameObject,
            shootOrigin,
            gameObject,
            Team,
            queuedProjectileDamage,
            queuedShotDirection,
            spawnPosition);

        if (spawned != null)
        {
            // Force movement direction in case a prefab/setup path resets init direction.
            spawned.SetMovement(queuedShotDirection);
        }

        if (enableAimDebug)
        {
            Debug.DrawLine(shootOrigin.position, targetMichael.transform.position, Color.red, Mathf.Max(0.05f, aimDebugDurationSeconds));
        }

        if (spawned is HybrizoProjectile bossProjectile)
        {
            bossProjectile.ConfigureTravelSpeed(queuedProjectileSpeed);
            bossProjectile.ConfigureDamage(queuedProjectileDamage);
        }
    }

    private void TryStartRelocation(bool forceRelocation)
    {
        if (currentState != BossState.PuzzleActive && !forceRelocation)
        {
            return;
        }

        if (!TrySelectRelocationDestination(out Vector2 destination))
        {
            relocationTimer = Mathf.Max(0.1f, relocationIntervalSeconds);
            return;
        }

        relocationDestination = destination;
        isRelocating = true;
        speed = Mathf.Max(0f, relocationSpeed);
        BeginRelocationLeg();
        ResetShootCycleState();

        Vector2 movementDirection = (relocationDestination - (Vector2)transform.position).normalized;
        FaceMoveDirection(movementDirection);
        SetMovement(movementDirection);
    }

    private void UpdateRelocationMovement()
    {
        Vector2 current = transform.position;
        Vector2 toDestination = relocationDestination - current;
        float remainingDistance = toDestination.magnitude;
        lastKnownRelocationDistance = remainingDistance;
        relocationTravelTimer -= Time.deltaTime;

        if (ShouldFinishRelocation(current, toDestination, remainingDistance, out bool shouldSnapToDestination))
        {
            if (shouldSnapToDestination)
            {
                transform.position = relocationDestination;
            }

            FinishRelocation();
            return;
        }

        bool madeProgress = remainingDistance < (lastRelocationDistance - Mathf.Max(0.0001f, relocationProgressEpsilon));
        if (madeProgress)
        {
            relocationStuckTimer = 0f;
            lastRelocationDistance = remainingDistance;
        }
        else
        {
            relocationStuckTimer += Time.deltaTime;
        }

        bool timedOut = relocationTravelTimer <= 0f;
        bool isStuck = relocationStuckTimer >= Mathf.Max(0.1f, relocationStuckGraceSeconds);
        if (timedOut || isStuck)
        {
            if (TryApplyNearTargetExtension(remainingDistance))
            {
                // Extended once near destination; keep current target.
            }
            else
            {
                if (TrySelectRelocationDestination(out Vector2 rerolledDestination))
                {
                    relocationDestination = rerolledDestination;
                    BeginRelocationLeg();
                }
                else
                {
                    FinishRelocation();
                    return;
                }
            }
        }

        Vector2 movementDirection = (relocationDestination - (Vector2)transform.position).normalized;
        FaceMoveDirection(movementDirection);
        SetMovement(movementDirection);
        previousRelocationDistance = remainingDistance;
    }

    private bool TrySelectRelocationDestination(out Vector2 destination)
    {
        return TryGetRelocationPointDestination((Vector2)transform.position, out destination);
    }

    private bool TryGetRelocationPointDestination(Vector2 currentPosition, out Vector2 destination)
    {
        destination = Vector2.zero;
        if (relocationPoints == null || relocationPoints.Count == 0)
        {
            return false;
        }

        List<Vector2> validCandidates = new List<Vector2>();
        Vector2 michaelPosition = targetMichael != null
            ? (Vector2)targetMichael.transform.position
            : Vector2.positiveInfinity;

        for (int i = 0; i < relocationPoints.Count; i++)
        {
            Transform point = relocationPoints[i];
            if (point == null)
            {
                continue;
            }

            Vector2 candidate = point.position;
            if (targetMichael != null && Vector2.Distance(candidate, michaelPosition) < michaelExclusionRadius)
            {
                continue;
            }

            if (Vector2.Distance(candidate, currentPosition) <= Mathf.Max(0.01f, relocationStopDistance))
            {
                continue;
            }

            validCandidates.Add(candidate);
        }

        if (validCandidates.Count == 0)
        {
            return false;
        }

        destination = validCandidates[Random.Range(0, validCandidates.Count)];
        return true;
    }

    private void BeginRelocationLeg()
    {
        relocationStartPosition = transform.position;
        float legDistance = Vector2.Distance(transform.position, relocationDestination);
        relocationInitialDistance = legDistance;
        currentRunTravelDuration = Mathf.Max(0.01f, legDistance / Mathf.Max(0.01f, relocationSpeed));
        float expectedTravelTime = legDistance / Mathf.Max(0.01f, relocationSpeed);
        relocationTravelTimer = Mathf.Max(0.05f, expectedTravelTime + Mathf.Max(0f, relocationGraceSeconds));
        relocationStuckTimer = 0f;
        lastRelocationDistance = legDistance;
        previousRelocationDistance = legDistance;
        lastKnownRelocationDistance = legDistance;
        usedNearTargetExtension = false;
        Vector2 legDirection = (relocationDestination - (Vector2)transform.position).normalized;
        relocationTravelDirection = legDirection.sqrMagnitude <= Mathf.Epsilon ? Vector2.right : legDirection;
    }

    private void ResolveLiveTargetMichael()
    {
        if (IsValidTarget(targetMichael))
        {
            return;
        }

        Michael[] candidates = FindObjectsByType<Michael>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < candidates.Length; i++)
        {
            if (!IsValidTarget(candidates[i]))
            {
                continue;
            }

            bool wasAssigned = targetMichael != null;
            targetMichael = candidates[i];
            if (wasAssigned && !warnedRetargetedMichael)
            {
                Debug.LogWarning($"{nameof(HybrizoBoss)} on '{name}' detected a stale Michael reference and retargeted at runtime.", this);
                warnedRetargetedMichael = true;
            }

            return;
        }

        targetMichael = null;
    }

    private static bool IsValidTarget(Michael michael)
    {
        return michael != null && michael.gameObject.activeInHierarchy && !michael.IsDead;
    }

    private void EnsurePhysicsSafety()
    {
        if (rb == null)
        {
            return;
        }

        rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
        rb.angularVelocity = 0f;
    }

    private void WarnIfSetupLooksIncomplete()
    {
        if (!warnedMissingProjectilePrefab && bossProjectilePrefab == null)
        {
            Debug.LogWarning($"{nameof(HybrizoBoss)} on '{name}' is missing Boss Projectile Prefab. Boss will not fire.", this);
            warnedMissingProjectilePrefab = true;
        }

        if (!warnedMissingProjectileOrigin && projectileOrigin == null)
        {
            Debug.LogWarning($"{nameof(HybrizoBoss)} on '{name}' has no Projectile Origin. Falling back to boss transform.", this);
            warnedMissingProjectileOrigin = true;
        }

        if (!warnedMissingRelocationPoints && (relocationPoints == null || relocationPoints.Count == 0))
        {
            Debug.LogWarning($"{nameof(HybrizoBoss)} on '{name}' has no Relocation Points. Relocation will be skipped.", this);
            warnedMissingRelocationPoints = true;
        }

        if (!warnedMissingTargetMichael && !IsValidTarget(targetMichael))
        {
            Debug.LogWarning($"{nameof(HybrizoBoss)} on '{name}' has no valid active Michael target. Boss will wait until one is available.", this);
            warnedMissingTargetMichael = true;
        }
    }

    private void FinishRelocation()
    {
        isRelocating = false;
        relocationTravelTimer = 0f;
        relocationStuckTimer = 0f;
        lastRelocationDistance = 0f;
        previousRelocationDistance = 0f;
        relocationInitialDistance = 0f;
        currentRunTravelDuration = 0f;
        currentRunAnimationSpeed = 1f;
        relocationStartPosition = transform.position;
        relocationTravelDirection = Vector2.zero;
        lastKnownRelocationDistance = 0f;
        usedNearTargetExtension = false;
        relocationArrivalLockTimer = Mathf.Max(0f, relocationArrivalLockSeconds);
        SetMovement(Vector2.zero);
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        relocationTimer = Mathf.Max(0.1f, relocationIntervalSeconds);

        if (pendingWeakStateAfterRelocation)
        {
            EnterWeakWindowState(pendingWeakWindowDuration);
            pendingWeakStateAfterRelocation = false;
        }
    }

    private void FaceShootDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }
        lastFacingDirectionIndex = ResolveDirectionIndex(direction, lastFacingDirectionIndex);
    }

    private void FaceMoveDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }
        lastFacingDirectionIndex = ResolveDirectionIndex(direction, lastFacingDirectionIndex);
    }

    private void ResetShootCycleState()
    {
        isShootCycleActive = false;
        shootCycleTimer = 0f;
        shootCycleElapsed = 0f;
        shootCycleDuration = 0f;
    }

    private ProjectileSpawner ResolveProjectileSpawner()
    {
        if (projectileSpawner != null)
        {
            return projectileSpawner;
        }

        projectileSpawner = GetComponent<ProjectileSpawner>();
        if (projectileSpawner != null)
        {
            return projectileSpawner;
        }

        projectileSpawner = gameObject.AddComponent<ProjectileSpawner>();
        return projectileSpawner;
    }

    private void OnDrawGizmosSelected()
    {
        if (!enableAimDebug || !IsValidTarget(targetMichael))
        {
            return;
        }

        Transform shootOrigin = projectileOrigin != null ? projectileOrigin : transform;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(shootOrigin.position, targetMichael.transform.position);
    }

    private int ResolveShootDirectionIndex()
    {
        if (isShootCycleActive)
        {
            return ResolveDirectionIndex(queuedShotDirection, lastFacingDirectionIndex);
        }

        if (!IsValidTarget(targetMichael))
        {
            return lastFacingDirectionIndex;
        }

        Transform shootOrigin = projectileOrigin != null ? projectileOrigin : transform;
        Vector2 toMichael = (Vector2)targetMichael.transform.position - (Vector2)shootOrigin.position;
        return ResolveDirectionIndex(toMichael, lastFacingDirectionIndex);
    }

    private static int ResolveDirectionIndex(Vector2 direction, int fallbackDirection)
    {
        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            return Mathf.Clamp(fallbackDirection, 0, 7);
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (angle < 0f)
        {
            angle += 360f;
        }

        return Mathf.RoundToInt(angle / 45f) % 8;
    }

    private string GetShootStateName(int direction)
    {
        direction = ResolveVisualDirectionIndex(direction);
        return direction switch
        {
            0 => "shooting0_right",
            1 => "shooting1_up_right",
            2 => "shooting2_up",
            3 => "shooting3_up_left",
            4 => "shooting4_left",
            5 => "shooting5_down_left",
            6 => "shooting6_down",
            7 => "shooting7_down_right",
            _ => "shooting0_right"
        };
    }

    private string GetRunStateName(int direction)
    {
        direction = ResolveVisualDirectionIndex(direction);
        return direction switch
        {
            0 => "run0_right",
            1 => "run1_up_right",
            2 => "run2_up",
            3 => "run3_up_left",
            4 => "run4_left",
            5 => "run5_down_left",
            6 => "run6_down",
            7 => "run7_down_right",
            _ => "run0_right"
        };
    }

    private float GetCurrentFireInterval()
    {
        return currentState == BossState.WeakWindow
            ? Mathf.Max(0.01f, weakWindowFireInterval)
            : Mathf.Max(0.01f, puzzleActiveFireInterval);
    }

    private float GetAnimationSpeedForDuration(string stateName, float desiredDuration)
    {
        float clipLength = GetClipLength(stateName);
        if (clipLength <= Mathf.Epsilon)
        {
            return 1f;
        }

        return Mathf.Max(0.01f, clipLength / Mathf.Max(0.01f, desiredDuration));
    }

    private float GetRunAnimationSpeed(string runStateName)
    {
        float clipLength = GetClipLength(runStateName);
        if (clipLength <= Mathf.Epsilon)
        {
            return Mathf.Max(0.01f, runAnimationSpeed);
        }

        float expectedTravelDuration = Mathf.Max(0.01f, currentRunTravelDuration);

        float baseSpeed = clipLength / expectedTravelDuration;
        return Mathf.Max(0.01f, baseSpeed * Mathf.Max(0.01f, runAnimationSpeed));
    }

    private bool HasCrossedRelocationDestination(Vector2 currentPosition, Vector2 toDestination)
    {
        if (relocationTravelDirection.sqrMagnitude <= Mathf.Epsilon)
        {
            return false;
        }

        Vector2 toCurrent = currentPosition - relocationStartPosition;
        float traveledAlongPath = Vector2.Dot(toCurrent, relocationTravelDirection);
        float pathLength = Vector2.Distance(relocationStartPosition, relocationDestination);
        if (traveledAlongPath >= pathLength)
        {
            return true;
        }

        // Guard against edge jitter: if distance starts increasing very near target, treat as crossed.
        bool distanceGrowing = previousRelocationDistance > 0f
            && (toDestination.magnitude - previousRelocationDistance) > Mathf.Max(0.0001f, relocationProgressEpsilon);
        bool nearTarget = toDestination.magnitude <= Mathf.Max(relocationSettleRadius, nearTargetRadius);
        return distanceGrowing && nearTarget;
    }

    private void CacheAnimationClipDurations()
    {
        clipLengthLookup.Clear();

        if (animator == null || animator.runtimeAnimatorController == null)
        {
            return;
        }

        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        for (int i = 0; i < clips.Length; i++)
        {
            AnimationClip clip = clips[i];
            if (clip == null)
            {
                continue;
            }

            clipLengthLookup[clip.name] = Mathf.Max(0.01f, clip.length);
        }
    }

    private float GetClipLength(string clipName)
    {
        if (clipLengthLookup.TryGetValue(clipName, out float clipLength))
        {
            return clipLength;
        }

        // Canonical state names are shooting*, but clips may still be named shoot*.
        if (clipName.StartsWith("shooting"))
        {
            string legacyShootName = "shoot" + clipName.Substring("shooting".Length);
            if (clipLengthLookup.TryGetValue(legacyShootName, out clipLength))
            {
                return clipLength;
            }
        }

        return 0f;
    }

    private bool CanPlayState(string stateName)
    {
        if (string.IsNullOrWhiteSpace(stateName) || animator == null || animator.runtimeAnimatorController == null)
        {
            return false;
        }

        int layerIndex = ResolveAnimatorLayerIndex();
        if (layerIndex < 0)
        {
            return false;
        }

        return animator.HasState(layerIndex, Animator.StringToHash(stateName));
    }

    private void TryPlayAnimationState(string stateName, string preferredFallbackState)
    {
        if (animator == null)
        {
            if (!warnedMissingAnimator)
            {
                Debug.LogWarning($"{nameof(HybrizoBoss)} on '{name}' has no Animator component.", this);
                warnedMissingAnimator = true;
            }

            return;
        }

        if (animator.runtimeAnimatorController == null)
        {
            if (!warnedMissingController)
            {
                Debug.LogWarning($"{nameof(HybrizoBoss)} on '{name}' has no Animator Controller assigned.", this);
                warnedMissingController = true;
            }

            return;
        }

        int layerIndex = ResolveAnimatorLayerIndex();
        if (layerIndex < 0)
        {
            return;
        }

        lastRequestedAnimationState = stateName;
        string resolvedStateName = stateName;
        if (!CanPlayState(resolvedStateName))
        {
            if (!warnedMissingAnimationStates.Contains(resolvedStateName))
            {
                Debug.LogWarning($"{nameof(HybrizoBoss)} is missing Animator state '{resolvedStateName}'.", this);
                warnedMissingAnimationStates.Add(resolvedStateName);
            }

            string fallbackState = ResolveSafeFallbackState(resolvedStateName, preferredFallbackState);
            if (!CanPlayState(fallbackState))
            {
                return;
            }

            resolvedStateName = fallbackState;
        }

        lastResolvedAnimationState = resolvedStateName;
        int stateHash = Animator.StringToHash(resolvedStateName);
        if (animator.GetCurrentAnimatorStateInfo(layerIndex).shortNameHash == stateHash)
        {
            return;
        }

        animator.Play(stateHash, layerIndex, 0f);
    }

    private int ResolveAnimatorLayerIndex()
    {
        if (animatorLayerIndex != -2)
        {
            return animatorLayerIndex;
        }

        if (animator == null || animator.runtimeAnimatorController == null)
        {
            animatorLayerIndex = -1;
            return animatorLayerIndex;
        }

        if (animator.layerCount <= 0)
        {
            if (!warnedInvalidAnimatorLayer)
            {
                Debug.LogWarning($"{nameof(HybrizoBoss)} on '{name}' has an Animator with no valid layers.", this);
                warnedInvalidAnimatorLayer = true;
            }

            animatorLayerIndex = -1;
            return animatorLayerIndex;
        }

        animatorLayerIndex = 0;
        return animatorLayerIndex;
    }

    private string ResolveSafeFallbackState(string requestedState, string preferredFallbackState)
    {
        if (CanPlayState(preferredFallbackState))
        {
            return preferredFallbackState;
        }

        if (requestedState.StartsWith("run"))
        {
            if (CanPlayState("run0_right"))
            {
                return "run0_right";
            }
        }

        if (requestedState.StartsWith("shoot"))
        {
            if (CanPlayState("shooting0_right"))
            {
                return "shooting0_right";
            }
        }

        if (CanPlayState("die"))
        {
            return "die";
        }

        return requestedState;
    }

    private bool ShouldFinishRelocation(Vector2 currentPosition, Vector2 toDestination, float remainingDistance, out bool shouldSnapToDestination)
    {
        shouldSnapToDestination = false;

        if (remainingDistance <= Mathf.Max(0.01f, relocationSettleRadius))
        {
            shouldSnapToDestination = true;
            return true;
        }

        if (remainingDistance <= Mathf.Max(0.01f, relocationStopDistance))
        {
            return true;
        }

        if (HasCrossedRelocationDestination(currentPosition, toDestination))
        {
            shouldSnapToDestination = true;
            return true;
        }

        return false;
    }

    private bool TryApplyNearTargetExtension(float remainingDistance)
    {
        if (usedNearTargetExtension || remainingDistance > Mathf.Max(0f, nearTargetRadius))
        {
            return false;
        }

        usedNearTargetExtension = true;
        relocationTravelTimer = Mathf.Max(0.01f, nearTargetExtensionSeconds);
        relocationStuckTimer = 0f;
        return true;
    }

    private int ResolveVisualDirectionIndex(int worldDirectionIndex)
    {
        int normalized = ((worldDirectionIndex % 8) + 8) % 8;
        if (!invertDirectionalAnimation)
        {
            return normalized;
        }

        // 180 degree visual remap for sprite sets authored with opposite facing semantics.
        return (normalized + 4) % 8;
    }
}
