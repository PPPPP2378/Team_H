using UnityEngine;
using UnityEngine.UI;


public class StageSelectButton : MonoBehaviour
{
    public string stageKey;      // "Stage1Clear"
    public Image buttonImage;
    public Sprite clearedSprite; // クリア後のボタン画像

    void Start()
    {
        if (PlayerPrefs.GetInt(stageKey, 0) == 1)
        {
            buttonImage.sprite = clearedSprite;
        }
    }
}
