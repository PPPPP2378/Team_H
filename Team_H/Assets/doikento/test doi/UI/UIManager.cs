using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
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

    private player_move player;
    private bool isInventoryOpen = false;

    private void Start()
    {
        if (resultPanel != null && resultPanel.activeSelf)
        {
                resultPanel.SetActive(false);
            Debug.Log("ResultPanel を初期化時に非表示にしました。");
        }
        // インベントリも非表示で開始
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        player = FindAnyObjectByType<player_move>();

        // 初期インベントリの表示
        ShowEquipmentInventory();

        // タブボタンにイベント登録
        equipmentTabButton?.onClick.AddListener(ShowEquipmentInventory);
        seedTabButton?.onClick.AddListener(ShowSeedInventory);
    }

    public void ShowResult(bool isClear)
    {
        if (resultPanel == null) return;

        resultPanel.SetActive(true);

        // タイトル
        titleText.text = isClear ? "STAGE CLEAR" : "GAME OVER";

        // スコア表示（任意）
        if (player != null)
            scoreText.text = $"SCORE: {playerScoreText()}";
        else
            scoreText.text = "";

        // ボタン表示制御
        retryButton.gameObject.SetActive(!isClear); // ゲームオーバー時のみ再挑戦ボタンON
        stageSelectButton.gameObject.SetActive(true);

        // 操作完全停止
        WaveManager.PlayerCanControl = false;
        WaveManager.CanGrow = false;
    }

    private string playerScoreText()
    {
        return $"{player.GetType().GetField("currentScore", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(player)}";
    }

    // 再挑戦ボタン
    public void OnRetryButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ステージ選択に戻る
    public void OnStageSelectButton()
    {
        SceneManager.LoadScene("stageselect"); // あなたのステージ選択シーン名に変更
    }

    public void ToggleInventory()
    {
        if (inventoryPanel == null) return;

        isInventoryOpen = !isInventoryOpen;
        inventoryPanel.SetActive(!isInventoryOpen);
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
    //スコア
    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }

    //レベル
    public void UpdateLevel(int level, int currentExp, int expToNext)
    {
        if (levelText != null)
            levelText.text = $"Lv {level}  EXP: {currentExp}/{expToNext}";
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
}
