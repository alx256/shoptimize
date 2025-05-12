using System;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [SerializeField]
    private int minutes = 0;
    [SerializeField]
    private int seconds = 30;
    [SerializeField]
    private TMP_Text timerText;

    private float totalTime = 0.0f;
    private float remainingTime = 0.0f;

    private static Timer instance;

    public static Timer Instance
    {
        get { return instance; }
    }

    public float TotalTime
    {
        get { return totalTime; }
    }

    public float RemainingTime
    {
        get { return remainingTime; }
    }

    private void Start()
    {
        totalTime = minutes * 60 + seconds;
        remainingTime = totalTime;
        instance = this;
    }

    private void Update()
    {
        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0.0f)
        {
            remainingTime = 0.0f;
            Time.timeScale = 0.0f;
            Parameters.Instance.IsDone = true;
        }

        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);

        timerText.text = string.Format("{0:00}:{1:00} remaining", minutes, seconds);
    }
}
