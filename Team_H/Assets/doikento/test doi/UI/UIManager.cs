using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("ステージ設定")]
    [Tooltip("このシーンのステージ番号 (1, 2, 3...)")]
    [SerializeField] private int stageNumber = 1;

    [Header("スコア表示")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("レベル表示")]
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("進行ゲージUI")]
    [SerializeField] private Image progressBar;
    [SerializeField] private CanvasGroup progressGroup;

    [Header("インベントリ")]
    [SerializeField] private GameObject inventoryPanel;//全体
    [SerializeField] private GameObject equipmentInventoryUI;//設備用
    [SerializeField] private GameObject seedInventoryUI;//種用
    [SerializeField] private Button equipmentTabButton;//設備用ボタン
    [SerializeField] private Button seedTabButton;//種タブボタン

    [Header("リザルトUI")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button stageSelectButton;

    [Header("設備説明UI")]
    [SerializeField] private GameObject descriptionPanel;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("ツールチップ")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipText;

    private player_move player;
    private bool isInventoryOpen = false;

    private void Start()
    {
        // リザルト画面が ON になっていた場合 OFF にしておく
        if (resultPanel != null && resultPanel.activeSelf)
        {
            resultPanel.SetActive(false);
        }

        // インベントリUI 初期状態は非表示
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        // 説明パネル 初期は非表示
        if (descriptionPanel != null)
            descriptionPanel.SetActive(false);

        // プレイヤー情報を取得
        player = FindAnyObjectByType<player_move>();

        // 初期状態では設備タブを表示
        ShowEquipmentInventory();

        // タブ切り替えボタン設定
        equipmentTabButton?.onClick.AddListener(ShowEquipmentInventory);
        seedTabButton?.onClick.AddListener(ShowSeedInventory);
    }

    /// <summary>
    /// リザルト画面を表示し、クリア時はデータを保存する
    /// </summary>
    /// <param name="isClear"></param>
    public void ShowResult(bool isClear)
    {
        if (resultPanel == null) return;

        resultPanel.SetActive(true);

        // タイトルテキストの設定
        titleText.text = isClear ? "STAGE CLEAR" : "GAME OVER";

        // スコアの取得と表示
        if (player != null)
        {
            int currentScore = GetPlayerScore();
            _scoreText.text = $"SCORE: {currentScore}";

            // ステージクリア時のみデータを保存
            if (isClear)
            {
                SaveStageData(currentScore);
            }
        }
        else
        {
            _scoreText.text = "";
        }

        // ボタン表示制御
        retryButton.gameObject.SetActive(!isClear);
        stageSelectButton.gameObject.SetActive(true);

        // ゲーム操作を止める
        WaveManager.PlayerCanControl = false;
        WaveManager.CanGrow = false;
    }

    /// <summary>
    /// PlayerPrefsを使用してステージデータを保存
    /// </summary>
    private void SaveStageData(int score)
    {
        // クリアフラグ (1 = クリア済み)
        PlayerPrefs.SetInt($"Stage{stageNumber}Clear", 1);

        // 今回のスコアを「前回のスコア」として保存
        PlayerPrefs.SetInt($"Stage{stageNumber}LastScore", score);

        // ハイスコアの更新
        int currentBest = PlayerPrefs.GetInt($"Stage{stageNumber}HighScore", 0);
        if (score > currentBest)
        {
            PlayerPrefs.SetInt($"Stage{stageNumber}HighScore", score);
        }

        PlayerPrefs.Save();
        UnityEngine.Debug.Log($"Stage {stageNumber} Data Saved: Score {score}");
    }

    /// <summary>
    /// リフレクションを使用してplayer_moveから現在のスコアを数値で取得
    /// </summary>
    private int GetPlayerScore()
    {
        if (player == null) return 0;
        var field = player.GetType().GetField("currentScore", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field != null ? (int)field.GetValue(player) : 0;
    }

    /// <summary>
    /// プレイヤーのスコア表示用文字列（UI更新用）
    /// </summary>
    private string playerScoreText()
    {
        return GetPlayerScore().ToString();
    }

    // リトライボタン処理
    public void OnRetryButton()
    {
        LoadingManager.nextSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene("LoadingScene");
    }

    // ステージセレクトへ戻る
    public void OnStageSelectButton()
    {
        LoadingManager.nextSceneName = "StageSelect"; //セレクト画面のシーン名
        SceneManager.LoadScene("LoadingScene");
    }

    // インベントリの開閉
    public void ToggleInventory()
    {
        if (inventoryPanel == null) return;

        isInventoryOpen = !isInventoryOpen;
        inventoryPanel.SetActive(isInventoryOpen);

        if (!isInventoryOpen)
        {
            HideDescription();
        }
    }

    public void CloseInventory()
    {
        inventoryPanel.SetActive(false);
        descriptionPanel.SetActive(false);
    }

    public void ShowEquipmentInventory()
    {
        if (equipmentInventoryUI == null || seedInventoryUI == null) return;
        equipmentInventoryUI.SetActive(true);
        seedInventoryUI.SetActive(false);
    }

    public void ShowSeedInventory()
    {
        if (equipmentInventoryUI == null || seedInventoryUI == null) return;
        equipmentInventoryUI.SetActive(false);
        seedInventoryUI.SetActive(true);
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $": {score}";
    }

    public void UpdateLevel(int level, int currentExp, int expToNext)
    {
        if (levelText != null)
            levelText.text = $"   : {currentExp}/{expToNext} : Lv{level}";
    }

    public void ShowProgress(bool show)
    {
        if (progressGroup != null)
            progressGroup.alpha = show ? 1 : 0;

        if (progressBar != null)
            progressBar.gameObject.SetActive(show);
    }

    public void SetProgress(float value)
    {
        if (progressBar != null)
            progressBar.fillAmount = Mathf.Clamp01(value);
    }

    public void ShowDescription(string text)
    {
        if (descriptionPanel != null)
            descriptionPanel.SetActive(true);

        if (descriptionText != null)
            descriptionText.text = text;
    }

    public void HideDescription()
    {
        if (descriptionPanel != null)
            descriptionPanel.SetActive(false);
    }

    public void ShowTooltip(string text, Vector3 worldPos)
    {
        if (tooltipPanel == null) return;

        tooltipPanel.SetActive(true);
        tooltipText.text = text;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        tooltipPanel.transform.position = screenPos + new Vector3(0, 40f, 0);
    }

    public void HideTooltip()
    {
        if (tooltipPanel == null) return;
        tooltipPanel.SetActive(false);
    }
}