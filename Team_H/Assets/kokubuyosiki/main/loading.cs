using UnityEngine;
using TMPro;

public class LoadingTextBlink : MonoBehaviour
{
    public TextMeshProUGUI text;
    float t = 0;

    void Update()
    {
        t += Time.deltaTime * 3f;
        float a = (Mathf.Sin(t) + 1) / 2f;
        text.alpha = a;
    }
}