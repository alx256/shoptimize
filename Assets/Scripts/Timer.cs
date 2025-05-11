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

    private float remainingTime = 0.0f;

    private void Start()
    {
        remainingTime = minutes * 60 + seconds;
    }

    private void Update()
    {
        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0.0f)
        {
            remainingTime = 0.0f;
            Time.timeScale = 0.0f;
        }

        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);

        timerText.text = string.Format("{0:00}:{1:00} remaining", minutes, seconds);
    }
}
