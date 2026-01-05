using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class FluorescentLight : MonoBehaviour
{
    [Header("Thành phần ánh sáng")]
    public Light2D light2D;

    [Header("Cường độ sáng")]
    public float minIntensity = 0.8f;
    public float maxIntensity = 2.0f;

    [Header("Tốc độ nhấp nháy")]
    public float flickerSpeedMin = 0.05f;
    public float flickerSpeedMax = 0.2f;

    [Header("Xác suất và thời gian mất điện")]
    [Range(0f, 1f)] public float blackoutChance = 0.06f;
    public float blackoutDuration = 3f;

    [Header("Âm thanh")]
    public AudioClip buzzLoop;  // âm thanh điện rè nhẹ
    public AudioClip zapClip;   // âm thanh “bzzzt” bật tắt mạnh
    private AudioSource audioSource;

    private bool isBlackout = false;
    private bool isBuzzing = false;

    void Start()
    {
        if (!light2D) light2D = GetComponent<Light2D>();
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = false;
        StartCoroutine(FlickerRoutine());
    }

    IEnumerator FlickerRoutine()
    {
        while (true)
        {
            if (!isBlackout)
            {
                // nhấp nháy ngẫu nhiên
                light2D.intensity = Random.Range(minIntensity, maxIntensity);

                // bật tiếng “bzzzt” nhỏ khi thay đổi cường độ
                if (Random.value > 0.8f && zapClip)
                    audioSource.PlayOneShot(zapClip, Random.Range(0.05f, 0.1f));

                // rè nhẹ khi sáng ổn định
                if (!isBuzzing && buzzLoop && light2D.intensity > 1.0f)
                {
                    StartCoroutine(PlayBuzzLoop());
                }

                yield return new WaitForSeconds(Random.Range(flickerSpeedMin, flickerSpeedMax));

                // đôi khi tắt hẳn
                if (Random.value < blackoutChance)
                    StartCoroutine(Blackout());
            }
            else
                yield return null;
        }
    }

    IEnumerator Blackout()
    {
        isBlackout = true;
        float current = light2D.intensity;

        // giảm sáng dần
        for (float i = current; i > 0; i -= Time.deltaTime * 2)
        {
            light2D.intensity = i;
            yield return null;
        }

        light2D.intensity = 0;
        StopBuzz();

        yield return new WaitForSeconds(blackoutDuration);

        // bật lại – nhấp nháy mạnh 3 lần
        for (int i = 0; i < 3; i++)
        {
            light2D.intensity = Random.Range(minIntensity, maxIntensity);
            PlayZapStrong();
            yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));

            light2D.intensity = 0;
            yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
        }

        light2D.intensity = maxIntensity;
        PlayZapStrong();
        StartCoroutine(PlayBuzzLoop());
        isBlackout = false;
    }

    IEnumerator PlayBuzzLoop()
    {
        isBuzzing = true;
        while (!isBlackout)
        {
            audioSource.PlayOneShot(buzzLoop, 0.15f);
            yield return new WaitForSeconds(buzzLoop.length * 0.9f);
        }
        isBuzzing = false;
    }

    void StopBuzz() => audioSource.Stop();

    void PlayZapStrong()
    {
        if (zapClip)
            audioSource.PlayOneShot(zapClip, 0.3f);
    }
}
