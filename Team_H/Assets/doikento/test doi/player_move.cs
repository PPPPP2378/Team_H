using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class player_move : MonoBehaviour
{
    public float speed = 3f; //プレイヤーの移動速度
    private Rigidbody2D rb;
    private Collider2D currentTarget; // 今触れているオブジェクトを記録する
    private Vector2 movement;

    [Header("畑のスプライト差し替え用")]
    [SerializeField] private Sprite plowedSprite;   // 耕した後
    [SerializeField] private Sprite wateredSprite;  // 水やり後
    [SerializeField] private Sprite seedSprite;     // 種を植えた後
    [SerializeField] private Sprite grownSprite;    // 成長後の見た目（追加）
    [SerializeField] private Sprite plowSprite;     //耕す前

    [Header("成長にかかる時間(秒)")]
    [SerializeField] private float growTime = 5f;   // 種が育つまでの時間

    [Header("スコア設定")]
    [SerializeField] private int harvestPoints = 10;          // 1回収穫ごとのポイント
    [SerializeField] private TextMeshProUGUI scoreText;       // UI表示用
    private int currentScore = 100;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;

        UpdateScoreUI(); // 初期スコアを表示
    }
    void FixedUpdate()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        // 移動ベクトル作成＆正規化
        Vector2 move = new Vector2(x, y).normalized;

        // 速度をかけて移動
        rb.MovePosition(rb.position + move * speed * Time.fixedDeltaTime);
    }
    void Update()
    {

        //指定タグでスペースキーを押した時の処理
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (currentTarget != null)
            {
                SpriteRenderer sr = currentTarget.GetComponent<SpriteRenderer>();
                if (sr == null) return;

                if (currentTarget.CompareTag("Plow"))
                {
                    Debug.Log("畑を耕す");
                    sr.sprite = plowedSprite;
                    currentTarget.tag = "Plowed";
                }
                else if (currentTarget.CompareTag("Plowed"))
                {
                    Debug.Log("畑に水やりをした");
                    sr.sprite = wateredSprite;
                    currentTarget.tag = "Moist_Plowe";
                }
                else if (currentTarget.CompareTag("Moist_Plowe"))
                {
                    Debug.Log("畑に種を植えた");
                    sr.sprite = seedSprite;
                    currentTarget.tag = "Seed";

                    // 成長処理を開始
                    StartCoroutine(GrowPlant(currentTarget, sr));
                }
                else if (currentTarget.CompareTag("Grown"))
                {
                    Debug.Log("作物を収穫した！");
                    HarvestCrop(sr);
                }
            }
        }
    }
   
    void OnTriggerStay2D(Collider2D other)
    {
        currentTarget = other;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (currentTarget == other)
        {
            currentTarget = null;
        }
    }

    // 一定時間後に植物を成長させる
    private IEnumerator GrowPlant(Collider2D target, SpriteRenderer sr)
    {
        yield return new WaitForSeconds(growTime); // 成長時間待ち

        // タグがSeedのままなら成長（他の状態に変わっていないことを確認）
        if (target != null && target.CompareTag("Seed"))
        {
            Debug.Log("植物が成長しました！");
            sr.sprite = grownSprite;
            target.tag = "Grown"; // タグ変更
        }
    }
    private void HarvestCrop(SpriteRenderer sr)
    {
        // スコア加算
        currentScore += harvestPoints;
        UpdateScoreUI();

        // 畑をリセット（再び耕せる状態に戻す）
        sr.sprite = plowedSprite;
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
}