using UnityEngine;

public class RandomExploration : MovementStrategy
{
    private Agent agent;
    private Eyes eyes;

    private const int SPEED_DIVISOR = 50;

    private void Start()
    {
        agent = GetComponent<Agent>();
        eyes = GetComponent<Eyes>();
        RotateRandomly();
    }

    private void Update()
    {
        agent.Move(transform.forward * Time.deltaTime);

        if (eyes.ShortSightedLook().Count != 0)
        {
            // Something directly in front of us
            RotateRandomly();
        }
    }

    private void RotateRandomly()
    {
        transform.Rotate(0.0f,
                         Random.Range(0.0f, 360.0f),
                         0.0f);
    }
}
