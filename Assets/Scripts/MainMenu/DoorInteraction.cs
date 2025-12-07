using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class DoorInteraction : MonoBehaviour
{
    // --- Cài đặt trong Inspector ---
    [Header("Cài đặt Cửa")]
    public float openAngle = 90.0f; // Góc sẽ mở (ví dụ: 90 độ)
    public float speed = 5.0f;      // Tốc độ cửa mở

    // --- Biến nội bộ ---
    private Quaternion closedRotation; // Vị trí đóng (ban đầu)
    private Quaternion openRotation;   // Vị trí khi mở
    private bool isOpen = false;       // Trạng thái hiện tại của cửa
    private Coroutine currentCoroutine = null; // Để theo dõi coroutine đang chạy

    void Start()
    {
        // Lưu lại vị trí đóng ban đầu
        closedRotation = transform.rotation;

        // Tính toán vị trí khi mở
        // (Xoay 90 độ quanh trục Y so với vị trí đóng)
        openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);
    }
    public void OpenDoorForAI()
    {
        if (isOpen == false) // Chỉ mở nếu cửa đang đóng
        {
            ToggleDoor(); // Gọi hàm mở/đóng
        }
    }
    public void ToggleDoor()
    {
        // Nếu đang có coroutine (cửa đang chạy), dừng nó lại
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        // Bắt đầu một coroutine mới để Mở hoặc Đóng
        currentCoroutine = StartCoroutine(AnimateDoor());
    }

    // Đây là Coroutine để "diễn hoạt" cửa
    private IEnumerator AnimateDoor()
    {
        // 1. Quyết định mục tiêu:
        // Nếu cửa đang đóng (isOpen = false) -> mục tiêu là Mở
        // Nếu cửa đang mở (isOpen = true) -> mục tiêu là Đóng
        isOpen = !isOpen; // Đảo ngược trạng thái
        Quaternion targetRotation = isOpen ? openRotation : closedRotation;

        // 2. Vòng lặp xoay:
        // Xoay cho đến khi nào cửa gần đến vị trí mục tiêu
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            // Dùng Lerp để xoay mượt mà
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, speed * Time.deltaTime);
            yield return null; // Chờ đến frame tiếp theo
        }

        // 3. Hoàn tất:
        // Đặt vị trí về chính xác mục tiêu (để tránh sai số nhỏ)
        transform.rotation = targetRotation;
        currentCoroutine = null; // Báo là đã chạy xong
    }
}
