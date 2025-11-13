using UnityEngine;

public class PlayerInteract_Advanced : MonoBehaviour
{
    // Biến này lưu "Quản lý" mà Player đang đứng gần
    private DoorTriggerManager currentManager = null;

    void Update()
    {
        if (currentManager != null && Input.GetKeyDown(KeyCode.E))
        {
            // Ra lệnh cho "Quản lý"
            currentManager.ToggleAllDoors();
        }
    }

    // Tìm Quản lý khi đi vào
    private void OnTriggerEnter(Collider other)
    {
        DoorTriggerManager manager = other.GetComponent<DoorTriggerManager>();
        if (manager != null)
        {
            currentManager = manager;
        }
    }

    // "Quên" Quản lý khi đi ra
    private void OnTriggerExit(Collider other)
    {
        DoorTriggerManager manager = other.GetComponent<DoorTriggerManager>();
        if (manager == currentManager)
        {
            currentManager = null;
        }
    }
}