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


    private player_move player;
    private bool isInventoryOpen = false;

    private void Start()
    {
        if (resultPanel != null && resultPanel.activeSelf)
        {
            resultPanel.SetActive(false);
           
        }

        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        player = FindAnyObjectByType<player_move>();

        ShowEquipmentInventory();

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

        retryButton.gameObject.SetActive(!isClear);
        stageSelectButton.gameObject.SetActive(true);

        WaveManager.PlayerCanControl = false;
        WaveManager.CanGrow = false;
    }

    private string playerScoreText()
    {
        return $"{player.GetType().GetField("currentScore", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(player)}";
    }

    public void OnRetryButton()
    {
        LoadingManager.nextSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene("LoadingScene");
    }

    public void OnStageSelectButton()
    {
        LoadingManager.nextSceneName = "StageSelect"; //セレクト画面のシーン名
        SceneManager.LoadScene("LoadingScene");
    }

    public void ToggleInventory()
    {
        if (inventoryPanel == null) return;

        isInventoryOpen = !isInventoryOpen;
        inventoryPanel.SetActive(isInventoryOpen);
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
}