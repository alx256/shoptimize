using UnityEngine;

public class Agent : MonoBehaviour
{
    public string agentName = "Unnamed Agent";

    private void Start()
    {
        Scoreboard.Instance.RegisterId(gameObject.GetInstanceID(), agentName);
    }
}
