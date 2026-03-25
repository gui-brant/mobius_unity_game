using UnityEngine;

public class TestEnemySpawner2 : MonoBehaviour
{
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private Michael targetMichael;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initializing test enemy and attaching new strategy with reference to player
        Enemy enemy = Instantiate(enemyPrefab);
        enemy.Init(targetMichael);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
