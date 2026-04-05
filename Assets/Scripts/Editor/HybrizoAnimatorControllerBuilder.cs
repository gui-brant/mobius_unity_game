using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class HybrizoAnimatorControllerBuilder
{
    private const string ControllerPath = "Assets/Visuals/Animations/Bosses/HybrizoGui/Hybrizo.controller";
    private const string ClipsFolderPath = "Assets/Visuals/Animations/Bosses/HybrizoGui";
    private const string ImportedSpritesRoot = "Assets/ImportedAssets/Bosses/HybrizoGui";
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

    private readonly struct ClipSpec
    {
        public readonly string ClipName;
        public readonly string SpriteFolder;
        public readonly string SpritePrefix;
        public readonly int Angle;
        public readonly bool Loop;

        public ClipSpec(string clipName, string spriteFolder, string spritePrefix, int angle, bool loop)
        {
            ClipName = clipName;
            SpriteFolder = spriteFolder;
            SpritePrefix = spritePrefix;
            Angle = angle;
            Loop = loop;
        }
    }

    private static readonly ClipSpec[] ClipSpecs =
    {
        // Shooting
        new ClipSpec("shooting0_right", "Attack_Bow", "Attack_Bow_Body_", 0, true),
        new ClipSpec("shooting1_up_right", "Attack_Bow", "Attack_Bow_Body_", 45, true),
        new ClipSpec("shooting2_up", "Attack_Bow", "Attack_Bow_Body_", 90, true),
        new ClipSpec("shooting3_up_left", "Attack_Bow", "Attack_Bow_Body_", 135, true),
        new ClipSpec("shooting4_left", "Attack_Bow", "Attack_Bow_Body_", 180, true),
        new ClipSpec("shooting5_down_left", "Attack_Bow", "Attack_Bow_Body_", 225, true),
        new ClipSpec("shooting6_down", "Attack_Bow", "Attack_Bow_Body_", 270, true),
        new ClipSpec("shooting7_down_right", "Attack_Bow", "Attack_Bow_Body_", 315, true),

        // Running
        new ClipSpec("run0_right", "Run_Bow", "Run_Bow_Body_", 0, true),
        new ClipSpec("running0_right", "Run_Bow", "Run_Bow_Body_", 0, true),
        new ClipSpec("run1_up_right", "Run_Bow", "Run_Bow_Body_", 45, true),
        new ClipSpec("run2_up", "Run_Bow", "Run_Bow_Body_", 90, true),
        new ClipSpec("run3_up_left", "Run_Bow", "Run_Bow_Body_", 135, true),
        new ClipSpec("run4_left", "Run_Bow", "Run_Bow_Body_", 180, true),
        new ClipSpec("run5_down_left", "Run_Bow", "Run_Bow_Body_", 225, true),
        new ClipSpec("run6_down", "Run_Bow", "Run_Bow_Body_", 270, true),
        new ClipSpec("run7_down_right", "Run_Bow", "Run_Bow_Body_", 315, true),

        // Death
        new ClipSpec("die", "Death_Bow", "Death_Bow_Body_", 0, false)
    };

    [MenuItem("Tools/Hybrizo/Rebuild Animator Controller")]
    public static void RebuildController()
    {
        RebuildClips();

        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
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

    [InitializeOnLoadMethod]
    private static void EnsureValidControllerOnEditorLoad()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null || !HasRequiredStates(controller) || !HasUsableClips())
            {
                RebuildController();
            }
        };
    }

    [MenuItem("Tools/Hybrizo/Rebuild Clips + Controller")]
    public static void RebuildClipsAndController()
    {
        RebuildController();
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

    private static bool HasRequiredStates(AnimatorController controller)
    {
        if (controller == null || controller.layers == null || controller.layers.Length == 0)
        {
            return false;
        }

        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        if (stateMachine == null)
        {
            return false;
        }

        HashSet<string> names = new HashSet<string>();
        ChildAnimatorState[] states = stateMachine.states;
        for (int i = 0; i < states.Length; i++)
        {
            if (states[i].state != null)
            {
                names.Add(states[i].state.name);
            }
        }

        for (int i = 0; i < RequiredStateNames.Length; i++)
        {
            if (!names.Contains(RequiredStateNames[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool HasUsableClips()
    {
        for (int i = 0; i < ClipSpecs.Length; i++)
        {
            string clipPath = $"{ClipsFolderPath}/{ClipSpecs[i].ClipName}.anim";
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (clip == null)
            {
                return false;
            }

            EditorCurveBinding spriteBinding = EditorCurveBinding.PPtrCurve(string.Empty, typeof(SpriteRenderer), "m_Sprite");
            ObjectReferenceKeyframe[] keys = AnimationUtility.GetObjectReferenceCurve(clip, spriteBinding);
            if (keys == null || keys.Length == 0)
            {
                return false;
            }
        }

        return true;
    }

    private static void RebuildClips()
    {
        AssetDatabase.StartAssetEditing();
        try
        {
            for (int i = 0; i < ClipSpecs.Length; i++)
            {
                RebuildClip(ClipSpecs[i]);
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    private static void RebuildClip(ClipSpec spec)
    {
        string clipPath = $"{ClipsFolderPath}/{spec.ClipName}.anim";
        string spriteTexturePath = $"{ImportedSpritesRoot}/{spec.SpriteFolder}/{spec.SpritePrefix}{spec.Angle:000}.png";
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(spriteTexturePath);
        Sprite[] sprites = assets.OfType<Sprite>().OrderBy(sprite => sprite.name).ToArray();

        if (sprites.Length == 0)
        {
            Debug.LogWarning($"No sprites found for clip '{spec.ClipName}' at '{spriteTexturePath}'.");
            return;
        }

        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        if (clip == null)
        {
            clip = new AnimationClip
            {
                name = spec.ClipName,
                frameRate = 60f
            };

            AssetDatabase.CreateAsset(clip, clipPath);
        }

        EditorCurveBinding spriteBinding = EditorCurveBinding.PPtrCurve(string.Empty, typeof(SpriteRenderer), "m_Sprite");
        ObjectReferenceKeyframe[] keys = new ObjectReferenceKeyframe[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            keys[i] = new ObjectReferenceKeyframe
            {
                time = i / 60f,
                value = sprites[i]
            };
        }

        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keys);
        SetLooping(clip, spec.Loop);
        EditorUtility.SetDirty(clip);
    }

    private static void SetLooping(AnimationClip clip, bool shouldLoop)
    {
        SerializedObject serializedClip = new SerializedObject(clip);
        SerializedProperty clipSettings = serializedClip.FindProperty("m_AnimationClipSettings");
        if (clipSettings != null)
        {
            SerializedProperty loopTime = clipSettings.FindPropertyRelative("m_LoopTime");
            if (loopTime != null)
            {
                loopTime.boolValue = shouldLoop;
            }
        }

        serializedClip.ApplyModifiedProperties();
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
