using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio; // Cần cho AudioMixer

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject loadGamePanel;
    [SerializeField] private string sceneToLoad = "SampleScene";
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private Slider volumeSlider;

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
    
    public void SetMasterVolume(float volume)
    {
        float dbValue = Mathf.Log10(volume) * 20;

        mainMixer.SetFloat("MasterVolume", dbValue);
    }
    void Start()
    {
        if (volumeSlider != null)
        {
            SetMasterVolume(volumeSlider.value);
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

    public void OnLoadSlotButton(int slotIndex)
    {
        if (SaveLoadManager.instance == null)
        {
            Debug.LogError("Lỗi: Không tìm thấy SaveLoadManager!");
            return;
        }

        SaveLoadManager.instance.isLoadingGame = true;

        SaveLoadManager.instance.currentSlotToLoad = slotIndex;

        Debug.Log($"Đang load Slot {slotIndex}, chuyển sang scene: {sceneToLoad}");
        SceneManager.LoadSceneAsync(sceneToLoad);
    }
}