using UnityEngine;
using UnityEngine.UI; // Textを使う場合

public class StageManager : MonoBehaviour
{
    // インスペクターで、各ステージの「クリア」テキストをアタッチします
    public GameObject[] clearTexts;

    void Start()
    {
        // ゲーム開始時にクリア状況をチェックして表示を更新
        UpdateClearDisplay();
    }

    public void UpdateClearDisplay()
    {
        for (int i = 0; i < clearTexts.Length; i++)
        {
            // PlayerPrefsなどで保存されたクリアフラグを確認（例: Stage1Clear）
            if (PlayerPrefs.GetInt("Stage" + (i + 1) + "Clear", 0) == 1)
            {
                clearTexts[i].SetActive(true); // クリアしていたら表示
            }
            else
            {
                clearTexts[i].SetActive(false); // 未クリアなら非表示
            }
        }
    }

    // ステージクリア時にこの関数を呼ぶ
    public void MarkStageAsCleared(int stageNumber)
    {
        PlayerPrefs.SetInt("Stage" + stageNumber + "Clear", 1);
        PlayerPrefs.Save();
        UpdateClearDisplay();
    }
}