using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject loadGamePanel;
    [SerializeField] private string sceneToLoad = "SampleScene";

    public void OnNewGameButton()
    {
        SaveLoadManager.instance.isLoadingGame = false;
        SaveLoadManager.instance.currentSlotToLoad = 0;
        SceneManager.LoadSceneAsync(sceneToLoad);
    }

    public void OnContinueButton()
    {
        if (loadGamePanel != null)
        {
            loadGamePanel.SetActive(true);
        }
    }

    public void OnSettingsButton()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    public void OnSettingsBackButton()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void OnLoadGameBackButton()
    {
        if (loadGamePanel != null)
        {
            loadGamePanel.SetActive(false);
        }
    }

    public void OnExitButton()
    {
        UnityEngine.Debug.Log("Bạn đã bấm Thoát!");
        UnityEngine.Application.Quit();
    }
}