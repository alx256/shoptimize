using System;
using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField]
    private float baseValue;
    [SerializeField]
    private float discount;
    [SerializeField]
    private float savedValue;
    [SerializeField]
    private float size;

    private Parameters parameters;
    private System.Random random;

    public float BaseValue { get { return baseValue; } }
    public float Discount { get { return discount; } }
    public float SavedValue { get { return savedValue; } }
    public float Size { get { return size; } }

    private void Start()
    {
        parameters = Parameters.Instance;

        // Use System.Random instead of Unity's Random to
        // only use the seed for generating item values
        // and sizes
        random = new System.Random(transform.position.GetHashCode());

        if (random == null)
        {
            random = new(parameters.EnvironmentValuesSeed * transform.position.GetHashCode());
        }

        baseValue = (baseValue == -1) ?
            RandomFloat(parameters.MinBaseValue, parameters.MaxBaseValue) :
            baseValue;
        discount = (discount == -1) ?
            RandomFloat(parameters.MinDiscount, parameters.MaxDiscount) :
            discount;
        size = (size == -1) ?
            RandomFloat(parameters.MinSize, parameters.MaxSize) :
            size;
        savedValue = baseValue - (baseValue * discount);
    }

    private float RandomFloat(float start, float end)
    {
        return start + (end - start) * (float)random.NextDouble();
    }
}
