using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCasting : MonoBehaviour
{
    public static float DistanceFromTarget;
    public float ToTarget;
    public float maxDistance = 3f;
    private const float aimRadius = 0.15f; 

    void Update()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.TransformDirection(Vector3.forward));

        if (Physics.SphereCast(ray, aimRadius, out hit, maxDistance))
        {
            ToTarget = hit.distance;
            DistanceFromTarget = ToTarget;
        }
        else
        {
            DistanceFromTarget = maxDistance + 1f;
        }
    }
}