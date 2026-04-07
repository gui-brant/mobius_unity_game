using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class MoveScene : MonoBehaviour
{
    private static MoveScene instance;

    private GameObject prefabInstance;
    private Character michaelScript; // Declare this here without assigning yet

    [Header("Scene Names")]
    public string pgrSceneName = "(PGR) Procedurally generated rooms";
    [Header("Scene Names")]
    public List<String> ListOfAllBossScenes = new List<String> { "EmilBoss" };
    [Header("Scene Names")]
    public string sampleSceneName = "SampleScene";
    public Vector3 cameraOffset = new Vector3(-5f, 0f, 0f);

    private bool isTransitioning = false;
    public int BossSceneForTransition;
    public bool once;
    public EmilBoss EmilBoss;
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    void Update()
    {
        if (!TryGetControllablePlayer()) return;

        // 3. THIRD: Check health (Only if we successfully found the script)
        if (michaelScript != null)
        {
            
            // Usually we check for <= 0 for death
            if (michaelScript.health == 0)
            {



               
                //StartCoroutine(MoveBackToSample());
                return; // Stop here for this frame
            }
        }

        // 4. FOURTH: Movement Logic
        Vector3 currentPos = prefabInstance.transform.position;
        string currentActiveScene = SceneManager.GetActiveScene().name;
        FollowActivePlayerWithCamera(currentPos);

        if (!isTransitioning)
        {
            if (currentActiveScene == sampleSceneName)
            {
                if (currentPos.x > 2.4f && currentPos.y > -3f)
                {
                    isTransitioning = true;
                    StartCoroutine(MoveToPGR());
                }
            }
            
        }
        

    }

    private void FollowActivePlayerWithCamera(Vector3 playerPosition)
    {
        Camera main = Camera.main;
        if (main == null || !main.isActiveAndEnabled)
        {
            Camera[] allCameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
            for (int i = 0; i < allCameras.Length; i++)
            {
                if (allCameras[i] == null || !allCameras[i].isActiveAndEnabled) continue;
                main = allCameras[i];
                break;
            }
        }

        if (main == null) return;
        main.transform.position = playerPosition + cameraOffset;
    }

    private bool TryGetControllablePlayer()
    {
        if (prefabInstance != null && michaelScript != null && !michaelScript.IsDead && prefabInstance.activeInHierarchy)
        {
            return true;
        }

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject fallbackPlayer = null;
        Character fallbackCharacter = null;

        for (int i = 0; i < players.Length; i++)
        {
            GameObject candidate = players[i];
            if (candidate == null || !candidate.activeInHierarchy) continue;

            Character candidateCharacter = candidate.GetComponent<Character>();
            if (candidateCharacter == null) continue;

            if (!candidateCharacter.IsDead)
            {
                prefabInstance = candidate;
                michaelScript = candidateCharacter;
                return true;
            }

            if (fallbackPlayer == null)
            {
                fallbackPlayer = candidate;
                fallbackCharacter = candidateCharacter;
            }
        }

        prefabInstance = fallbackPlayer;
        michaelScript = fallbackCharacter;
        return prefabInstance != null && michaelScript != null;
    }

    public IEnumerator MoveToPGR()
    {
        Debug.Log("<color=yellow>Entering PGR...</color>");
        yield return StartCoroutine(TransitionProcess(pgrSceneName));
        isTransitioning = false;
    }

    public IEnumerator MoveBackToSample()
    {
        Debug.Log("<color=cyan>Returning to Sample Scene...</color>");
        yield return StartCoroutine(TransitionProcess(sampleSceneName));
        isTransitioning = false;
    }
    public IEnumerator MoveToRandomBossRoom()
    {
        BossSceneForTransition = Random.Range(0,1);

        Debug.Log("<color=cyan>Entering random boss room...</color>");
        Debug.Log(ListOfAllBossScenes[BossSceneForTransition]);
        yield return StartCoroutine(TransitionProcess(ListOfAllBossScenes[BossSceneForTransition]));
        isTransitioning = false;
    }

    public IEnumerator  TransitionProcess(string toScene)
    {
        Scene destinationScene = SceneManager.GetSceneByName(toScene);
        if (!destinationScene.isLoaded)
        {
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(toScene, LoadSceneMode.Additive);
            if (loadOp != null)
            {
                while (!loadOp.isDone) yield return null;
            }
        }

        Scene targetScene = SceneManager.GetSceneByName(toScene);
        if (!targetScene.IsValid() || !targetScene.isLoaded)
        {
            Debug.LogError($"Failed to load target scene '{toScene}'.");
            isTransitioning = false;
            yield break;
        }

        if (prefabInstance != null)
        {
            Character targetSceneAlivePlayer = FindAlivePlayerInScene(targetScene);
            if (targetSceneAlivePlayer != null && targetSceneAlivePlayer.gameObject != prefabInstance)
            {
                if (michaelScript != null && michaelScript.IsDead)
                {
                    Destroy(prefabInstance);
                    prefabInstance = targetSceneAlivePlayer.gameObject;
                    michaelScript = targetSceneAlivePlayer;
                }
                else
                {
                    Destroy(targetSceneAlivePlayer.gameObject);
                    prefabInstance.transform.parent = null;
                    SceneManager.MoveGameObjectToScene(prefabInstance, targetScene);
                }
            }
            else
            {
                prefabInstance.transform.parent = null;
                SceneManager.MoveGameObjectToScene(prefabInstance, targetScene);
            }
        }
        SceneManager.SetActiveScene(targetScene);

        if (prefabInstance != null)
        {
            FollowActivePlayerWithCamera(prefabInstance.transform.position);
        }

        yield return StartCoroutine(UnloadNonTargetGameplayScenes(toScene));

        Debug.Log($"<color=green>Successfully moved to {toScene}</color>");
        if (prefabInstance != null)
        {
            if (toScene == "(PGR) Procedurally generated rooms") { prefabInstance.transform.position = new Vector3(0f,0f,0f); }
            else if (toScene == "SampleScene") { prefabInstance.transform.position = new Vector3(0f,-4.6f,1); }
        }
    }

    public IEnumerator UnloadNonTargetGameplayScenes(string targetSceneName)
    {
        for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.IsValid() || !scene.isLoaded) continue;
            if (scene.name == targetSceneName) continue;

            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(scene);
            if (unloadOp != null)
            {
                while (!unloadOp.isDone) yield return null;
            }
        }
    }

    private Character FindAlivePlayerInScene(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded)
        {
            return null;
        }

        GameObject[] rootObjects = scene.GetRootGameObjects();
        for (int i = 0; i < rootObjects.Length; i++)
        {
            Character[] characters = rootObjects[i].GetComponentsInChildren<Character>(true);
            for (int j = 0; j < characters.Length; j++)
            {
                Character character = characters[j];
                if (character == null || !character.gameObject.activeInHierarchy) continue;
                if (!character.CompareTag("Player")) continue;
                if (character.IsDead) continue;
                return character;
            }
        }

        return null;
    }
}
