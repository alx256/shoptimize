using UnityEngine;
using UnityEngine.SceneManagement;

public class ExperimentationManager : MonoBehaviour
{
    [SerializeField]
    private int experimentCount = 5;

    private int remainingExperiments;

    private class AgentResults
    {
        public float Mean { get; set; }
        public float Max { get; set; }
        public float Min { get; set; }
        public float[] AverageResultOverTime { get; set; }
    }

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
        if (Timer.Instance != null && Timer.Instance.RemainingTime <= 0.0f && remainingExperiments != 0)
        {
            remainingExperiments--;
            Parameters.Instance.IsDone = false;
            SceneManager.LoadScene("MainScene");
        }


    }
}
