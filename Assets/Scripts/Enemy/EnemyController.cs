using UnityEngine;

public class EnemyController : MonoBehaviour
{
    
    // Creating rigidbody and animator references 
    private Rigidbody2D _rigidBody;
    private Animator _animator;
    
    // Creating reference to store applied movement strategy
    private IMovementStrategy _movementStrategy;
    
    // 1. Setting rigidbody and animator reference as soon as possible
    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Check for null exceptions here maybe
    }

    // Used by factory
    public void Initialize(IMovementStrategy movementStrategy)
    {
        _movementStrategy = movementStrategy;
    }
    
    void FixedUpdate()
    {
        _movementStrategy?.Execute(_rigidBody, 0.5f);
    }
    
    // Update is called once per frame
    
    void Update()
    {
        // Handle additional logic/inputs/timers
    }
}
