using System.Collections;
using UnityEngine;
using TMPro;

public class WaveManager : MonoBehaviour
{
    [Header("ウェーブ設定")]
    public float prepTime = 10f;     // 準備時間（秒）
    public float waveDuration = 20f; // 各ウェーブの長さ（秒）
    public int maxWave = 5;          // 全ウェーブ数

    [Header("テキスト表示時間")]
    public float displayTime = 2f;     //UI表示時間
    public float displayTimeClear = 5f;//ステージクリア時の表示時間

    [Header("UI表示用")]
    public TextMeshProUGUI waveText; // 現在のウェーブ表示
    public TextMeshProUGUI timerText;// タイマー表示
    public TextMeshProUGUI stateText;// 状態

    [Header("敵スポナー")]
    public EnemySpawner spawner; // 敵を出す専用スクリプト

    private int currentWave = 0; // 現在のウェーブ番号
    private float timer = 0f;    // タイマー（準備 or 戦闘時間）
    private bool inWave = false; // ウェーブ中かどうか
    private bool inPrep = false; // 準備時間中かどうか

    private bool isTextActive = false;   //テキスト表示地中のフラグ
    private Coroutine stateTextCoroutine;// 状態テキストのコルーチン制御用

    // ゲーム開始時にウェーブ管理ループを開始
    void Start()
    {
        StartCoroutine(GameLoop());
    }

    // ウェーブ進行のメインループ
    private IEnumerator GameLoop()
    {
        while (currentWave < maxWave)
        {
            // --- 準備フェーズ ---
            inPrep = true;
            inWave = false;
            ShowStateText  ("PREPA TIME",displayTime);
            
            spawner.StopSpawning();
            timer = prepTime;
            
            // 準備時間中（テキスト表示中はタイマーを止める）
            while (timer > 0)
            {
                if(!isTextActive)
                    timer -= Time.deltaTime;

                UpdateUI();
                yield return null;
            }

            // --- ウェーブ開始 ---
            currentWave++;
            inPrep = false;
            inWave = true;

            ShowStateText ($"WAVE {currentWave} START",displayTime);
            spawner.StartSpawning(currentWave); // 敵出現開始
            timer = waveDuration;

            // ウェーブ中（テキスト表示中はタイマーを止める）
            while (timer > 0)
            {
                if (!isTextActive)
                    timer -= Time.deltaTime;

                UpdateUI();
                yield return null;
            }

            // --- ウェーブ終了 ---
            inWave = false;
            spawner.StopSpawning();

            // 「WAVE X CLEAR」表示
            ShowStateText($"WAVE {currentWave} CLEAR",displayTime);
            // 次のウェーブに行く前に少し待機
            yield return new WaitForSeconds(2f);
        }

        // 全ウェーブ終了
        ShowStateText("STAGE CLEAR",displayTimeClear);
        spawner.StopSpawning();
    }

    // UI（ウェーブ番号・残り時間）の更新
    private void UpdateUI()
    {
        if (waveText != null)
            waveText.text = $"WAVE: {currentWave}/{maxWave}";

        if (timerText != null)
            timerText.text = $"TIME: {timer:F1} SECOND";
    }

    // 一定時間だけステートテキストを表示して自動で消す
    private void ShowStateText(string text, float duration)
    {
        if (stateText == null) return;

        // テキストを表示し、フラグをON
        stateText.text = text;
        isTextActive = true;

        // 既に他の表示コルーチン(Coroutine)が動いている場合は止める
        if (stateTextCoroutine != null)
            StopCoroutine(stateTextCoroutine);

        // 新しいコルーチン(Coroutine)を開始
        stateTextCoroutine = StartCoroutine(HideStateTextAfter(duration));

        
    }

    // 指定時間後にstateTextを消す処理
    private IEnumerator HideStateTextAfter(float time)
    {
        yield return new WaitForSeconds(time);

        // テキストを消す
        if (stateText != null)
            stateText.text = "";

        // フラグをOFFに戻す
        isTextActive = false;
        stateTextCoroutine = null;
    }
}