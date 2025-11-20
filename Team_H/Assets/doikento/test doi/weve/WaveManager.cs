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
    public TextMeshProUGUI gameOverText; //ゲームオーバー表示用

    [Header("敵スポナー")]
    public EnemySpawner spawner; // 敵を出す専用スクリプト

    private int currentWave = 0; // 現在のウェーブ番号
    private float timer = 0f;    // タイマー（準備 or 戦闘時間）
    private bool inWave = false; // ウェーブ中かどうか
    private bool inPrep = false; // 準備時間中かどうか

    private bool isTextActive = false;   //テキスト表示地中のフラグ
    private Coroutine stateTextCoroutine;// 状態テキストのコルーチン制御用
    private int startPlowCount=0;//ウェーブ開始時の畑
    private bool isGameOver = false;//ゲームオーバー判定
    public static bool CanGrow = false; //成長可能フラグ
    public static bool PlayerCanControl = true; // プレイヤー操作可否

    [Header("畑スプライト設定")]
    public Sprite plowedSoilSprite; // ← Plow状態のスプライトをここに設定

    [Header("畑カウント設定")]
    public string[] fieldTags = { "Plow", "Plowed", "Moist_Plowe", "Seed", "Grown" };
    public float fieldCheckInterval = 3f; // 何秒ごとに再カウントするか

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
            CanGrow = false; // 成長ストップ！
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

            //一時的に止めておく
            CanGrow = false;
            spawner.StopSpawning();

            ShowStateText ($"WAVE {currentWave} START",displayTime);

            // テキストが消えるまで待機
            while (isTextActive)
                yield return null;

            //テキストが消えたらスタート
            CanGrow = true;
            spawner.StartSpawning(currentWave); // 敵出現開始
            timer = waveDuration;

            //畑カウントの自動チェックを開始
            StartCoroutine(CheckFieldStatusRoutine());

            //ウェーブ開始時に「Plow」タグの数を記録
            startPlowCount = CountAllFieldTiles();
            Debug.Log($"WAVE {currentWave} 開始時の畑数: {startPlowCount}");

            // ウェーブ中（テキスト表示中はタイマーを止める）
            while (timer > 0)
            {
                if (!isTextActive)
                    timer -= Time.deltaTime;

                UpdateUI();

                //毎フレーム「Plow」数をチェック
                if (CountAllFieldTiles() <= 0)
                {
                    Debug.Log("畑がすべて荒らされました → ゲームオーバー");
                    TriggerGameOver();
                }
                yield return null;
            }

            // --- ウェーブ終了 ---
            inWave = false;
            CanGrow = false; // 成長ストップ！
            spawner.StopSpawning();
            // ウェーブ終了時に敵を全削除
            RabbitAI_Complete.RemoveAllRabbits();

            if (isGameOver)
                yield break; //ゲームオーバーならここで終了

            CollectAllGrownCrops();
            ShowStateText($"WAVE {currentWave} CLEAR", displayTime);
            yield return new WaitForSeconds(2f);
            // 「WAVE X CLEAR」表示
            ShowStateText($"WAVE {currentWave} CLEAR",displayTime);
            // 次のウェーブに行く前に少し待機
            yield return new WaitForSeconds(2f);
        }

        // 全ウェーブ終了
        ShowStateText("STAGE CLEAR",displayTimeClear);
        spawner.StopSpawning();

        // 成長・操作ともに完全停止
        CanGrow = false;
        PlayerCanControl = false;

        UnlockNextStage();

        //リザルト
        UIManager ui = FindAnyObjectByType<UIManager>();
        if (ui != null) ui.ShowResult(true);
    }

    // Grown タグの作物をすべて回収
    private void CollectAllGrownCrops()
    {
        GameObject[] grownCrops = GameObject.FindGameObjectsWithTag("Grown");

        if (grownCrops.Length == 0)
        {
            Debug.Log("収穫する作物はありません。");
            return;
        }

        int totalCollected = 0;

        // プレイヤー取得
        player_move player = FindAnyObjectByType<player_move>();
        if (player == null)
        {
            Debug.LogWarning("player_move が見つかりません。スコア加算できません。");
            return;
        }

        foreach (GameObject crop in grownCrops)
        {
            if (crop == null) continue;

            //スコア加算
            player.AddScore(10);

            //スプライト変更
            SpriteRenderer sr = crop.GetComponent<SpriteRenderer>();
            if (sr != null && plowedSoilSprite != null)
            {
                sr.sprite = plowedSoilSprite;
            }

            //タグを Plow に変更
            crop.tag = "Plow";

            totalCollected++;
        }

        Debug.Log($"Grown 作物を {totalCollected} 個収穫 → Plow に戻しました。");
    }

    // 畑タグをまとめてカウント
    private int CountAllFieldTiles()
    {
        // ゲーム内で「畑」と見なすタグをここで指定
        string[] fieldTags = { "Plow", "Seed", "Grown", "Moist_Plowe", "Plowed" };

        int total = 0;
        foreach (string tag in fieldTags)
        {
            GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
            total += objs.Length;
        }

        return total;
    }

    // ゲームオーバー処理
    private void TriggerGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        spawner.StopSpawning();
        RabbitAI_Complete.RemoveAllRabbits();

        // 成長停止・操作停止
        CanGrow = false;
        PlayerCanControl = false;

        ShowStateText("GAME OVER", displayTimeClear);
        Debug.Log("ゲームオーバー処理を実行しました。");

        UIManager ui = FindAnyObjectByType<UIManager>();
        if (ui != null) ui.ShowResult(false);
    }

    // UI（ウェーブ番号・残り時間）の更新
    private void UpdateUI()
    {
        if (waveText != null)
            waveText.text = $"WAVE: {currentWave}/{maxWave}";

        if (timerText != null)
            timerText.text = $"TIME: {timer:F1}";
    }

    // 一定時間だけステートテキストを表示して自動で消す
    private void ShowStateText(string text, float duration)
    {
        if (stateText == null) return;

        // テキストを表示し、フラグをON
        stateText.text = text;
        isTextActive = true;

        // === プレイヤー操作をロック ===
        PlayerCanControl = false;

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

        // === 操作を再び有効化 ===
        PlayerCanControl = true;
    }

    private IEnumerator CheckFieldStatusRoutine()
    {
        while (inWave) // ウェーブ中のみ実行
        {
            int totalFields = 0;

            // 登録されたタグをすべてチェック
            foreach (string tag in fieldTags)
            {
                totalFields += GameObject.FindGameObjectsWithTag(tag).Length;
            }

            Debug.Log($"畑の残数: {totalFields}");

            if (totalFields == 0)
            {
                Debug.Log("すべての畑が失われました…ゲームオーバー！");
                TriggerGameOver();
                yield break;
            }

            yield return new WaitForSeconds(fieldCheckInterval);
        }
    }

    private void UnlockNextStage()
    {
        // 現在のステージ番号をどこかに設定しておく必要がある
        // 例：Stage1 → 1, Stage2 → 2
        int currentStage = GetCurrentStageNumber();

        // セーブされている解放済み最高ステージ番号を取得（初期値1）
        int unlocked = PlayerPrefs.GetInt("UnlockedStage", 1);

        // 今クリアしたステージが解放済み最高より大きければ更新
        if (currentStage >= unlocked)
        {
            PlayerPrefs.SetInt("UnlockedStage", currentStage + 1);
            PlayerPrefs.Save();
            Debug.Log($"ステージ {currentStage + 1} を解放しました！");
        }

    }
    private int GetCurrentStageNumber()
    {
        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // "Stage1" → 1 に変換
        if (scene.StartsWith("Stage"))
        {
            string numStr = scene.Replace("Stage", "");
            if (int.TryParse(numStr, out int num))
                return num;
        }

        return 1; // デフォルト
    }
}