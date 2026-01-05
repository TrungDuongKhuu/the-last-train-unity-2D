using UnityEngine;

public class TrainShake : MonoBehaviour
{
    [Header("Tùy chỉnh độ rung")]
    public float amplitude = 0.05f;   // Độ cao dao động (0.02–0.08 là vừa)
    public float frequency = 2f;      // Tốc độ dao động (1.5–3 cho tự nhiên)

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        // Dao động theo hình sin: lên - xuống mềm
        float offsetY = Mathf.Sin(Time.time * frequency) * amplitude;
        transform.localPosition = startPos + new Vector3(0f, offsetY, 0f);
    }
}
