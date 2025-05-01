using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Eyes : MonoBehaviour
{
    private const float MAX_DISTANCE = 4.0f;
    private const float SHORT_SIGHTED_MAX_DISTANCE = 3.0f;
    private const float LOOK_ANGLE_ADJUST = 0.33f;

    public HashSet<GameObject> ShortSightedScan()
    {
        return ShortSightedLook(lookAngleStart: -45.0f,
                                lookAngleEnd: 45.0f,
                                mask: 1 << LayerMask.NameToLayer("Item"));
    }

    public HashSet<GameObject> ShortSightedLook(float lookAngleStart = 0.0f, float lookAngleEnd = 0.0f, int mask = int.MaxValue)
    {
        HashSet<GameObject> set = new();
        float lookAngle = lookAngleStart;

        while (lookAngle <= lookAngleEnd)
        {
            Vector3 direction = Quaternion.AngleAxis(lookAngle, Vector3.forward) * transform.forward;

            Debug.DrawRay(transform.position,
                direction * SHORT_SIGHTED_MAX_DISTANCE,
                Color.red);
            if (Physics.Raycast(transform.position,
                                direction,
                                out RaycastHit hit,
                                SHORT_SIGHTED_MAX_DISTANCE,
                                mask))
            {
                set.Add(hit.collider.gameObject);
            }

            lookAngle += LOOK_ANGLE_ADJUST;
        }

        return set;
    }

    private RaycastHit Look()
    {
        Physics.Raycast(transform.position,
            transform.forward,
            out RaycastHit hit,
            MAX_DISTANCE);

        return hit;
    }
}
