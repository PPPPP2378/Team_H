using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
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

    public void ShowResult(bool isClear)
    {
        if (resultPanel == null) return;

        resultPanel.SetActive(true);

        // タイトルテキストを元に戻す
        titleText.text = isClear ? "STAGE CLEAR" : "GAME OVER";

        // スコア表示
        if (player != null)
            _scoreText.text = $"SCORE: {playerScoreText()}";
        else
            _scoreText.text = "";
        // ボタン表示制御
        retryButton.gameObject.SetActive(!isClear);
        stageSelectButton.gameObject.SetActive(true);
        // ゲーム操作を止める
        WaveManager.PlayerCanControl = false;
        WaveManager.CanGrow = false;
    }

    private string playerScoreText()
    {
        return $"{player.GetType().GetField("currentScore", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(player)}";
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

        // インベントリを閉じた時に説明文を強制的に消す
        if (!isInventoryOpen)
        {
            HideDescription();
        }
    }
    // 設備タブの表示
    public void ShowEquipmentInventory()
    {
        if (equipmentInventoryUI == null || seedInventoryUI == null) return;

        equipmentInventoryUI.SetActive(true);
        seedInventoryUI.SetActive(false);
    }
    // 種タブの表示
    public void ShowSeedInventory()
    {
        if (equipmentInventoryUI == null || seedInventoryUI == null) return;

        equipmentInventoryUI.SetActive(false);
        seedInventoryUI.SetActive(true);
    }
    // スコア表示の更新
    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $": {score}";
    }
    // レベル情報の更新
    public void UpdateLevel(int level, int currentExp, int expToNext)
    {
        if (levelText != null)
            levelText.text = $"   : {currentExp}/{expToNext} : Lv{level}";
    }
    // 進行ゲージの表示切り替え
    public void ShowProgress(bool show)
    {
        if (progressGroup != null)
            progressGroup.alpha = show ? 1 : 0;

        if (progressBar != null)
            progressBar.gameObject.SetActive(show);
    }
    // プログレスバー進行値の設定
    public void SetProgress(float value)
    {
        if (progressBar != null)
            progressBar.fillAmount = Mathf.Clamp01(value);
    }
    // 設備・種の説明テキスト表示
    public void ShowDescription(string text)
    {
        if (descriptionPanel != null)
            descriptionPanel.SetActive(true);

        if (descriptionText != null)
            descriptionText.text = text;
    }
    // 説明パネルを閉じる
    public void HideDescription()
    {
        if (descriptionPanel != null)
            descriptionPanel.SetActive(false);
    }
}