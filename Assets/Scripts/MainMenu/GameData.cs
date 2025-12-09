using UnityEngine;
using System.Collections.Generic; // Cần thư viện này để dùng List

[System.Serializable]
public class GameData
{
    public Vector3 playerPosition;
    public string sceneName;

    // --- THÊM MỚI: Danh sách các hành động đã làm ---
    // Ví dụ chứa: ["Lay_ChiaKhoa_1", "Mo_Cua_Chinh", "Giet_Ghost_Boss"]
    public List<string> completedActions;
    // ------------------------------------------------

    public GameData()
    {
        completedActions = new List<string>(); // Khởi tạo danh sách trống
    }
}