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
    [SerializeField]
    private float timeScale = 1.0f;
    [SerializeField]
    private int environmentValuesSeed = 123;
    [SerializeField]
    private float maxPickupDistance = 2.0f;
    private bool isDone = false;

    public static Parameters Instance { get { return instance; } }
    public float MinBaseValue { get { return minBaseValue; } }
    public float MaxBaseValue { get { return maxBaseValue; } }
    public float MinDiscount { get { return minDiscount; } }
    public float MaxDiscount { get { return maxDiscount; } }
    public float MinSize { get { return minSize; } }
    public float MaxSize { get { return maxSize; } }
    public float ShoppingCartCapacity { get { return shoppingCartCapacity; } }
    public int EnvironmentValuesSeed { get { return environmentValuesSeed; } }
    public float MaxPickupDistance { get { return maxPickupDistance; } }
    public bool IsDone { get { return isDone; } set { isDone = value; } }

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        // If the timeScale has been set to 0 by another script,
        // this means an experiment has finished and we should
        // not interfere with this.
        if (!isDone)
        {
            Time.timeScale = timeScale;
        }
    }
}
