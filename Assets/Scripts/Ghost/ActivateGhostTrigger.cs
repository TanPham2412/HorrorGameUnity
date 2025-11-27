using UnityEngine;

public class ActivateGhostTrigger : MonoBehaviour
{
    // Cờ (flag) để đảm bảo nó chỉ chạy 1 lần
    private bool hasBeenTriggered = false;

    // Hàm này tự động chạy khi có vật gì đó đi vào
    private void OnTriggerEnter(Collider other)
    {
        // Kiểm tra xem có phải Player đi vào không
        // (Và nó chưa được kích hoạt)
        if (other.CompareTag("Player") && !hasBeenTriggered)
        {
            hasBeenTriggered = true; // Đánh dấu là đã kích hoạt

            // Kích hoạt con Ma!
            GameManager.ghostIsActive = true;

            // In ra Console để báo
            Debug.Log("!!! GHOST ĐÃ ĐƯỢC KÍCH HOẠT !!!");

            // (Tùy chọn) Xóa vật phẩm này đi
            Destroy(gameObject);
        }
    }
}