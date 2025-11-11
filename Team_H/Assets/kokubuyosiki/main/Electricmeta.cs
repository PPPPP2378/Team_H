using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Electricmeta : MonoBehaviour
{
    [Header("UI Components")]
    public Slider electricSlider;
    public TMP_Text electricText;

    [Header("Electric Stats")]
    public float currentElectric = 0f;
    public float maxElectric = 10f;
    public float electricPerSecond = 1f;

    void Start()
    {
        // スライダー初期化
        electricSlider.maxValue = maxElectric;
        electricSlider.value = currentElectric;
        UpdateUI();
    }

    void Update()
    {
        // 時間経過で電力を増加
        currentElectric += electricPerSecond * Time.deltaTime;

        // 上限チェック
        currentElectric = Mathf.Clamp(currentElectric, 0, maxElectric);

        // UIを更新
        UpdateUI();
    }

    void UpdateUI()
    {
        if (electricSlider != null)
            electricSlider.value = currentElectric;

        if (electricText != null)
            electricText.text = $"{currentElectric:F2} / {maxElectric}";
    }
}
