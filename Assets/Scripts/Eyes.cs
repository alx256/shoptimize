using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Eyes : MonoBehaviour
{
    private const float SHORT_SIGHTED_MAX_DISTANCE = 1.5f;
    private const float LONG_SIGHTED_MAX_DISTANCE = 100.0f;
    private const float JUST_IN_FRONT_DISTANCE = 1.0f;
    private const float LOOK_ANGLE_ADJUST = 0.33f;
    private readonly HashSet<string> CAPTURE_PARENT_LAYERS = new(new string[]{
        "Shelf"
    });

    public HashSet<GameObject> ShortSightedScan()
    {
        return ShortSightedLook(lookAngleStart: -45.0f,
                                lookAngleEnd: 45.0f,
                                mask: 1 << LayerMask.NameToLayer("Item"));
    }

    public HashSet<GameObject> ShortSightedLook(float lookAngleStart = 0.0f,
                                                float lookAngleEnd = 0.0f,
                                                int mask = int.MaxValue)
    {
        return Look(SHORT_SIGHTED_MAX_DISTANCE, lookAngleStart, lookAngleEnd, mask);
    }

    public HashSet<GameObject> LongSightedLook(float lookAngleStart = 0.0f,
                                               float lookAngleEnd = 0.0f,
                                               int mask = int.MaxValue)
    {
        return Look(LONG_SIGHTED_MAX_DISTANCE, lookAngleStart, lookAngleEnd, mask);
    }

    public HashSet<GameObject> Look(float distance,
                                    float lookAngleStart,
                                    float lookAngleEnd,
                                    int mask)
    {
        HashSet<GameObject> set = new();
        float lookAngle = lookAngleStart;
        // Unity (for some reason) says you can't have two
        // BoxColliders with different shapes, so to create
        // a more complicated collider shape (such as an L
        // shape), we need multiple child objects with the
        // colliders. In these cases, we must save the parent
        // to the set, not the collided object.
        //
        // Right now, we only do this with shelves.
        bool captureParent = false;

        foreach (string layer in CAPTURE_PARENT_LAYERS)
        {
            if (((mask >> LayerMask.NameToLayer(layer)) & 1) == 1)
            {
                captureParent = true;
                break;
            }
        }

        while (lookAngle <= lookAngleEnd)
        {
            Vector3 direction = Quaternion.AngleAxis(lookAngle, Vector3.forward) * transform.forward;

            Debug.DrawRay(transform.position,
                direction * distance,
                Color.red);
            if (Physics.Raycast(transform.position,
                                direction,
                                out RaycastHit hit,
                                distance,
                                mask))
            {
                set.Add((captureParent && CAPTURE_PARENT_LAYERS.Contains(LayerMask.LayerToName(hit.transform.gameObject.layer)))
                    ? hit.collider.transform.parent.gameObject :
                    hit.collider.gameObject);
            }

            lookAngle += LOOK_ANGLE_ADJUST;
        }

        return set;
    }

    public GameObject PeekDirectlyInFront(int mask)
    {
        HashSet<GameObject> jif = Look(JUST_IN_FRONT_DISTANCE, 0.0f, 0.0f, mask);
        return (jif.Count > 0) ? jif.First() : null;
    }
}
