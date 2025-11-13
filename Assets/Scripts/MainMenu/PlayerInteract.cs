using UnityEngine; // <--- DÒNG NÀY RẤT QUAN TRỌNG

// Code của bạn bắt đầu ở đây
public class PlayerInteract : MonoBehaviour
{
    private DoorInteraction currentDoor = null;

    void Update()
    {
        if (currentDoor != null && Input.GetKeyDown(KeyCode.E))
        {
            currentDoor.ToggleDoor();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        DoorInteraction door = other.GetComponentInParent<DoorInteraction>();
        if (door != null)
        {
            currentDoor = door;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        DoorInteraction door = other.GetComponentInParent<DoorInteraction>();
        if (door == currentDoor)
        {
            currentDoor = null;
        }
    }
}