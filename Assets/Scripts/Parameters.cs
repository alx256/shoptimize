using UnityEngine;

public class Parameters : MonoBehaviour
{
    private static Parameters instance;

    [SerializeField]
    private float minBaseValue;
    [SerializeField]
    private float maxBaseValue;
    [SerializeField]
    private float minDiscount;
    [SerializeField]
    private float maxDiscount;
    [SerializeField]
    private float minSize;
    [SerializeField]
    private float maxSize;
    [SerializeField]
    private float shoppingCartCapacity;

    public static Parameters Instance { get { return instance; } }
    public float MinBaseValue { get { return minBaseValue; } }
    public float MaxBaseValue { get { return maxBaseValue; } }
    public float MinDiscount { get { return minDiscount; } }
    public float MaxDiscount { get { return maxDiscount; } }
    public float MinSize { get { return minSize; } }
    public float MaxSize { get { return maxSize; } }
    public float ShoppingCartCapacity { get { return shoppingCartCapacity; } }

    private void Awake()
    {
        instance = this;
    }
}
