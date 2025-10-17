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
        // �X���C�_�[������
        electricSlider.maxValue = maxElectric;
        electricSlider.value = currentElectric;
        UpdateUI();
    }

    void Update()
    {
        // ���Ԍo�߂œd�͂𑝉�
        currentElectric += electricPerSecond * Time.deltaTime;

        // ����`�F�b�N
        currentElectric = Mathf.Clamp(currentElectric, 0, maxElectric);

        // UI���X�V
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
