using UnityEngine;
using TMPro;

//敵がダメージを受けたらテキストを表示
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

