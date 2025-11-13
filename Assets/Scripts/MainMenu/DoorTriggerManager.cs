using System.Collections.Generic;
using UnityEngine;

public class DoorTriggerManager : MonoBehaviour
{
    // Danh sách chứa TẤT CẢ các cánh cửa mà vùng Trigger này điều khiển
    [SerializeField]
    private List<DoorInteraction> doorsToToggle = new List<DoorInteraction>();

    // Một hàm public để Player có thể gọi
    public void ToggleAllDoors()
    {
        // Lặp qua từng cái cửa trong danh sách
        foreach (DoorInteraction door in doorsToToggle)
        {
            if (door != null)
            {
                // ... và ra lệnh cho nó tự mở/đóng
                door.ToggleDoor();
            }
        }
    }
}