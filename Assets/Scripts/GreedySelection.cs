using System;
using System.Collections.Generic;
using UnityEngine;

public class GreedySelection : SelectionStrategy
{
    private Eyes eyes;

    private void Start()
    {
        eyes = GetComponent<Eyes>();
    }

    private void Update()
    {
        HashSet<GameObject> observedObjects = eyes.ShortSightedScan();
        float maxDiscount = float.NegativeInfinity;
        Item bestValueItem = null;


        foreach (GameObject observedOject in observedObjects)
        {
            if (!observedOject.TryGetComponent<Item>(out var item))
            {
                continue;
            }

            if (item.Discount > maxDiscount)
            {
                maxDiscount = item.Discount;
                bestValueItem = item;
            }
        }

        if (bestValueItem != null &&
            Scoreboard.Instance.CanAdd(gameObject.GetInstanceID(), bestValueItem))
        {
            Scoreboard.Instance.AddItem(gameObject, bestValueItem);
        }
    }
}
