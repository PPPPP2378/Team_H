using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    public TextMeshProUGUI text;

    public void SetText(string value)
    {
        text.text = value;
    }

    public void DestroySelf()
    {
        Destroy(gameObject, 1f);
    }
}

