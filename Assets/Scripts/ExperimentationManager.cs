using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExperimentationManager : MonoBehaviour
{
    [SerializeField]
    private int experimentCount = 5;

    private int remainingExperiments;
    private float lastRemainingTime = float.PositiveInfinity;
    private bool hasWrittenJson = false;
    private int stepCount = 0;
    private int experimentNumber = 0;

    private class AgentResults
    {
        public string name;
        public float[] results;
        public float[] maxOverTime;
        public float[] minOverTime;

        [NonSerialized]
        public float mean;
        [NonSerialized]
        public float[] meanValues;
    }

    private readonly Dictionary<string, AgentResults> results = new();

    private void Start()
    {
        remainingExperiments = experimentCount;

        if (SceneManager.GetActiveScene().name == "ExperimentStartScene")
        {
            DontDestroyOnLoad(gameObject);
            SceneManager.LoadScene("MainScene");
            remainingExperiments--;
        }
    }

    private void LateUpdate()
    {
        // A second or more has passed
        if (Timer.Instance != null && lastRemainingTime - Timer.Instance.RemainingTime >= 60.0f)
        {
            foreach (Scoreboard.ShoppingCart cart in Scoreboard.Instance.ShoppingCarts.Values)
            {
                if (!results.ContainsKey(cart.AgentName))
                {
                    int totalSeconds = Mathf.CeilToInt(Timer.Instance.TotalTime);

                    results[cart.AgentName] = new()
                    {
                        maxOverTime = Enumerable.Repeat(-1.0f, totalSeconds).ToArray(),
                        minOverTime = Enumerable.Repeat(-1.0f, totalSeconds).ToArray()
                    };
                }
            }

            lastRemainingTime = Timer.Instance.RemainingTime;
        }

        IterationRecord();

        if (Timer.Instance != null && Timer.Instance.RemainingTime <= 0.0f)
        {
            if (remainingExperiments != 0)
            {
                EndOfExperiment();
                remainingExperiments--;
                Parameters.Instance.IsDone = false;
                SceneManager.LoadScene("MainScene");
            }
            else if (!hasWrittenJson)
            {
                hasWrittenJson = true;
                EndOfExperiment();
                EndOfAllExperiments();

                string json = "{\"results\": [";
                int i = 0;

                foreach (AgentResults ar in results.Values)
                {
                    json += JsonUtility.ToJson(ar);
                    if (i++ < results.Count - 1)
                    {
                        json += ",";
                    }
                }

                json += "]}";

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string savePath = Path.Combine(Application.persistentDataPath, "ShoptimiseExperimentRuns");
                string filePath = Path.Combine(savePath, timestamp + ".json");

                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }

                File.WriteAllText(filePath, json);
                Debug.Log("Results saved to " + filePath);
            }
        }
    }

    private void EndOfExperiment()
    {
        foreach (AgentResults result in results.Values)
        {
            result.mean /= stepCount;

            if (result.meanValues == null)
            {
                result.meanValues = new float[experimentCount];
            }

            result.meanValues[experimentNumber] = result.mean;
            result.mean = 0;
        }

        experimentNumber++;
        stepCount = 0;
    }

    private void EndOfAllExperiments()
    {
        string[] keys = results.Keys.ToArray();

        foreach (string name in keys)
        {
            results[name] = new()
            {
                name = name,
                results = results[name].meanValues,
                maxOverTime = results[name].maxOverTime,
                minOverTime = results[name].minOverTime
            };
        }
    }

    private void IterationRecord()
    {
        stepCount++;

        if (Scoreboard.Instance == null)
        {
            return;
        }

        foreach (Scoreboard.ShoppingCart cart in Scoreboard.Instance.ShoppingCarts.Values)
        {
            if (!results.ContainsKey(cart.AgentName))
            {
                results[cart.AgentName] = new();
            }

            // Add the results for this run
            results[cart.AgentName].mean += cart.TotalSavings;

            int second = Mathf.FloorToInt(Timer.Instance.TotalTime - Timer.Instance.RemainingTime);
            float[] maxOverTime = results[cart.AgentName].maxOverTime;
            float[] minOverTime = results[cart.AgentName].minOverTime;

            if (second < maxOverTime.Count())
            {
                maxOverTime[second] = (maxOverTime[second] == -1.0f) ?
                    cart.TotalSavings :
                    Mathf.Max(maxOverTime[second], cart.TotalSavings);
                minOverTime[second] = (minOverTime[second] == -1.0f) ?
                    cart.TotalSavings :
                    Mathf.Min(minOverTime[second], cart.TotalSavings);
            }
        }
    }
}
