using UnityEngine;

public class TruthfulnessManager : MonoBehaviour
{
    public static TruthfulnessManager Instance;

    [Range(0, 100)]
    public int truthfulness = 50;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (Instance == null) Instance = this;
    }

    public void AddTruth(int amount)
    {
        truthfulness = Mathf.Clamp(truthfulness + amount, 0, 100);
        PlayerPrefs.SetInt("TruthValue", truthfulness);
    }

    public int GetTruth()
    {
        return truthfulness;
    }
}
