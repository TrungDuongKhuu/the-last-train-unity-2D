using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeTester : MonoBehaviour
{
    public RawImage fadeImg;   // kéo IrisFade_Image vào
    Material inst;

    void Awake()
    {
        // tạo material instance và gán lại
        inst = new Material(fadeImg.material);
        fadeImg.material = inst;
        inst.SetFloat("_Radius", 1f);
        inst.SetVector("_Center", new Vector2(0.5f, 0.5f));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
            StartCoroutine(Fade(1f, 0f, 0.4f));  // thu đen
        if (Input.GetKeyDown(KeyCode.F2))
            StartCoroutine(Fade(0f, 1f, 0.4f));  // bung sáng
    }

    IEnumerator Fade(float from, float to, float dur)
    {
        float t = 0;
        while (t < dur)
        {
            t += Time.deltaTime;
            inst.SetFloat("_Radius", Mathf.Lerp(from, to, t / dur));
            yield return null;
        }
        inst.SetFloat("_Radius", to);
    }
}
