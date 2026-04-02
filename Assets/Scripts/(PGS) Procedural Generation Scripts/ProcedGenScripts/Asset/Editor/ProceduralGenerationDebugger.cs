using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


[CustomEditor(typeof(Spawner), true)]
public class ProceduralGenerationDebugger : Editor
{
    [SerializeField]
    //the prefab of the map
    GameObject objectivePrefab;

    Spawner spawner;
    // when script is first loaded, before the game starts
    public void Awake()
    {
        //
        spawner = (Spawner)target; // target is the object in unity when it is selected in Hierarchy

    }
    //in the inspector
    public override void OnInspectorGUI()
    {
        objectivePrefab = (GameObject)EditorGUILayout.ObjectField("Objective Prefab", objectivePrefab, typeof(GameObject), false);

        base.OnInspectorGUI();
        if (GUILayout.Button("Kill all enemies")/*create button */) // which if presed
        {
            // Finds all Enemy components in the scene
            Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

            foreach (Enemy enemy in enemies)
            {
                if (enemy.gameObject.name == "Zombie(Clone)")
                {
                    Destroy(enemy.gameObject);
                }
            }
        }
        if (GUILayout.Button("Destroy all barrels")/*create button */) // which if presed
        {
            // Finds all Enemy components in the scene
            WorldObject[] barrels = FindObjectsByType<WorldObject>(FindObjectsSortMode.None);

            foreach (WorldObject barrel in barrels)
            {
                if (barrel.gameObject.name == "Barrel(Clone)")
                {
                    barrel.ConfigureObjectiveDrop(objectivePrefab);
                    barrel.SpawnObjectiveDrop();
                    Destroy(barrel.gameObject);
                }
            }
        }
        
        if (GUILayout.Button("Spawn all items again")/*create button */) // which if presed
        {
            WorldObject[] barrels = FindObjectsByType<WorldObject>(FindObjectsSortMode.None);
            spawner.hasSpawnedForCurrentRoom = false;  
            spawner.TrySpawnWhenReady();
        }
        


    }
}
