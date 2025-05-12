using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Scoreboard : MonoBehaviour
{
    public class ShoppingCart
    {
        private List<Item> items;
        private float totalSize;
        private float totalSavings;
        private int ranking;
        private string agentName;

        public List<Item> Items { get { return items; } }
        public float TotalSize
        {
            get { return totalSize; }
            set { totalSize = value; }
        }
        public float TotalSavings
        {
            get { return totalSavings; }
            set { totalSavings = value; }
        }
        public int Ranking
        {
            get { return ranking; }
            set { ranking = value; }
        }
        public string AgentName
        {
            get { return agentName; }
            set { agentName = value; }
        }

        public ShoppingCart()
        {
            items = new List<Item>();
            totalSize = 0.0f;
            totalSavings = 0.0f;
            ranking = -1;
            agentName = "Unnamed Agent";
        }
    }

    [SerializeField]
    private TextMeshProUGUI scoreboardText;

    private static Scoreboard instance;
    private Dictionary<int, ShoppingCart> shoppingCarts;
    private List<int> rankings;

    public static Scoreboard Instance { get { return instance; } }

    public Dictionary<int, ShoppingCart> ShoppingCarts
    {
        get
        {
            return shoppingCarts;
        }
    }

    public void RegisterId(int id, string name)
    {
        // Insert at last rank by default.
        ShoppingCart cart = new()
        {
            AgentName = name,
            Ranking = rankings.Count
        };
        shoppingCarts.Add(id, cart);
        rankings.Add(id);

        UpdateRankings();
    }

    public bool AddItem(GameObject adder, Item item)
    {
        int id = adder.GetInstanceID();
        float distance = Vector3.Distance(adder.transform.position, item.transform.position);

        if (distance > Parameters.Instance.MaxPickupDistance)
        {
            Debug.LogWarning("Tried to add an item that was too far away (item was " + distance + " away)");
            Debug.Log(item.transform.position);
            Debug.Log(item.name);
            return false;
        }

        if (!shoppingCarts.ContainsKey(id))
        {
            Debug.LogWarning("Tried to add item to unregistered id!");
            return false;
        }

        if (!CanAdd(id, item))
        {
            Debug.LogWarning("Tried to add item that there is no space for! Ignoring.");
            return false;
        }

        ShoppingCart cart = shoppingCarts[id];
        cart.Items.Add(item);
        cart.TotalSize += item.Size;
        cart.TotalSavings += item.SavedValue;

        if (cart.Ranking == -1)
        {
            rankings.Add(id);
            cart.Ranking = rankings.Count - 1;
        }

        UpdateRankings();
        return true;
    }

    public bool RemoveItem(GameObject adder, Item item)
    {
        int id = adder.GetInstanceID();
        float distance = Vector3.Distance(adder.transform.position, item.transform.position);

        if (distance > Parameters.Instance.MaxPickupDistance)
        {
            Debug.LogWarning("Tried to remove an item but was too far away to put it back (item was " + distance + " away)");
            Debug.Log(item.transform.position);
            Debug.Log(item.name);
            return false;
        }

        if (!shoppingCarts.ContainsKey(id))
        {
            Debug.LogWarning("Tried to add item to unregistered id!");
            return false;
        }

        ShoppingCart cart = shoppingCarts[id];

        if (!cart.Items.Remove(item))
        {
            Debug.LogWarning("Tried to remove an item that was not in the cart!");
            return false;
        }

        cart.TotalSize -= item.Size;
        cart.TotalSavings -= item.SavedValue;

        if (cart.Ranking == -1)
        {
            rankings.Add(id);
            cart.Ranking = rankings.Count - 1;
        }

        UpdateRankings();
        return true;
    }

    public bool CanAdd(int id, Item item)
    {
        return shoppingCarts.ContainsKey(id) &&
            shoppingCarts[id].TotalSize + item.Size <= Parameters.Instance.ShoppingCartCapacity;
    }

    private void Awake()
    {
        instance = this;
        shoppingCarts = new();
        rankings = new List<int>();
    }

    private void UpdateRankings()
    {
        string rankingsStr = "";

        foreach (int id in rankings)
        {
            if (!shoppingCarts.ContainsKey(id))
            {
                Debug.LogWarning("Rankings contains reference to ID that is not registered!");
                continue;
            }

            ShoppingCart cart = shoppingCarts[id];

            rankingsStr += string.Format("{0} : ${1:0.00} ({2}% full)\n",
                cart.AgentName,
                cart.TotalSavings,
                Math.Round(100 * (cart.TotalSize / Parameters.Instance.ShoppingCartCapacity), 2));
        }

        scoreboardText.text = rankingsStr;
    }
}
