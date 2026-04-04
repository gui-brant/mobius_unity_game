using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class HybrizoAnimatorControllerBuilder
{
    private const string ControllerPath = "Assets/Visuals/Animations/Bosses/HybrizoGui/Hybrizo.controller";
    private const string ClipsFolderPath = "Assets/Visuals/Animations/Bosses/HybrizoGui";
    private const string PrefabPath = "Assets/Prefabs/Bosses/HybrizoGui/Hybrizo.prefab";

    private static readonly string[] RequiredStateNames =
    {
        "die",
        "shooting0_right",
        "shooting1_up_right",
        "shooting2_up",
        "shooting3_up_left",
        "shooting4_left",
        "shooting5_down_left",
        "shooting6_down",
        "shooting7_down_right",
        "run0_right",
        "running0_right",
        "run1_up_right",
        "run2_up",
        "run3_up_left",
        "run4_left",
        "run5_down_left",
        "run6_down",
        "run7_down_right"
    };

    [MenuItem("Tools/Hybrizo/Rebuild Animator Controller")]
    public static void RebuildController()
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
        {
            Debug.LogError($"Hybrizo controller not found at '{ControllerPath}'.");
            return;
        }

        Dictionary<string, AnimationClip> clipsByName = LoadClipsByName();
        if (clipsByName.Count == 0)
        {
            Debug.LogError($"No clips found under '{ClipsFolderPath}'.");
            return;
        }

        Undo.RegisterCompleteObjectUndo(controller, "Rebuild Hybrizo Animator Controller");
        controller.layers = new[] { BuildLayer(clipsByName) };
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();

        AssignControllerToPrefab(controller);

        Debug.Log("Hybrizo Animator Controller rebuilt and assigned.");
    }

    private static AnimatorControllerLayer BuildLayer(Dictionary<string, AnimationClip> clipsByName)
    {
        AnimatorStateMachine stateMachine = new AnimatorStateMachine
        {
            name = "Base Layer"
        };
        AssetDatabase.AddObjectToAsset(stateMachine, ControllerPath);

        AnimatorState defaultState = null;
        for (int i = 0; i < RequiredStateNames.Length; i++)
        {
            string stateName = RequiredStateNames[i];
            if (!clipsByName.TryGetValue(stateName, out AnimationClip clip))
            {
                continue;
            }

            AnimatorState state = stateMachine.AddState(stateName);
            state.motion = clip;
            state.writeDefaultValues = true;

            if (stateName == "shooting0_right")
            {
                defaultState = state;
            }
        }

        if (defaultState == null && stateMachine.states.Length > 0)
        {
            defaultState = stateMachine.states[0].state;
        }

        stateMachine.defaultState = defaultState;

        return new AnimatorControllerLayer
        {
            name = "Base Layer",
            stateMachine = stateMachine,
            defaultWeight = 1f
        };
    }

    private static Dictionary<string, AnimationClip> LoadClipsByName()
    {
        Dictionary<string, AnimationClip> clips = new Dictionary<string, AnimationClip>();
        string[] guids = AssetDatabase.FindAssets("t:AnimationClip", new[] { ClipsFolderPath });
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
            {
                continue;
            }

            clips[clip.name] = clip;
        }

        return clips;
    }

    private static void AssignControllerToPrefab(RuntimeAnimatorController controller)
    {
        GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefabAsset == null)
        {
            Debug.LogWarning($"Hybrizo prefab not found at '{PrefabPath}'.");
            return;
        }

        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            Animator animator = prefabRoot.GetComponent<Animator>();
            if (animator == null)
            {
                animator = prefabRoot.AddComponent<Animator>();
            }

            animator.runtimeAnimatorController = controller;
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
    }
}
