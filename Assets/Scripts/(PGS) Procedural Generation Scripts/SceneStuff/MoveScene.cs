using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

public class MoveScene : MonoBehaviour
{

    // Keeping this static so it persists, but we need to find it first
    static private GameObject prefabInstance;
    private Character michaelScript; // Declare this here without assigning yet

    [Header("Scene Names")]
    public string pgrSceneName = "(PGR) Procedurally generated rooms";
    public string sampleSceneName = "SampleScene";

    private bool isTransitioning = false;
    private void Awake(){ DontDestroyOnLoad(this.gameObject);}
    void Update()
    {
        
        // 1. FIRST: Find the Player if we don't have it
        if (prefabInstance == null)
        {
            prefabInstance = GameObject.FindWithTag("Player");
            if (prefabInstance == null) return; // Exit and wait for next frame if not found
        }

        // 2. SECOND: Get the Character script from the found player
        if (michaelScript == null)
        {
            michaelScript = prefabInstance.GetComponent<Character>();
        }
        
        if (prefabInstance == null || michaelScript == null || michaelScript.isDead)
        {
            Character[] allCharacters = FindObjectsByType<Character>(FindObjectsSortMode.None);

            foreach (Character c in allCharacters)
            {
                Debug.Log(c.transform.position);
                Vector3 pos = c.transform.position;
                Vector3 min = new Vector3(-3.5f, -5f, 0f);
                Vector3 max = new Vector3(-2f, -3f, 0f);

                if (pos.x >= min.x && pos.x <= max.x &&
                    pos.y >= min.y && pos.y <= max.y)
                {
                    prefabInstance = c.gameObject;
                    michaelScript = c;
                    break;
                }
            }
        }
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

    IEnumerator MoveToPGR()
    {
        Debug.Log("<color=yellow>Entering PGR...</color>");
        yield return StartCoroutine(TransitionProcess(sampleSceneName, pgrSceneName));
        isTransitioning = false;
    }

    public IEnumerator MoveBackToSample()
    {
        Debug.Log("<color=cyan>Returning to Sample Scene...</color>");
        yield return StartCoroutine(TransitionProcess(pgrSceneName, sampleSceneName));
        isTransitioning = false;
    }

    IEnumerator TransitionProcess(string fromScene, string toScene)
    {
        
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(toScene, LoadSceneMode.Additive);
        while (!loadOp.isDone) yield return null;

        Scene targetScene = SceneManager.GetSceneByName(toScene);

        prefabInstance.transform.parent = null;
        SceneManager.MoveGameObjectToScene(prefabInstance, targetScene);
        SceneManager.SetActiveScene(targetScene);

        if (Camera.main != null)
        {
            Vector3 pos = prefabInstance.transform.position;
            Camera.main.transform.position = new Vector3(pos.x, pos.y, -20f);
        }

        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(fromScene);
        while (!unloadOp.isDone) yield return null;

        Debug.Log($"<color=green>Successfully moved to {toScene}</color>");
        if (toScene == "(PGR) Procedurally generated rooms") { prefabInstance.transform.position = new Vector3(0f,0f,0f); }
        else if (toScene == "SampleScene") { prefabInstance.transform.position = new Vector3(0f,-4.6f,1); }
    }
}