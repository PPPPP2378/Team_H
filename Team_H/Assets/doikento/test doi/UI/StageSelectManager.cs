using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StageSelectManager : MonoBehaviour
{
    [System.Serializable]
    public struct StageDisplay
    {
        public GameObject clearTextObject;
        public Text bestScoreText;
        public Text lastScoreText;
    }

    [SerializeField] private StageDisplay[] stageDisplays;

    // StartをOnEnableに変更（または両方書く）
    // これにより、画面が表示されるたびに最新スコアを読み直します
    void OnEnable()
    {
        RefreshDisplay();
    }

    public void RefreshDisplay()
    {
        for (int i = 0; i < stageDisplays.Length; i++)
        {
            int stageNum = i + 1;

            // 保存されたデータを取得
            bool isCleared = PlayerPrefs.GetInt($"Stage{stageNum}Clear", 0) == 1;
            int best = PlayerPrefs.GetInt($"Stage{stageNum}HighScore", 0);
            int last = PlayerPrefs.GetInt($"Stage{stageNum}LastScore", 0);

            // コンソールに読み込み結果を表示（デバッグ用）
            Debug.Log($"Stage{stageNum}読み込み: Clear={isCleared}, Best={best}");

            if (stageDisplays[i].clearTextObject != null)
                stageDisplays[i].clearTextObject.SetActive(isCleared);

            if (stageDisplays[i].bestScoreText != null)
                stageDisplays[i].bestScoreText.text = $"Best: {best}";

            if (stageDisplays[i].lastScoreText != null)
                stageDisplays[i].lastScoreText.text = $"Last: {last}";
        }
    }
}