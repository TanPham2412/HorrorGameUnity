using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalInventory : MonoBehaviour
{
    public static bool hasKey = false;
    public static bool hasGuardKey = false;
    public static bool hasVHSTape = false;
    public static bool hasFlashlight = false;

    // Chỉ cho nhặt flashlight sau khi xem xong băng cassette
    public static bool canPickupFlashlight = false;
}