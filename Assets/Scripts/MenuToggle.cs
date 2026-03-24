using UnityEngine;

public class MenuToggle : MonoBehaviour
{
    public GameObject menuPanel;
    public GameObject mainMenuPanel;
    public GameObject skillTreePanel;
    public GameObject backButton;
    public GameObject pointsTextObject;

    private bool isOpen = false;

    void Start()
    {
        menuPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        skillTreePanel.SetActive(false);

        backButton.SetActive(false);
        pointsTextObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            isOpen = !isOpen;
            menuPanel.SetActive(isOpen);

            if (isOpen)
            {
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                
                mainMenuPanel.SetActive(true);
                skillTreePanel.SetActive(false);

                backButton.SetActive(false);
                pointsTextObject.SetActive(false);
            }
        }
    }
    
    public void OpenSkillTree()
    {
        Debug.Log("Opening Skill Tree");

        mainMenuPanel.SetActive(false);
        skillTreePanel.SetActive(true);

        backButton.SetActive(true);
        pointsTextObject.SetActive(true); // 🔥 SHOW HERE
    }

    public void CloseSkillTree()
    {
        Debug.Log("Closing Skill Tree");

        skillTreePanel.SetActive(false);
        mainMenuPanel.SetActive(true);

        backButton.SetActive(false);
        pointsTextObject.SetActive(false);
    }

    public void ResumeGame()
    {
        isOpen = false;
        menuPanel.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        mainMenuPanel.SetActive(true);
        skillTreePanel.SetActive(false);

        backButton.SetActive(false);
        pointsTextObject.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game");

        Application.Quit();
        
        //This is for game quiting as it only works when you build the project as opposed to in editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}