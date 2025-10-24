using System.Collections;
using UnityEngine;
using TMPro;

public class WaveManager : MonoBehaviour
{
    [Header("�E�F�[�u�ݒ�")]
    public float prepTime = 10f;     // �������ԁi�b�j
    public float waveDuration = 20f; // �e�E�F�[�u�̒����i�b�j
    public int maxWave = 5;          // �S�E�F�[�u��

    [Header("�e�L�X�g�\������")]
    public float displayTime = 2f;     //UI�\������
    public float displayTimeClear = 5f;//�X�e�[�W�N���A���̕\������

    [Header("UI�\���p")]
    public TextMeshProUGUI waveText; // ���݂̃E�F�[�u�\��
    public TextMeshProUGUI timerText;// �^�C�}�[�\��
    public TextMeshProUGUI stateText;// ���

    [Header("�G�X�|�i�[")]
    public EnemySpawner spawner; // �G���o����p�X�N���v�g

    private int currentWave = 0; // ���݂̃E�F�[�u�ԍ�
    private float timer = 0f;    // �^�C�}�[�i���� or �퓬���ԁj
    private bool inWave = false; // �E�F�[�u�����ǂ���
    private bool inPrep = false; // �������Ԓ����ǂ���

    private bool isTextActive = false;   //�e�L�X�g�\���n���̃t���O
    private Coroutine stateTextCoroutine;// ��ԃe�L�X�g�̃R���[�`������p

    // �Q�[���J�n���ɃE�F�[�u�Ǘ����[�v���J�n
    void Start()
    {
        StartCoroutine(GameLoop());
    }

    // �E�F�[�u�i�s�̃��C�����[�v
    private IEnumerator GameLoop()
    {
        while (currentWave < maxWave)
        {
            // --- �����t�F�[�Y ---
            inPrep = true;
            inWave = false;
            ShowStateText  ("PREPA TIME",displayTime);
            
            spawner.StopSpawning();
            timer = prepTime;
            
            // �������Ԓ��i�e�L�X�g�\�����̓^�C�}�[���~�߂�j
            while (timer > 0)
            {
                if(!isTextActive)
                    timer -= Time.deltaTime;

                UpdateUI();
                yield return null;
            }

            // --- �E�F�[�u�J�n ---
            currentWave++;
            inPrep = false;
            inWave = true;

            ShowStateText ($"WAVE {currentWave} START",displayTime);
            spawner.StartSpawning(currentWave); // �G�o���J�n
            timer = waveDuration;

            // �E�F�[�u���i�e�L�X�g�\�����̓^�C�}�[���~�߂�j
            while (timer > 0)
            {
                if (!isTextActive)
                    timer -= Time.deltaTime;

                UpdateUI();
                yield return null;
            }

            // --- �E�F�[�u�I�� ---
            inWave = false;
            spawner.StopSpawning();

            // �uWAVE X CLEAR�v�\��
            ShowStateText($"WAVE {currentWave} CLEAR",displayTime);
            // ���̃E�F�[�u�ɍs���O�ɏ����ҋ@
            yield return new WaitForSeconds(2f);
        }

        // �S�E�F�[�u�I��
        ShowStateText("STAGE CLEAR",displayTimeClear);
        spawner.StopSpawning();
    }

    // UI�i�E�F�[�u�ԍ��E�c�莞�ԁj�̍X�V
    private void UpdateUI()
    {
        if (waveText != null)
            waveText.text = $"WAVE: {currentWave}/{maxWave}";

        if (timerText != null)
            timerText.text = $"TIME: {timer:F1} SECOND";
    }

    // ��莞�Ԃ����X�e�[�g�e�L�X�g��\�����Ď����ŏ���
    private void ShowStateText(string text, float duration)
    {
        if (stateText == null) return;

        // �e�L�X�g��\�����A�t���O��ON
        stateText.text = text;
        isTextActive = true;

        // ���ɑ��̕\���R���[�`��(Coroutine)�������Ă���ꍇ�͎~�߂�
        if (stateTextCoroutine != null)
            StopCoroutine(stateTextCoroutine);

        // �V�����R���[�`��(Coroutine)���J�n
        stateTextCoroutine = StartCoroutine(HideStateTextAfter(duration));

        
    }

    // �w�莞�Ԍ��stateText����������
    private IEnumerator HideStateTextAfter(float time)
    {
        yield return new WaitForSeconds(time);

        // �e�L�X�g������
        if (stateText != null)
            stateText.text = "";

        // �t���O��OFF�ɖ߂�
        isTextActive = false;
        stateTextCoroutine = null;
    }
}