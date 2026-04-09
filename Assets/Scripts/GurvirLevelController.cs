using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class GurvirLevelController : MonoBehaviour
{

    [SerializeField] private Michael michael;
    [SerializeField] private DevilBoss boss;

    [SerializeField] private TorchManager torchesRoom1;
    
    // Used to determine success in room two
    [SerializeField] private TorchManager torchesRoom2;

    [SerializeField] private bool SkipRoom1 = false;
    
    [SerializeField] MoveScene moveScene;
    private bool once = false;

    private Renderer _renderer;
    private string _initSortingLayerName;
    private int _initOrderInLayer;
    
    void Awake()
    {
        // Find Micheal script
        if (michael == null)
            michael = Object.FindFirstObjectByType<Michael>();
        
        _renderer = michael.GetComponent<SpriteRenderer>();
        // Saving pre-scene settings
        _initSortingLayerName = _renderer.sortingLayerName;
        _initOrderInLayer = _renderer.sortingOrder;
        
        // Updating for compatibility with scene
        _renderer.sortingLayerName = "Character";
        _renderer.sortingOrder = 0;

        // Find Boss
        if (boss == null)
            boss = Object.FindFirstObjectByType<DevilBoss>();
        
        
        
        
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (moveScene == null) moveScene = FindFirstObjectByType<MoveScene>();
        moveScene.dontUseMoveSceneCamera = true;
        Debug.Log("moveSceneFound!");

        
        // Spawn him in the right place and get him going
        if (SkipRoom1 == false)
        {
            Debug.Log("Spawn in room 1!");
            michael.transform.position = new Vector3(0f, 0f, 1f);
        }
        else
        {  
            // Go straight to boss fight
            torchesRoom1.RoomCleared();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Check if level is cleared
        // Debug.Log(torchesRoom2.torchesCleared);
        if (boss.IsDead && !michael.IsDead && torchesRoom2.torchesCleared && !once)
        {
            once = true;
            Invoke("MoveOn", 3f);
            //Debug.Log("TRIGGERED");
        }

    }

    // wrapper class for continuing with game functionality
    private void MoveOn()
    {
        Debug.Log("Congrats");
        moveScene.dontUseMoveSceneCamera = false;
        // Resetting micehal sprite renderer settings
        _renderer.sortingLayerName = _initSortingLayerName;
        _renderer.sortingOrder = _initOrderInLayer;
        moveScene.StartCoroutine(moveScene.TransitionProcess("(PGR) Procedurally generated rooms"));
    }
}
