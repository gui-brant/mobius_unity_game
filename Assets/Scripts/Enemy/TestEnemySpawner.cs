using UnityEngine;
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject followTarget;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initializing test enemy and attaching new strategy with reference to player
        GameObject enemy = Instantiate(enemyPrefab);
        EnemyController controller = enemy.GetComponent<EnemyController>();
        controller.Initialize(new FollowTargetStrategy(followTarget.GetComponent<Transform>()));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
