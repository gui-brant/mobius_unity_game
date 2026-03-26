using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MoveScene : MonoBehaviour
{
    // We leave this hidden or empty; the script will find it automatically
    private GameObject prefabInstance;

    public string targetSceneName = "(PGR) Procedurally generated rooms";
    public string sceneToUnload = "SampleScene";

    private bool isTransitioning = false;

    void Update()
    {
        // AUTOMATIC FINDER: 
        // If the slot is empty, look for the object tagged "Player"
        if (prefabInstance == null)
        {
            prefabInstance = GameObject.FindWithTag("Player");

            // If we still can't find it, stop here so we don't crash
            if (prefabInstance == null) return;

            Debug.Log($"<color=cyan>Found Player:</color> {prefabInstance.name} at {prefabInstance.transform.position}");
        }

        Vector3 currentPos = prefabInstance.transform.position;

        // Log coordinates so you can see them moving in the console
        // (Only logs if you are actually moving)
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            Debug.Log($"Tracking: {currentPos.x:F2}, {currentPos.y:F2}");
        }

        if (!isTransitioning)
        {
            // Update these to coordinates you WANT to reach (away from your start point)
            if (currentPos.x > 2.4f && currentPos.y > -3f )
            {
                isTransitioning = true;
                StartCoroutine(MoveAndUnload());
            }
        }
    }

    IEnumerator MoveAndUnload()
    {
        Debug.Log("<color=yellow>Transitioning Scenes...</color>");

        AsyncOperation loadOp = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive);
        while (!loadOp.isDone) yield return null;

        Scene targetScene = SceneManager.GetSceneByName(targetSceneName);

        // Disconnect from any parents (like a Grid) before moving
        prefabInstance.transform.parent = null;
        SceneManager.MoveGameObjectToScene(prefabInstance, targetScene);
        SceneManager.SetActiveScene(targetScene);

        // Move Camera to the player's world position
        if (Camera.main != null)
        {
            Vector3 pos = prefabInstance.transform.position;
            Camera.main.transform.position = new Vector3(pos.x, pos.y, -20f);
        }

        SceneManager.UnloadSceneAsync(sceneToUnload);
    }
}