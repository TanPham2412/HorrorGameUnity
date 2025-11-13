using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    void Start()
    {
        if (SaveLoadManager.instance.isLoadingGame == true)
        {
            int slot = SaveLoadManager.instance.currentSlotToLoad;

            GameData data = SaveLoadManager.instance.LoadGame(slot);

            if (data != null)
            {
                this.gameObject.transform.position = data.playerPosition;

                UnityEngine.Debug.Log("ĐÃ TẢI VỊ TRÍ PLAYER TỪ SLOT " + slot);
            }
            SaveLoadManager.instance.isLoadingGame = false;
        }
        else
        {
            UnityEngine.Debug.Log("Bắt đầu Game Mới, dùng vị trí mặc định.");
        }
    }
}