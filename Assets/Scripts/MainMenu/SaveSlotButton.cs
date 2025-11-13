using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SaveSlotButton : MonoBehaviour
{
    [SerializeField] private int slotNumber;

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
            buttonText.text = "Slot " + slotNumber + " (Ghi đè)";
        }
        else
        {
            buttonText.text = "Slot " + slotNumber + " (Lưu mới)";
        }
    }

    public void OnClick()
    {
        GameData data = new GameData();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            data.playerPosition = player.transform.position;
        }
        else
        {
            UnityEngine.Debug.LogError("Không tìm thấy Player! Hãy gán tag 'Player' cho Player của bạn trong Inspector.");
            data.playerPosition = Vector3.zero;
        }

        data.sceneName = SceneManager.GetActiveScene().name;

        SaveLoadManager.instance.SaveGame(slotNumber, data);

        UpdateVisuals();

        UnityEngine.Debug.Log("Đã LƯU vào Slot: " + slotNumber);
    }
}