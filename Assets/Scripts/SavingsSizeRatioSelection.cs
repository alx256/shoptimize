using UnityEngine;

public class SavingsSizeRatioSelection : SelectionStrategy
{
    private Eyes eyes;
    private int itemMask;
    private float minSaving;
    private float maxSaving;
    private GameObject lastViewedItem;

    private const float ACCEPTANCE_THRESHOLD = 1.7f;

    private void Start()
    {
        eyes = GetComponent<Eyes>();
        itemMask = 1 << LayerMask.NameToLayer("Item");
        minSaving = Parameters.Instance.MinBaseValue * Parameters.Instance.MinDiscount;
        maxSaving = Parameters.Instance.MaxBaseValue * Parameters.Instance.MaxDiscount;
    }

    private void Update()
    {
        GameObject peakedItem = eyes.PeekDirectlyInFront(mask: itemMask);

        if (peakedItem == null)
        {
            return;
        }

        if (lastViewedItem != null && lastViewedItem == peakedItem)
        {
            return;
        }

        Item item = peakedItem.GetComponent<Item>();
        float normalizedSaving = (item.SavedValue - minSaving) /
            (maxSaving - minSaving);
        float normalizedSize = (item.Size - Parameters.Instance.MinSize) /
            (Parameters.Instance.MaxSize - Parameters.Instance.MinSize);
        float ratio = normalizedSaving / normalizedSize;

        if (ratio > ACCEPTANCE_THRESHOLD)
        {
            if (Scoreboard.Instance.CanAdd(gameObject.GetInstanceID(), item))
            {
                Scoreboard.Instance.AddItem(gameObject.GetInstanceID(), item);
            }
        }

        lastViewedItem = peakedItem;
    }
}
