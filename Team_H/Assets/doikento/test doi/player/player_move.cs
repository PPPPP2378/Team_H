using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;



[System.Serializable]
public class EquipmentLevelData
{
    public Sprite sprite;　　　//グラフィック
    public int cost;            //設置コスト
    public int damage;          // 敵に与えるダメージ量
}

[System.Serializable]
public class EquipmentData
{
    public string name;              // 設備名
    public EquipmentLevelData[] levels; // レベルごとのデータ
}



public class player_move : MonoBehaviour
{
    public float speed = 3f; //プレイヤーの移動速度
    [Header("設備リスト")]
    [SerializeField] private EquipmentData[] equipments; // 複数設備データ

    private int selectedEquipmentIndex = -1;               // 現在選択中の設備番号
    
    private Rigidbody2D rb;
    private Collider2D currentTarget; // 今触れているオブジェクトを記録する
    private Vector2 movement;

    [Header("畑のスプライト差し替え用")]
    [SerializeField] private Sprite plowedSprite;   // 耕した後
    [SerializeField] private Sprite wateredSprite;  // 水やり後
    [SerializeField] private Sprite seedSprite;     // 種を植えた後
    [SerializeField] private Sprite seed_Sprite;   // 成長段階
    [SerializeField] private Sprite grownSprite;    // 成長後の見た目（追加）
    [SerializeField] private Sprite plowSprite;     // 耕す前

    [Header("成長にかかる時間(秒)")]
    [SerializeField] public float growTime = 5f;   // 種が育つまでの時間
    [SerializeField] public float fullgrowTime = 5f;

    [Header("スコア設定")]
    [SerializeField] private int harvestPoints = 10;          // 1回収穫ごとのポイント
    [SerializeField] private TextMeshProUGUI scoreText;       // UI表示用
    private int currentScore = 100;                           //初期スコア


    [Header("操作設定")]
    [SerializeField] private float holdInterval = 0.8f; // 長押し判定時間
    public float holdTimer = 0f;
    private bool isHolding = false; // 今キーを押しているかどうか

    [Header("UI設定")]
    [SerializeField] private Image progressBar;         // ゲージ部分のImageをここにドラッグ
    [SerializeField] private CanvasGroup progressGroup; //  ゲージ全体をまとめてフェードする用（任意）

    [Header("レベルシステム")]
    [SerializeField] private TextMeshProUGUI levelText;
    private int playerLevel = 1;
    private int currentExp = 0;
    private int expToNext = 50; // 次のレベルに必要な経験値
    private float speedGrowthRate = 0.2f; // レベルアップごとの速度増加
    private float holdReductionRate = 0.1f; // 長押し時間短縮率

    [Header("ハイライト")]
    [SerializeField] private Sprite highlightSprite;//赤湧くスプライト
    private GameObject highlightFrame;    //赤枠オブジェクト
    public int on_in_layer = 2;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;

        UpdateScoreUI(); // 初期スコアを表示

        if(highlightSprite!=null)
        {
            highlightFrame = new GameObject("HighlightFrame");
            var sr = highlightFrame.AddComponent<SpriteRenderer>();
            sr.sprite = highlightSprite;
            sr.sortingOrder = on_in_layer;
            highlightFrame.SetActive(false);
        }
        // 初期はゲージを非表示
        if (progressGroup != null) progressGroup.alpha = 0;
        if (progressBar != null)
        {
            progressBar.fillAmount = 0f;
            progressBar.gameObject.SetActive(false); //ゲージを完全非表示
        }
    }
    void FixedUpdate()
    {
        //WaveManagerから操作制限を確認
        if (!WaveManager.PlayerCanControl)
            return;

        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        // 移動ベクトル作成＆正規化
        Vector2 move = new Vector2(x, y).normalized;

        // 速度をかけて移動
        rb.MovePosition(rb.position + move * speed * Time.fixedDeltaTime);
    }
    void Update()
    {
        //WaveManagerから操作制限を確認
        if (!WaveManager.PlayerCanControl)
            return;

        HandleHoldProgress();//長押し処理を分離してわかりやすく
    }

    private void HandleHoldProgress()
    {
        //スペース長押し時間を計測
        if (Input.GetKey(KeyCode.Space))
        {
            // 押し始めた瞬間にゲージを表示
            if (!isHolding)
            {
                isHolding = true;
                holdTimer = 0f;

                if (progressBar != null)
                {
                    progressBar.fillAmount = 0f;
                    progressBar.gameObject.SetActive(true); // ← 表示
                }
                if (progressGroup != null) progressGroup.alpha = 1;
            }

            holdTimer += Time.deltaTime;

            // ゲージ更新
            if (progressBar != null)
                progressBar.fillAmount = Mathf.Clamp01(holdTimer / holdInterval);

            // 満タンになったら実行
            if (holdTimer >= holdInterval)
            {
                TryInteract();
                holdTimer = 0f;
                if (progressBar != null) progressBar.fillAmount = 0f;
                progressBar.gameObject.SetActive(false);
                if (progressGroup != null) progressGroup.alpha = 0;
                isHolding = false;
            }
        }
        else
        {
            // キーを離したらゲージ非表示＆リセット
            if (isHolding)
            {
                isHolding = false;
                holdTimer = 0f;

                if (progressBar != null)
                {
                    progressBar.fillAmount = 0f;
                    progressBar.gameObject.SetActive(false); //非表示
                }
                if (progressGroup != null) progressGroup.alpha = 0;
            }
        }
    }
    private void TryInteract()
    {
        if (currentTarget != null)
        {
            if (currentTarget == null) return;
            SpriteRenderer sr = currentTarget.GetComponent<SpriteRenderer>();
            if (sr == null) return;

            if (currentTarget.CompareTag("Plow"))
            {
                Debug.Log("畑を耕す");
                sr.sprite = plowedSprite;
                currentTarget.tag = "Plowed";
                GainExp(5);
            }
            else if (currentTarget.CompareTag("Plowed"))
            {
                Debug.Log("畑に水やりをした");
                sr.sprite = wateredSprite;
                currentTarget.tag = "Moist_Plowe";
                GainExp(5);
            }
            else if (currentTarget.CompareTag("Moist_Plowe"))
            {
                Debug.Log("畑に種を植えた");
                sr.sprite = seedSprite;
                currentTarget.tag = "Seed";
                GainExp(10);

                // 成長処理を開始
                StartCoroutine(GrowPlant(currentTarget, sr));
            }
            else if (currentTarget.CompareTag("Grown"))
            {
                Debug.Log("作物を収穫した！");
                HarvestCrop(sr);
                GainExp(15);
            }
            //設備設置
            else if (currentTarget.CompareTag("Grassland")|| IsPlacedEquipment(currentTarget.tag))
            {
                //設備未選択の場合おけない
                if (selectedEquipmentIndex == -1)
                {
                    Debug.Log("設備が選択されていません");
                    return;
                }

                EquipmentData selected = equipments[selectedEquipmentIndex];

                // 設備が既に置かれているかチェック
                PlacedEquipment placed = currentTarget.GetComponent<PlacedEquipment>();

                // --- まだ設置されていない場合 ---
                if (placed == null)
                {
                    if (currentScore >= selected.levels[0].cost)
                    {

                        sr.sprite = selected.levels[0].sprite;
                        currentTarget.tag = selected.name;

                        // 設置データを追加
                        PlacedEquipment eq = currentTarget.gameObject.AddComponent<PlacedEquipment>();
                        eq.data = selected;
                        eq.level = 0;

                        // コスト支払い
                        currentScore -= selected.levels[0].cost;
                        UpdateScoreUI();
                        GainExp(20);
                        Debug.Log($"{selected.name} を設置しました（Lv1）");
                    }
                    else
                    {
                        Debug.Log("スコアが足りません！設置できません。");
                    }
                }
                // --- 既に設置されている場合：強化 ---
                else
                {
                    bool upgraded = placed.TryUpgrade(ref currentScore);
                    if (upgraded) UpdateScoreUI();
                }
            }
        }
    }
   
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Plow") || other.CompareTag("Plowed") ||
        other.CompareTag("Moist_Plowe") || other.CompareTag("Seed") ||
        other.CompareTag("Seed_")||other.CompareTag("Wall")||
        other.CompareTag("Grown") || other.CompareTag("Grassland"))
        {
            currentTarget = other;
            if (highlightFrame != null)
            {
                highlightFrame.SetActive(true);
                highlightFrame.transform.position = other.transform.position;
                //枠のサイズを合わせる
                SpriteRenderer targetSR = other.GetComponent<SpriteRenderer>();
                if (targetSR != null)
                {
                    float sizeX = targetSR.bounds.size.x;
                    float sizeY = targetSR.bounds.size.y;
                    highlightFrame.transform.localScale = new Vector3(sizeX, sizeY, 1f);
                }
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (currentTarget == other)
        {
            currentTarget = null;

            //ハイライトを消す
            if(highlightFrame!=null)
            {
                highlightFrame.SetActive(false);
            }
        }
    }

    // 一定時間後に植物を成長させる
    private IEnumerator GrowPlant(Collider2D target, SpriteRenderer sr)
    {
        float timer = 0f;

        while (timer < growTime)
        {
            //ウェーブ中のみ時間を進める
            if (WaveManager.CanGrow)
            {
                timer += Time.deltaTime;
            }

            // タグが変わっていたら中断（他の状態になったら終了）
            if (target == null || !target.CompareTag("Seed"))
                yield break;

            yield return null;
        }

        //中間段階
        if (target != null && target.CompareTag("Seed"))
        {
            Debug.Log("植物が少し成長");
            sr.sprite = seed_Sprite;
            target.tag = "Seed_";
        }

        timer = 0f;

        while (timer < fullgrowTime)
        {
            //ウェーブ中のみ時間を進める
            if (WaveManager.CanGrow)
            {
                timer += Time.deltaTime;
            }

            // タグが変わっていたら中断（他の状態になったら終了）
            if (target == null || !target.CompareTag("Seed_"))
                yield break;

            yield return null;
        }
        // 成長完了！
        if (target != null && target.CompareTag("Seed_"))
        {
            Debug.Log("植物が成長しました！");
            sr.sprite = grownSprite;
            target.tag = "Grown";
        }
    }
    private void HarvestCrop(SpriteRenderer sr)
    {
        // スコア加算
        currentScore += harvestPoints;
        UpdateScoreUI();

        // 畑をリセット（再び耕せる状態に戻す）
        sr.sprite = plowSprite;
        currentTarget.tag = "Plow";
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore}";
        }
        else
        {
            Debug.LogWarning("ScoreText（UI）が設定されていません！");
        }
    }

    //UIボタンから呼び出す
    public void SelectEquipment(int index)
    {
        if (index >= 0 && index < equipments.Length)
        {
            selectedEquipmentIndex = index;
            Debug.Log($"設備を選択: {equipments[index].name}");
        }
    }

    //選択解除
    public void DeselectEquipment()
    {
        selectedEquipmentIndex = -1;
        Debug.Log("設備選択を解除しました");
    }

    private bool IsPlacedEquipment(string tag)
    {
        foreach (var eq in equipments)
        {
            if (eq.name == tag) return true;
        }
        return false;
    }

    private void UpdateLevelUI()
    {
        if (levelText != null)
            levelText.text = $"Lv {playerLevel}  EXP: {currentExp}/{expToNext}";
    }
    //経験値システム
    private void GainExp(int amount)
    {
        currentExp += amount;
        Debug.Log($"経験値 +{amount}（現在 {currentExp}/{expToNext}）");
        if (currentExp >= expToNext)
        {
            LevelUp();
        }
        UpdateLevelUI();
    }

    public int PlayerLevel => playerLevel;
    private void LevelUp()
    {
        playerLevel++;
        currentExp -= expToNext;
        expToNext = Mathf.RoundToInt(expToNext * 1.2f); // 必要経験値増加
        speed += speedGrowthRate;
        holdInterval = Mathf.Max(0.3f, holdInterval - holdReductionRate); // 下限0.3秒
        Debug.Log($"レベルアップ！ Lv{playerLevel} | 速度:{speed} | 長押し時間:{holdInterval}");
        UpdateLevelUI();
    }
    public void AddScore(int amount)
    {
        currentScore += amount;
        UpdateScoreUI();
        Debug.Log($"敵を倒した！スコア +{amount}（現在: {currentScore}）");
    }


}