using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    public float moveUpSpeed = 1f;
    public float fadeSpeed = 1f;
    private TextMeshProUGUI text;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetText(string value)
    {
        text.text = value;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Vector3.up * moveUpSpeed * Time.deltaTime;

        Color c = text.color;
        c.a -= fadeSpeed * Time.deltaTime;
        text.color = c;

        if (c.a <= 0)
            Destroy(gameObject);
    }
}
