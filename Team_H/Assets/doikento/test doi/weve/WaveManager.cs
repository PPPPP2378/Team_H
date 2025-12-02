using System.Collections;

using TMPro;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("ウェーブ設定")]
    public float prepTime = 10f;
    public float waveDuration = 20f;
    public int maxWave = 5;

    [Header("テキスト表示時間")]
    public float displayTime = 2f;
    public float displayTimeClear = 5f;

    [Header("UI表示用")]
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI stateText;
    public TextMeshProUGUI gameOverText;

    [Header("敵スポナー")]
    public EnemySpawner spawner;

    private int currentWave = 0;
    private float timer = 0f;
    private bool inWave = false;
    private bool inPrep = false;

    private bool isTextActive = false;
    private Coroutine stateTextCoroutine;

    private bool isGameOver = false;
    public static bool CanGrow = false;
    public static bool PlayerCanControl = true;

    [Header("畑破壊の設定")]
    public int maxDestroyedCount = 5;
    private int destroyedCount = 0;

    [Header("BGM設定")]
    public AudioSource prepBGM;
    public AudioSource waveBGM;

    void Start()
    {
        StartCoroutine(GameLoop());
    }

    private IEnumerator GameLoop()
    {
        while (currentWave < maxWave)
        {
            // --- 準備フェーズ ---
            PlayPrepBGM();
            inPrep = true;
            inWave = false;
            CanGrow = false;
            ShowStateText("PREPA TIME", displayTime);

            spawner.StopSpawning();
            timer = prepTime;

            while (timer > 0)
            {
                if (!isTextActive) timer -= Time.deltaTime;
                UpdateUI();
                yield return null;
            }

            // --- ウェーブ開始 ---
            currentWave++;
            PlayWaveBGM();
            inPrep = false;
            inWave = true;

            CanGrow = false;        // 一瞬オフ
            spawner.StopSpawning();

            ShowStateText($"WAVE {currentWave} START", displayTime);

            while (isTextActive) yield return null;

            CanGrow = true;
            spawner.StartSpawning(currentWave);
            timer = waveDuration;

            while (timer > 0)
            {
                if (!isTextActive) timer -= Time.deltaTime;
                UpdateUI();
                yield return null;
            }

            // --- ウェーブ終了 ---
            inWave = false;
            CanGrow = false;
            spawner.StopSpawning();
            RabbitAI_Complete.RemoveAllRabbits();

            if (isGameOver)
                yield break;

            CollectAllGrownCrops();
            ShowStateText($"WAVE {currentWave} CLEAR", displayTime);
            yield return new WaitForSeconds(2f);
        }

        // --- ステージクリア ---
        StopAllBGM();
        ShowStateText("STAGE CLEAR", displayTimeClear);
        spawner.StopSpawning();

        CanGrow = false;
        PlayerCanControl = false;

        UnlockNextStage();

        UIManager ui = FindAnyObjectByType<UIManager>();
        if (ui != null) ui.ShowResult(true);
    }

    // --- 収穫処理 ---
    private void CollectAllGrownCrops()
    {
        GameObject[] grownCrops = GameObject.FindGameObjectsWithTag("Grown");

        if (grownCrops.Length == 0) return;

        player_move player = FindAnyObjectByType<player_move>();
        if (player == null) return;

        foreach (GameObject crop in grownCrops)
        {
            player.AddScore(10);

            SpriteRenderer sr = crop.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = null;  // ※必要なら plowedSoilSprite に戻す
            }

            crop.tag = "Plow";
        }
    }

    // --- ゲームオーバー処理 ---
    private void TriggerGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        StopAllBGM();
        spawner.StopSpawning();
        RabbitAI_Complete.RemoveAllRabbits();

        CanGrow = false;
        PlayerCanControl = false;

        ShowStateText("GAME OVER", displayTimeClear);

        UIManager ui = FindAnyObjectByType<UIManager>();
        if (ui != null) ui.ShowResult(false);
    }

    // --- UI 更新 ---
    private void UpdateUI()
    {
        if (waveText != null)
            waveText.text = $": {currentWave}/{maxWave}";

        if (timerText != null)
            timerText.text = $"TIME: {timer:F1}";
    }

    // --- 状態テキスト --- 
    private void ShowStateText(string text, float duration)
    {
        if (stateText == null) return;

        stateText.text = text;
        isTextActive = true;
        PlayerCanControl = false;

        if (stateTextCoroutine != null)
            StopCoroutine(stateTextCoroutine);

        stateTextCoroutine = StartCoroutine(HideStateTextAfter(duration));
    }

    private IEnumerator HideStateTextAfter(float time)
    {
        yield return new WaitForSeconds(time);

        if (stateText != null)
            stateText.text = "";

        isTextActive = false;
        stateTextCoroutine = null;
        PlayerCanControl = true;
    }

    // --- Rabbit から呼ばれる破壊通知 ---
    public void AddDestroyedField()
    {
        destroyedCount++;

        Debug.Log($"畑が荒らされた！ {destroyedCount}/{maxDestroyedCount}");

        if (destroyedCount >= maxDestroyedCount)
        {
            TriggerGameOver();
        }
    }

    // --- BGM ---
    private void PlayPrepBGM()
    {
        if (waveBGM) waveBGM.Stop();
        if (prepBGM && !prepBGM.isPlaying) prepBGM.Play();
    }

    private void PlayWaveBGM()
    {
        if (prepBGM) prepBGM.Stop();
        if (waveBGM && !waveBGM.isPlaying) waveBGM.Play();
    }

    private void StopAllBGM()
    {
        if (prepBGM) prepBGM.Stop();
        if (waveBGM) waveBGM.Stop();
    }


    // --- 次ステージ解放 ---
    private void UnlockNextStage()
    {
        int currentStage = GetCurrentStageNumber();
        int unlocked = PlayerPrefs.GetInt("UnlockedStage", 1);

        if (currentStage >= unlocked)
        {
            PlayerPrefs.SetInt("UnlockedStage", currentStage + 1);
            PlayerPrefs.Save();
        }
    }

    private int GetCurrentStageNumber()
    {
        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (scene.StartsWith("Stage"))
        {
            string numStr = scene.Replace("Stage", "");
            if (int.TryParse(numStr, out int num)) return num;
        }

        return 1;
    }
}