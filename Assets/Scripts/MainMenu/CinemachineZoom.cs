using UnityEngine;
using Cinemachine; // Rất quan trọng, phải thêm dòng này!

public class CinemachineZoom : MonoBehaviour
{
    // Biến công khai (Kéo camera vào đây)
    public CinemachineFreeLook freeLookCamera;

    [Header("Thông số Zoom")]
    public float zoomSpeed = 5f;    // Tốc độ zoom
    public float minRadius = 2f;    // Khoảng cách gần nhất
    public float maxRadius = 10f;   // Khoảng cách xa nhất

    private CinemachineFreeLook.Orbit[] originalOrbits; // Mảng để lưu cài đặt gốc

    void Start()
    {
        // Lưu lại cài đặt gốc của 3 vòng quỹ đạo (Trên, Giữa, Dưới)
        originalOrbits = new CinemachineFreeLook.Orbit[3];
        for (int i = 0; i < 3; i++)
        {
            originalOrbits[i].m_Height = freeLookCamera.m_Orbits[i].m_Height;
            originalOrbits[i].m_Radius = freeLookCamera.m_Orbits[i].m_Radius;
        }
    }

    void Update()
    {
        // Lấy input từ con lăn chuột (scroll wheel)
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        // Nếu không có input, không làm gì cả
        if (scrollInput == 0f)
            return;

        // "Vòng quỹ đạo" (Orbits) là thứ điều khiển khoảng cách của FreeLook Cam
        // Chúng ta sẽ thay đổi "Bán kính" (Radius) của cả 3 vòng cùng lúc
        for (int i = 0; i < 3; i++)
        {
            // Thay đổi bán kính dựa trên input (dấu trừ để cuộn lên là zoom vào)
            freeLookCamera.m_Orbits[i].m_Radius -= scrollInput * zoomSpeed;

            // Giới hạn bán kính trong "khoản cách nhất định"
            freeLookCamera.m_Orbits[i].m_Radius = Mathf.Clamp(freeLookCamera.m_Orbits[i].m_Radius, minRadius, maxRadius);
        }
    }
}