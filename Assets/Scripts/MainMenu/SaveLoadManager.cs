using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager instance;
    public bool isLoadingGame = false;
    public int currentSlotToLoad = 0;

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
    public void SaveGame(int slotNumber, GameData data)
    {
        string path = GetSavePath(slotNumber);
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        UnityEngine.Debug.Log("ĐÃ LƯU GAME vào Slot " + slotNumber);
    }

    public GameData LoadGame(int slotNumber)
    {
        string path = GetSavePath(slotNumber);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            GameData data = JsonUtility.FromJson<GameData>(json);
            UnityEngine.Debug.Log("ĐÃ TẢI GAME từ Slot " + slotNumber);
            return data;
        }
        else
        {
            UnityEngine.Debug.LogWarning("Không tìm thấy file lưu ở Slot " + slotNumber);
            return null;
        }
    }

    public bool DoesSaveExist(int slotNumber)
    {
        string path = GetSavePath(slotNumber);
        return File.Exists(path);
    }

    private string GetSavePath(int slotNumber)
    {
        return Path.Combine(Application.persistentDataPath, "save_" + slotNumber + ".json");
    }
    public List<string> currentSessionActions = new List<string>();

    // Hàm gọi khi nhặt đồ/mở cửa... để ghi nhớ
    public void MarkActionAsCompleted(string actionID)
    {
        if (!currentSessionActions.Contains(actionID))
        {
            currentSessionActions.Add(actionID);
            Debug.Log("ZME: Đã ghi nhớ hành động: " + actionID);
        }
    }

    // Hàm kiểm tra xem vật thể này đã bị tác động chưa (để xóa đi khi load)
    public bool IsActionCompleted(string actionID)
    {
        return currentSessionActions.Contains(actionID);
    }
}