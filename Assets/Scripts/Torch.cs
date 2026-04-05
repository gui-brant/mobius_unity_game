using UnityEngine;

public class Torch : MonoBehaviour, IInteractable
{

    // used to determine if friendly or not
    private bool IsReal { get; set; } = true;
    
    // represents if player has interacted with it
    private bool IsActive { get; set; }

    private Animator _anim;

    private GameObject _target;

    public void Interact(GameObject interactor)
    {
        if (IsActive) return;
        
        // assigned private variable for attack
        _target =  interactor;

        // set new state
        IsActive = true;
        
        if (IsReal)
        {
            Debug.Log("animation change triggered");
            _anim.SetTrigger("isLit");
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
}
