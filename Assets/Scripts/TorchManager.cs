using System.Collections.Generic;
using System.Linq;
using UnityEditor.Search;
using UnityEngine;

public class TorchManager : MonoBehaviour
{
    
    [SerializeField] Michael michael;
    private List<Torch> _torches;
    public int realTorchesNeeded = 3;
    private int realTorchesFound = 0;

    public int room;
    
    public bool torchesCleared = false;

    // references for updating the visual conditions
    private LightFlicker lf;
    [SerializeField] private GameObject visionCircle;
    
    void Awake()
    {  
        // Find Micheal script
        if (michael == null)
            michael = Object.FindFirstObjectByType<Michael>();
        
        // store reference to torches
        _torches = GetComponentsInChildren<Torch>().ToList();
        if (_torches.Count < 9)
        {
            room = 1;
        } else if (_torches.Count > 9)
        {
            room = 2;
        }
            
        RandomizeTorches();
        
        lf = visionCircle.GetComponent<LightFlicker>();
        
    }
    

    void RandomizeTorches()
    {
        if (_torches.Count < realTorchesNeeded)
        {
            Debug.LogWarning("Not enough torches in the list to meet the requirement!");
            return;
        }

        // Assign this manager to all torches and reset them
        foreach (var torch in _torches)
        {
            torch.manager = this;
            torch.IsReal = false;
        }

        // Shuffle and pick as many as needed. (OrderBy uses a random GUID to shuffle the list)
        var shuffled = _torches.OrderBy(t => System.Guid.NewGuid()).ToList();

        for (int i = 0; i < realTorchesNeeded; i++)
        {
            shuffled[i].IsReal = true;
        }
    }

    // Handle real torches activated
    public void OnTorchActivated(Torch torch)
    {
        // Updates when real torch is interacted with
        realTorchesFound++;
        
        // Improving visibility
        float vis = 2f;
        lf.baseScale.x += vis;
        lf.baseScale.y += vis;
        
        
        if (realTorchesFound >= realTorchesNeeded)
        {
            RoomCleared();
        }
    }
    void RoomCleared()
    {
        Debug.Log("Room Cleared! Stopping all traps.");
        foreach (var torch in _torches)
        {
            torch.StopAttacking();
        }
        
        
        // Destroy Active Projectiles
        for (int i = SpiritProjectile.ActiveSpirits.Count - 1; i >= 0; i--)
        {
            Destroy(SpiritProjectile.ActiveSpirits[i].gameObject);
        }

        if (room == 1)
        {
            michael.transform.position = new Vector3(53f, -40f, 1f);
        }
        // Trigger LevelController here to open door or whatever
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
