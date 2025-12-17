using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float lifeTime = 1.0f;

    public void SetText(string value)
    {
        text.text = value;

        Destroy(gameObject, lifeTime);
    }

    
}

