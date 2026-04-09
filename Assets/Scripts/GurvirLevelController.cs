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
        
        if (moveScene == null) moveScene = FindFirstObjectByType<MoveScene>();
        moveScene.dontUseMoveSceneCamera = true;

        TorchManager[] managers = Object.FindObjectsByType<TorchManager>(FindObjectsSortMode.None);

        if (torchesRoom1 == null || torchesRoom2 == null)
        {
            foreach (TorchManager manager in managers)
            {
                if (manager.room == 2)
                {
                    torchesRoom2 = manager;
                }
                if (manager.room == 1)
                {
                    torchesRoom1 = manager;
                }
            }
        }



        // Spawn him in the right place and get him going
        if (SkipRoom1 == false)
        {
            michael.transform.position = new Vector3(0f, 0f, 1f);
        }
        else
        {  
            // Go straight to boss fight
            torchesRoom1.RoomCleared();
        }
        
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Check if level is cleared
        Debug.Log(torchesRoom2.torchesCleared);
        if (boss.IsDead && !michael.IsDead && torchesRoom2.torchesCleared)
        {
            //once = true;
            //Invoke("MoveOn", 3f);
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
