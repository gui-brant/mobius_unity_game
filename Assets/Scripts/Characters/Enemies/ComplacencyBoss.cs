using System;
using System.Collections.Generic;
using Combat;
using UnityEngine;

public class ComplacencyBoss : Boss
{
    [Header("Complacency Boss")] 
    [SerializeField] private float fleeRadius = 5f;

    [Header("Slow Projectile")]
    [SerializeField] private ComplacencyProjectile projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float projectileFireRate = 1.5f;
    [SerializeField] private float projectileDamage = 8f;
    [SerializeField] private float slowMultiplier = 0.4f;
    [SerializeField] private float slowDuration = 2;
    
    private ProjectileSpawner projectileSpawner;
    private float cooldownTimer;
    private bool isFleeing;
    
    private MoveScene moveScene;
    
    public override string BossId => "complacencyBoss";

    protected override void Awake()
    {
        base.Awake();
        projectileSpawner = GetComponentInChildren<ProjectileSpawner>() ?? gameObject.AddComponent<ProjectileSpawner>();
    }
    
    protected override void Update()
    {
        if (isDead || targetMichael == null)
        {
            isFleeing = false;
            base.Update();
            return;
        }
        
        float distance = Vector2.Distance(transform.position, targetMichael.transform.position);

        if (distance >= fleeRadius)
        {
            isFleeing = true;
            Vector2 awayFromMichael = ((Vector2)transform.position - (Vector2)targetMichael.transform.position).normalized;
            
            SetMovement(awayFromMichael);
            
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                FireProjectile();
                cooldownTimer = projectileFireRate;
            }
            
            UpdateAnimator();
        }
        else
        {
            isFleeing = false;
            cooldownTimer = 0f;
            base.Update();
        }
    }

    private void FireProjectile()
    {
        if (projectilePrefab == null || projectileSpawnPoint == null || targetMichael == null)
        {
            return;
        }
        
        Vector2 directionToMichael = ((Vector2)targetMichael.transform.position - (Vector2)projectileSpawnPoint.position).normalized;
        
        ComplacencyProjectile spawnedProjectile = projectileSpawner.SpawnProjectile(projectilePrefab.gameObject, projectileSpawnPoint, gameObject, CombatTeam.Enemy, projectileDamage, directionToMichael) as ComplacencyProjectile;

        if (spawnedProjectile == null)
        {
            return;
        }
        
        Renderer bossRenderer = GetComponentInChildren<Renderer>();
        Renderer projectileRenderer = spawnedProjectile.GetComponentInChildren<Renderer>();

        if (bossRenderer != null && projectileRenderer != null)
        {
            projectileRenderer.sortingLayerID = bossRenderer.sortingLayerID;
            projectileRenderer.sortingOrder = bossRenderer.sortingOrder;
        }
        spawnedProjectile.SetSlowParameters(slowMultiplier, slowDuration);
    }

    protected override void UpdateAnimator()
    {
        base.UpdateAnimator();
    }

    public override void OnEncounterStart()
    {
        base.OnEncounterStart();
        isFleeing = false;
    }

    public override void OnEncounterEnd()
    {
        base.OnEncounterEnd();
        isFleeing = false;
    }
}
