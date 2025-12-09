using UnityEngine;

public class GameSceneLoader : MonoBehaviour
{
    [Header("Tham chiếu")]
    public GameObject player; // Nhớ kéo nhân vật Player vào ô này ở Inspector

    void Awake()
    {
        // 1. Kiểm tra SaveLoadManager có tồn tại không
        if (SaveLoadManager.instance != null)
        {
            // 2. Kiểm tra cờ hiệu: Có phải đang Load game không?
            if (SaveLoadManager.instance.isLoadingGame)
            {
                int slotIndex = SaveLoadManager.instance.currentSlotToLoad;
                LoadDataAndApply(slotIndex);

                // 3. Quan trọng: Tắt cờ sau khi load xong
                SaveLoadManager.instance.isLoadingGame = false;
            }
        }
    }

    void LoadDataAndApply(int slotIndex)
    {
        GameData data = SaveLoadManager.instance.LoadGame(slotIndex);

        if (data != null)
        {
            SaveLoadManager.instance.currentSessionActions = data.completedActions;

            Vector3 loadPos = data.playerPosition;

            CharacterController cc = player.GetComponent<CharacterController>();

            if (cc != null) cc.enabled = false; // Tắt tạm thời

            player.transform.position = loadPos; // Gán vị trí

            if (cc != null) cc.enabled = true;  // Bật lại ngay

            Debug.Log("ZME: Đã load Player tới vị trí: " + loadPos);
        }
        else
        {
            Debug.LogError("ZME: Không đọc được dữ liệu (Data Null)!");
        }
    }
}