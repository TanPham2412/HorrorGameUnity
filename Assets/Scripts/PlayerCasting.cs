using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCasting : MonoBehaviour
{
    public static float DistanceFromTarget;
    public float ToTarget;
    public float maxDistance = 3f;
    void Update()
    {
        RaycastHit Hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out Hit, maxDistance))
        {
            ToTarget = Hit.distance;
            DistanceFromTarget = ToTarget;
        }
        else
        {
            DistanceFromTarget = maxDistance + 1f;
        }
    }
}