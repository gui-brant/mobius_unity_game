using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TorchManager : MonoBehaviour
{
    private List<Torch> _torches;
    
    void Awake()
    {  
        // store
        _torches = GetComponentsInChildren<Torch>().ToList();
        Debug.Log("Torches found: " + _torches.Count);
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
