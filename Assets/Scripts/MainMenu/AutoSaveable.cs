using UnityEngine;
#if UNITY_EDITOR
using UnityEditor; // Chỉ dùng trong Editor để sinh ID
#endif

public class AutoSaveable : MonoBehaviour
{
    [Header("ID Tự Động (Đừng sửa tay)")]
    public string uniqueID;

    private bool isCollected = false; // Biến cờ để phân biệt bị nhặt hay bị hủy do chuyển cảnh

    // 1. Tự động sinh ID ngay khi bạn gắn script này vào vật phẩm
    void Reset()
    {
        GenerateID();
    }

    // Nút bấm thủ công trong menu chuột phải (nếu cần tạo lại ID)
    [ContextMenu("Tạo ID Mới")]
    void GenerateID()
    {
        // Tạo một chuỗi ngẫu nhiên duy nhất (Ví dụ: 8f4b2-9a1c...)
        uniqueID = System.Guid.NewGuid().ToString();
#if UNITY_EDITOR
        // Đánh dấu để Unity biết file đã thay đổi (để lưu lại scene)
        EditorUtility.SetDirty(this);
#endif
    }

    void Start()
    {
        // Nếu ID này đã có trong danh sách đã làm -> Tự hủy ngay lập tức
        if (SaveLoadManager.instance.IsActionCompleted(uniqueID))
        {
            Destroy(gameObject); // Xóa vật phẩm khỏi scene
        }
    }

    // 2. Hàm này gọi khi người chơi nhặt đồ
    public void Collect()
    {
        isCollected = true; // Đánh dấu là người chơi nhặt

        // Lưu vào danh sách
        SaveLoadManager.instance.MarkActionAsCompleted(uniqueID);

        // Xóa vật phẩm
        Destroy(gameObject);
    }

    // Phòng hờ: Nếu dùng hàm Destroy() thường thì không lưu
    // Chỉ lưu khi gọi hàm Collect()
}