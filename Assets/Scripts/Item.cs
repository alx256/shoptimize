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

    public float BaseValue { get { return baseValue; } }
    public float Discount { get { return discount; } }
    public float SavedValue { get { return savedValue; } }
    public float Size { get { return size; } }

    private void Start()
    {
        parameters = Parameters.Instance;

        baseValue = Random.Range(parameters.MinBaseValue,
            parameters.MaxBaseValue);
        discount = Random.Range(parameters.MinDiscount,
            parameters.MaxDiscount);
        savedValue = baseValue - (baseValue * discount);
        size = Random.Range(parameters.MinSize,
            parameters.MaxSize);
    }
}
