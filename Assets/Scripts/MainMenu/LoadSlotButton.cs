using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadSlotButton : MonoBehaviour
{
    public int slotNumber;

    private Button button;
    private TextMeshProUGUI buttonText;

    void Awake()
    {
        button = GetComponent<Button>();
        buttonText = GetComponentInChildren<TextMeshProUGUI>();

        button.onClick.AddListener(OnClick);
    }

    void OnEnable()
    {
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        if (SaveLoadManager.instance.DoesSaveExist(slotNumber))
        {
            buttonText.text = "Slot " + slotNumber + ": Đã có dữ liệu";
        }
        else
        {
            buttonText.text = "Slot " + slotNumber + ": Trống (Tạo mới)";
        }
    }

    public void OnClick()
    {
        GameData data = SaveLoadManager.instance.LoadGame(slotNumber);

        if (data == null)
        {
            data = new GameData();
            UnityEngine.Debug.Log("Tạo game MỚI cho Slot " + slotNumber);
        }
        SaveLoadManager.instance.isLoadingGame = true;
        SaveLoadManager.instance.currentSlotToLoad = slotNumber;
        SceneManager.LoadSceneAsync(data.sceneName);
    }
}