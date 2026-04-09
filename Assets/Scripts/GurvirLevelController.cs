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
    void Awake()
    {
        // Find Micheal script
        if (michael == null)
            michael = Object.FindFirstObjectByType<Michael>();

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
        moveScene.StartCoroutine(moveScene.TransitionProcess("(PGR) Procedurally generated rooms"));
    }
}
