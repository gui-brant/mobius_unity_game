# Current Game Feature Specification

This document describes the currently implemented feature set in the Unity project (`Assets/`) based on scenes, prefabs, and runtime scripts.

## 1. Core Game Loop

1. Player starts in `SampleScene`.
2. Crossing the hub trigger transitions to `(PGR) Procedurally generated rooms` via `MoveScene`.
3. In each generated room, player must:
   - Defeat spawned enemies.
   - Collect the room objective (`Map` objective item).
4. After a configured number of cleared rooms (`roomsBeforeReturningToHub`, default 3), game can transition to a random boss room (`AllowSwitchToBoss` gate).
5. Boss encounters return player to PGR on completion.
6. If no boss scenes remain in `MoveScene.ListOfAllBossScenes`, transition goes to `Victory`.

## 2. Scenes and Room Types

### 2.1 Enabled Build Scenes

- `Assets/Scenes/(PGR) Procedurally generated rooms.unity`
- `Assets/Scenes/SampleScene.unity`
- `Assets/Scenes/FearLevel.unity`
- `Assets/Scenes/EmilBoss.unity`
- `Assets/Scenes/PierceBoss.unity`
- `Assets/Scenes/HybrizoRoom.unity`
- `Assets/Scenes/ComplacencyRoom.unity`
- `Assets/Scenes/SaahilRoom.unity`
- `Assets/Scenes/Victory.unity`

### 2.2 Hub / Intro Room (`SampleScene`)

- Contains player, menu UI, skill tree UI, dialogue UI, colour picker UI, and scene transition logic.
- Skull dialogue + branching choices are configured with `NPCDialogue` data asset.
- Includes weapon pickups (sword and bow prefabs).

### 2.3 Procedural Room Loop (`(PGR) Procedurally generated rooms`)

- Dungeon generated with random walk algorithm (`SimpleRandomWalkDungeonGenerator`).
- Floors and walls painted to tilemaps (`TilemapVisualizer`, `WallGenerator`).
- `Spawner` populates room with enemies, destructibles, and pickups under spawn-cap rules.
- Room completion requires both enemy clear and objective collection.
- Difficulty scaling after each clear:
  - `iterations += 10`
  - `walkLength += 30`
  - spawn caps and placement attempts increase.

### 2.4 Boss Rooms

- `EmilBoss.unity`: phase boss with shield/action thresholds, projectile throws, orb ring, lightning bursts.
- `ComplacencyRoom.unity`: flee-and-shoot slow projectile boss.
- `PierceBoss.unity`: passive damage aura + random dashes in bounded arena.
- `SaahilRoom.unity`: invulnerable survival encounter (timer win condition).
- `HybrizoRoom.unity`: puzzle-driven vulnerability windows with relocation + projectile pressure.
- `FearLevel.unity`: multi-room torch trap sequence and Devil boss progression via `GurvirLevelController`.

### 2.5 End Scene

- `Victory.unity`: final end-state scene with UI text/canvas.

## 3. Player Feature Set (Michael)

### 3.1 Movement and Facing

- 8-direction movement/facing with direction-indexed animations.
- Optional input delay queue (`inputDelay`) for delayed movement response.
- Crowd-control aware movement lockouts (stun/knockback).

### 3.2 Combat

- Hold `Space` to attack.
- Melee mode:
  - Hit detection via overlap sphere in facing direction.
  - Configurable damage/range/hit radius.
- Ranged mode:
  - Uses equipped projectile prefab via `ProjectileSpawner`.
- Attack windup delay (`attackWindupDelay`) before hit/shot fires.

### 3.3 Defense and Status

- Armor reduces incoming damage (`TakeDamage` subtracts armor).
- Supports:
  - Stun (`IStun`)
  - Knockback (`IKnockBack`)
  - Slow (`ISlowable`)
- Recovers from temporary status timers automatically.

### 3.4 Interaction

- `E` key interacts with nearby interactables.
- Torch interaction supported in radius scan.
- Trigger-based interaction with Skull NPC.

### 3.5 Progression Data on Player

- Tracks objective IDs collected in a hash set.
- Supports additive attack/range bonuses.
- Supports health and armor buffs from items/skills.

## 4. Enemy and Boss Systems

### 4.1 Base Enemy (`Enemy`)

- Aggro/chase behavior toward Michael.
- Attack when in range with cooldown + hit delay.
- Hurt/death state timers and directional animations.
- Awards skill points on death (`+20`).

### 4.2 Boss Base (`Boss`)

- Extends `Enemy` with `bossId` and encounter hooks.
- Default boss death transition returns to PGR via `MoveScene`.

### 4.3 Implemented Boss Behaviors

- `EmilBoss`:
  - Starts dormant until first hit.
  - Shield window: invincible and counts hits received.
  - Action window toggles mechanics based on shield-window hit thresholds:
    - Mirrors/projectile throw (`>=3` hits)
    - Lightning bursts (`>=6` hits)
    - Speed boost (`>=10` hits)
  - Spawns orbiting orb projectiles during shield phase.

- `ComplacencyBoss`:
  - If far enough from player (`fleeRadius`), flees and fires slow projectiles.
  - If close, falls back to base enemy chase/attack behavior.
  - Projectiles apply both damage and slow.

- `PierceBoss`:
  - Constant passive DPS to player every second.
  - Repeating random dash movement with collision-safe clamping within arena walls.
  - Uses wake/hurt animation cadence between dashes.

- `SaahilBoss`:
  - Survival encounter timer (`survivalTime`) instead of kill objective.
  - Boss is invincible (`TakeDamage` ignored).
  - After timer expires, transitions player back to PGR.

- `HybrizoBoss`:
  - Two-state fight: `PuzzleActive` and `WeakWindow`.
  - Puzzle state: periodic relocation among relocation points + projectile cycle.
  - Weak window: vulnerable to damage, slower projectile profile.
  - Supports runtime retargeting and animation-state safety fallbacks.

- `DevilBoss` (FearLevel flow):
  - Directional melee attack behavior with cooldown and range checks.
  - On death triggers return transition toward PGR flow.

## 5. Puzzle / Encounter Controllers

### 5.1 `BossRoomEncounterTrigger`

- Trigger-driven encounter bootstrap.
- Resolves/spawns player and boss, positions them at designated spawn points.
- Enables puzzle controller for encounter-specific flow.

### 5.2 `HybrizoPuzzleController`

- Runs repeating puzzle cycles:
  - Spawns barrel set each cycle.
  - Configures subset to drop boss objective items.
  - Player must collect required drops (`dropsPerCycle`) to solve cycle.
- On cycle completion:
  - Destroys spawned barrels/drops.
  - Signals boss weak window (`OnPuzzleSolved`).
  - Restarts next cycle after weak-window cooldown.

### 5.3 `GurvirLevelController` (FearLevel)

- Orchestrates multi-room torch challenge + boss check.
- Handles room skip option for testing.
- Validates room clear state and transitions forward when conditions are met.

## 6. Objects and Interactables

- `WorldObject` (destructible object):
  - Damageable/killable/interactable/targetable.
  - Can drop objective item prefab on destruction.
- `Torch`:
  - Real torch: lights and reports progress to `TorchManager`.
  - Fake torch trap: repeatedly spawns `SpiritProjectile` toward interactor.
- `TorchManager`:
  - Randomizes which torches are real.
  - Tracks required real torch activations.
  - Expands vision circle as progress feedback.
  - Clears trap spirits and advances room when solved.
- `DamageTile`:
  - Delayed activation damage tile (lightning hazard use-case).

## 7. Projectile and Combat Effects

- `Projectile` base:
  - Team filtering, owner filtering, runtime damage override.
  - Auto-destroy out of bounds.
- `HybrizoProjectile`:
  - Applies damage + stun + knockback.
- `Combat.ComplacencyProjectile`:
  - Applies damage + slow.
- `SpiritProjectile`:
  - Homing projectile used by torch traps.
- `OrbitalProjectile`:
  - Circular orbit around assigned target (Emil shield/orb mechanic).

## 8. Items, Pickups, and Objectives

### 8.1 Generic Item System

- `Item` base auto-collects when Michael collides/enters trigger.
- `Weapon` overrides collection semantics to equip profile (persistent pickup behavior).

### 8.2 Equipment and Consumables

- `MeleeWeapon` (`Sword` prefab): equips melee profile.
- `RangedWeapon` (`Bow` prefab): equips ranged projectile profile.
- `HealingItem` (`HealingCrystal`): restores health.
- `ArmorItem` (`IronArmor`): increases armor.

### 8.3 Objective Items

- `ObjectiveItem`: writes objective ID to Michael and fires collection event.
- `Map` objective: used by room loop completion logic in PGR rooms.
- `BossObjectiveItem`: puzzle-specific objective event for Hybrizo cycles.

## 9. UI and Meta Systems

- `MichaelHealthHUD`:
  - Bootstrapped automatically before scene load.
  - Persistent top-right HUD with current Michael health.
- `MenuToggle`:
  - `M` key opens pause/menu panel.
  - Integrates skill tree panel, resume, quit flow.
- `SkillTreeManager`:
  - Persistent skill-point economy.
  - 3 branches: Damage, Speed, Health (5 tiers each).
  - Enemy/boss kills grant points (notably base `Enemy` and `PierceBoss` give 20).
- `DialogueController` + `NPCDialogue`:
  - Typewriter dialogue, auto-progress lines, branch choices.
- `ColourPickerUI`:
  - Opens from skull dialogue path to recolor player sprite.

## 10. Procedural Generation Pipeline

- `AbstractDungeonGenerator`: generation event and floor position cache.
- `SimpleRandomWalkDungeonGenerator`: random walk room shape generation.
- `ProceduralGenerationAlgorithms`: direction sets + random walk step logic.
- `TilemapVisualizer`: paints floor and wall tiles from generated coordinates.
- `WallGenerator` + `WallTypesHelper`: bitmask-based wall tile selection.
- `Spawner`: room population, objective placement, room clear progression, difficulty scaling.

## 11. Current Prefab Inventory (Gameplay-Relevant)

### 11.1 Boss Prefabs

- `Assets/Prefabs/Bosses/GurvirBoss.prefab`
- `Assets/Prefabs/Bosses/HybrizoGui/Hybrizo.prefab`
- `Assets/Prefabs/Bosses/KrishBoss.prefab`
- `Assets/Prefabs/Bosses/PierceBoss.prefab`
- `Assets/Prefabs/Bosses/SaahilBoss.prefab`

### 11.2 Enemy/Projectile Prefabs

- `Assets/Prefabs/Enemies/Zombie.prefab`
- `Assets/Prefabs/Enemies/Lightning.prefab`
- `Assets/Prefabs/Bosses/HybrizoGui/ArrowProjectile.prefab`
- `Assets/Prefabs/GurvirFearLevelPrefabs/SpiritProjectile.prefab`

### 11.3 Item Prefabs

- `Assets/Prefabs/Items/HealingCrystal.prefab`
- `Assets/Prefabs/Items/Armor/IronArmor.prefab`
- `Assets/Prefabs/Items/Weapons/Sword.prefab`
- `Assets/Prefabs/Items/Weapons/Bow.prefab`
- `Assets/Prefabs/SpawnObjects/Map.prefab`

### 11.4 Puzzle / Object Prefabs

- `Assets/Prefabs/SpawnObjects/Barrel.prefab`
- `Assets/Prefabs/Bosses/HybrizoGui/HybrizoBarrelSpawn.prefab`
- `Assets/Prefabs/GurvirFearLevelPrefabs/Torch.prefab`
- `Assets/Prefabs/GurvirFearLevelPrefabs/Room1.prefab`
- `Assets/Prefabs/GurvirFearLevelPrefabs/Room2.prefab`
- `Assets/Prefabs/GurvirFearLevelPrefabs/FearLevelController.prefab`

### 11.5 Character/NPC/UI Prefabs

- `Assets/Prefabs/Michael.prefab`
- `Assets/Prefabs/Npc/Skull.prefab`
- `Assets/Prefabs/ChoiceButton.prefab`

## 12. Known Implementation Notes (Current State)

- `Skull` prefab currently references `Assembly-CSharp::Skull` in prefab data; runtime code class present in project is `SkullNPC`.
- `GurvirBoss.prefab` currently appears to be visual/physics setup without an attached boss behavior script component in prefab file.
- `MoveScene.ListOfAllBossScenes` default list includes duplicate `EmilBoss` entry and drives random boss selection/removal order at runtime.

