using UnityEngine;

public class Torch : MonoBehaviour, IInteractable
{ 
    public bool IsReal { get; set; } = false; // Set true by TorchManager
    private bool _isActive = false; // Interacted with yet?
    
    private Animator _anim;
    private GameObject _target;
    
    // For attacks
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float spawnRate = 2.5f;

    public TorchManager manager;
    
    public void Interact(GameObject interactor)
    {
        if (_isActive) return;
        
        // assigned private variable for attack
        _target =  interactor;

        // set new state
        _isActive = true;
        
        // one-time animation change if real
        if (IsReal)
        {
            Debug.Log("animation change triggered");
            _anim.SetTrigger("isLit");
        }
        else
        {
            // If it's a fake/trap torch, start the spawning loop
            InvokeRepeating(nameof(SpawnSpirit), 1f, spawnRate);
        }

    }

    void Awake()
    {
        _anim = GetComponent<Animator>();
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SpawnSpirit()
    {
        if (_target == null) return;

        GameObject go = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        SpiritProjectile spirit = go.GetComponent<SpiritProjectile>();
        
        if (spirit != null)
        {
            spirit.Setup(_target);
        }
    }

}
