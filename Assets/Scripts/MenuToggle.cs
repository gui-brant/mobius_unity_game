using UnityEngine;

public class MenuToggle : MonoBehaviour
{
    public static MenuToggle instance;

    public GameObject menuPanel;
    public GameObject mainMenuPanel;
    public GameObject skillTreePanel;
    public GameObject backButton;
    public GameObject pointsTextObject;

    private bool isOpen = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (skillTreePanel != null) skillTreePanel.SetActive(false);

        if (backButton != null) backButton.SetActive(false);
        if (pointsTextObject != null) pointsTextObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            isOpen = !isOpen;

            if (menuPanel != null)
                menuPanel.SetActive(isOpen);

            if (isOpen)
            {
                if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
                if (skillTreePanel != null) skillTreePanel.SetActive(false);

                if (backButton != null) backButton.SetActive(false);
                if (pointsTextObject != null) pointsTextObject.SetActive(false);

                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
                if (skillTreePanel != null) skillTreePanel.SetActive(false);

                if (backButton != null) backButton.SetActive(false);
                if (pointsTextObject != null) pointsTextObject.SetActive(false);
            }
        }
    }
    
    public void OpenSkillTree()
    {
        Debug.Log("Opening Skill Tree");

        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (skillTreePanel != null) skillTreePanel.SetActive(true);

        if (backButton != null) backButton.SetActive(true);
        if (pointsTextObject != null) pointsTextObject.SetActive(true);
    }

    public void CloseSkillTree()
    {
        Debug.Log("Closing Skill Tree");

        if (skillTreePanel != null) skillTreePanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);

        if (backButton != null) backButton.SetActive(false);
        if (pointsTextObject != null) pointsTextObject.SetActive(false);
    }

    public void ResumeGame()
    {
        isOpen = false;

        if (menuPanel != null)
            menuPanel.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (skillTreePanel != null) skillTreePanel.SetActive(false);

        if (backButton != null) backButton.SetActive(false);
        if (pointsTextObject != null) pointsTextObject.SetActive(false);
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