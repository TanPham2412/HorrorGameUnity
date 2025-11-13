using UnityEngine;
using Cinemachine; // Rất quan trọng, phải thêm dòng này!

// Yêu cầu component này phải được gắn trên cùng một GameObject
// với CinemachineFreeLook
[RequireComponent(typeof(CinemachineFreeLook))]
public class HoldToRotate : MonoBehaviour
{
    // Chúng ta sẽ kéo Camera vào đây
    private CinemachineFreeLook freeLookCamera;

    // Bạn có thể đổi '1' thành '0' (chuột trái) hoặc '2' (chuột giữa)
    public int mouseButtonToHold = 1; // 1 = Chuột phải

    void Start()
    {
        // Tự động tìm component FreeLook Camera trên chính GameObject này
        freeLookCamera = GetComponent<CinemachineFreeLook>();

        // Tắt input tự động (để chắc chắn)
        freeLookCamera.m_XAxis.m_InputAxisName = "";
        freeLookCamera.m_YAxis.m_InputAxisName = "";
    }

    void Update()
    {
        // Kiểm tra xem người chơi có đang "giữ" chuột phải không
        if (Input.GetMouseButton(mouseButtonToHold))
        {
            // Nếu có, "bơm" giá trị di chuyển của chuột vào các trục của Cinemachine
            freeLookCamera.m_XAxis.m_InputAxisValue = Input.GetAxis("Mouse X");
            freeLookCamera.m_YAxis.m_InputAxisValue = Input.GetAxis("Mouse Y");
        }
        else
        {
            // Nếu không giữ, "bơm" giá trị 0 (không di chuyển)
            freeLookCamera.m_XAxis.m_InputAxisValue = 0;
            freeLookCamera.m_YAxis.m_InputAxisValue = 0;
        }
    }
}