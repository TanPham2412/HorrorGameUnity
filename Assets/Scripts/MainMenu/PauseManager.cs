using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject saveGamePanel;

    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused && saveGamePanel.activeInHierarchy)
            {
                CloseSavePanel();
            }
            else
            {
                TogglePause();
            }
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            pausePanel.SetActive(true);
            Time.timeScale = 0f;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            CloseSavePanel();
            pausePanel.SetActive(false);
            Time.timeScale = 1f;

            Cursor.lockState = CursorLockMode.Locked; 
            Cursor.visible = false;
        }
    }

    public void OnResumeButton()
    {
        TogglePause();
    }

    public void OnSaveGameButton()
    {
        if (saveGamePanel != null)
        {
            saveGamePanel.SetActive(true);
        }
    }

    public void OnSaveGameBackButton()
    {
        CloseSavePanel();
    }

    private void CloseSavePanel()
    {
        if (saveGamePanel != null)
        {
            saveGamePanel.SetActive(false);
        }
    }

    public void OnExitToMenuButton()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None; 
        SceneManager.LoadSceneAsync("MainMenu_Scene");
    }
}