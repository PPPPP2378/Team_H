using UnityEngine;
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

    private bool isInventoryOpen = false;

    private void Start()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        // 初期インベントリの表示
        ShowEquipmentInventory();

        // タブボタンにイベント登録
        equipmentTabButton?.onClick.AddListener(ShowEquipmentInventory);
        seedTabButton?.onClick.AddListener(ShowSeedInventory);
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
