using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HybrizoBoss))]
public class HybrizoBossEditor : Editor
{
    private bool showRuntimeDebug;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawScriptField();
        DrawStatsSection();
        DrawReferencesSection();
        DrawRelocationSection();
        DrawPuzzleActiveSection();
        DrawWeakWindowSection();
        DrawRuntimeDebugSection();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawScriptField()
    {
        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((HybrizoBoss)target), typeof(MonoScript), false);
        }
    }

    private void DrawStatsSection()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Stats", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("health"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("speed"));
    }

    private void DrawReferencesSection()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("References", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("targetMichael"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("projectileSpawner"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("projectileOrigin"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("bossProjectilePrefab"));
    }

    private void DrawRelocationSection()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Relocation", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(
            serializedObject.FindProperty("relocationIntervalSeconds"),
            new GUIContent("Stationary Shoot Duration", "How long Hybrizo remains stationary and shooting before starting the next relocation."));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("relocationSpeed"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("relocationStopDistance"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("michaelExclusionRadius"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("relocationPoints"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("relocationGraceSeconds"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("nearTargetRadius"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("nearTargetExtensionSeconds"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("relocationSettleRadius"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("relocationStuckGraceSeconds"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("relocationProgressEpsilon"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("relocationArrivalLockSeconds"));
    }

    private void DrawPuzzleActiveSection()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Projectile Stats - Puzzle Active", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("puzzleActiveFireInterval"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("puzzleActiveProjectileSpeed"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("puzzleActiveProjectileDamage"));
    }

    private void DrawWeakWindowSection()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Projectile Stats - Weak Window", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("weakWindowFireInterval"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("weakWindowProjectileSpeed"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("weakWindowProjectileDamage"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("runAnimationSpeed"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("shootCycleToleranceSeconds"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("invertDirectionalAnimation"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("enableAimDebug"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("aimDebugDurationSeconds"));
    }

    private void DrawRuntimeDebugSection()
    {
        EditorGUILayout.Space();
        showRuntimeDebug = EditorGUILayout.Foldout(showRuntimeDebug, "Runtime Debug", true);
        if (!showRuntimeDebug)
        {
            return;
        }

        using (new EditorGUI.DisabledScope(true))
        {
            HybrizoBoss boss = (HybrizoBoss)target;
            EditorGUILayout.TextField("Animation Mode", boss.DebugAnimationMode ?? string.Empty);
            EditorGUILayout.IntField("Direction Index", boss.DebugDirectionIndex);
            EditorGUILayout.Toggle("Relocating", boss.DebugIsRelocating);
            EditorGUILayout.FloatField("Remaining Distance", boss.DebugRemainingDistance);
            EditorGUILayout.Vector2Field("Relocation Destination", boss.DebugRelocationDestination);
            EditorGUILayout.FloatField("Run Travel Duration", boss.DebugRunTravelDuration);
            EditorGUILayout.FloatField("Run Animation Speed", boss.DebugRunAnimationSpeed);
            EditorGUILayout.FloatField("Shoot Cycle Duration", boss.DebugShootCycleDuration);
            EditorGUILayout.FloatField("Shoot Cycle Elapsed", boss.DebugShootCycleElapsed);
            EditorGUILayout.FloatField("Shoot Animation Speed", boss.DebugShootAnimationSpeed);
            EditorGUILayout.TextField("Requested Anim State", boss.DebugRequestedAnimationState ?? string.Empty);
            EditorGUILayout.TextField("Resolved Anim State", boss.DebugResolvedAnimationState ?? string.Empty);
        }
    }
}
