using System.Collections.Generic;
using System.Linq;
using UnityEditor.Search;
using UnityEngine;

public class TorchManager : MonoBehaviour
{
    private List<Torch> _torches;
    public int realTorchesNeeded = 3;
    private int realTorchesFound = 0;

    // references for updating the visual conditions
    private LightFlicker lf;
    [SerializeField] private GameObject visionCircle;
    
    void Awake()
    {  
        // store reference to torches
        _torches = GetComponentsInChildren<Torch>().ToList();
        RandomizeTorches();
        
        lf = visionCircle.GetComponent<LightFlicker>();
        
    }
    

    void RandomizeTorches()
    {
        if (_torches.Count < realTorchesNeeded)
        {
            Debug.LogError("Not enough torches in the list to meet the requirement!");
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
