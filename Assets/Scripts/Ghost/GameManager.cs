using UnityEngine;
public class GameManager : MonoBehaviour
{
    // Cờ (flag) tĩnh mà con ma sẽ kiểm tra
    public static bool ghostIsActive = false;

    // (Ví dụ: Một hàm được gọi khi hoàn thành Cảnh 1)
    public void ActivateGhost()
    {
        ghostIsActive = true;
        Debug.Log("CẢNH 2 BẮT ĐẦU! MA ĐƯỢC KÍCH HOẠT!");
    }
}