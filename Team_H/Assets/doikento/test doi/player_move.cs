using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class player_move : MonoBehaviour
{
    [Header ("プレイヤー設定")]
    [SerializeField] public float speed = 5f;//プレイヤーの移動速度
    private Rigidbody2D rb;
    private Collider2D currentTarget; // 今触れているオブジェクトを記録する

    [Header("畑のスプライト差し替え用")]
    [SerializeField] private Sprite plowedSprite;  // 耕した後の見た目
    [SerializeField] private Sprite wateredSprite; // 水やり後の見た目
    [SerializeField] private Sprite seedSprite;　　//種を植えた後の見た目
    [SerializeField] private Sprite grownSprite;   //成長後
    [SerializeField] private Sprite plowSprite;    //収穫後の見た目
    [SerializeField] private Sprite dischargeSprite;     //放電罠設置後

    [Header("成長にかかる時間(秒)")]
    [SerializeField] private float growTime = 5f;   // 種が育つまでの時間

    [Header("ポイント設定")]
    [SerializeField] private int harvestPoints = 10;　　　//1収穫のポイント
    [SerializeField] private TextMeshProUGUI pointText;　//UI表示
    private int currentPoint = 100;                        //初期ポイント
    private bool isHolding = false;

    private void Awake()
    {
        updateScoreUI();//初期スコア表示
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Rigidbody2D の設定
        rb.gravityScale = 0;  // 2Dの上方向重力が不要ならオフ
        rb.freezeRotation = true; // 回転を固定（衝突時に回転しない）
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

    // Update is called once per frame
    void Update()
    {

        //指定タグでスペースキーを押した時の処理
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (currentTarget != null)
            {
                SpriteRenderer sr = currentTarget.GetComponent<SpriteRenderer>();
                if (sr == null) return; // SpriteRendererがない場合は何もしない

                if (currentTarget.CompareTag("Plow"))
                {
                    Debug.Log("畑を耕す");
                    sr.sprite = plowedSprite; // グラフィック変更
                    currentTarget.tag = "Plowed"; // タグも変更
                }
                else if (currentTarget.CompareTag("Plowed"))
                {
                    Debug.Log("畑に水やりをした");
                    sr.sprite = wateredSprite; // グラフィック変更
                    currentTarget.tag = "Moist_Plowe"; // タグも変更
                }
                else if (currentTarget.CompareTag("Moist_Plowe"))
                {
                    Debug.Log("畑に種を植えた");
                    sr.sprite = seedSprite; // グラフィック変更
                    currentTarget.tag = "Seed"; // タグも変更

                    //StartCoroutineで成長開始
                    StartCoroutine(GrowPlant(currentTarget, sr));
                }
                else if (currentTarget.CompareTag("Grown"))
                {
                    Debug.Log("作物を収穫");
                    HarvestCrop(sr);
                }
                else if (currentTarget.CompareTag("Grassland"))
                {
                    Debug.Log("罠設置");
                    sr.sprite = dischargeSprite;//グラフィック変更
                    currentTarget.tag = "Discharge";//タグ変更
                }
            }
        }
    }

    //作物の成長処理
    private IEnumerator GrowPlant(Collider2D target, SpriteRenderer sr)
    {
        yield return new WaitForSeconds(growTime);//成長待ち

        if (target != null && target.CompareTag("Seed"))
        {
            Debug.Log("作物が成長");
            sr.sprite = grownSprite;//グラフィック変更
            target.tag = "Grown";//タグ変更
        }
    }
    private void HarvestCrop(SpriteRenderer sr)
    {
        //ポイント加算
        currentPoint += harvestPoints;
        updateScoreUI();

        sr.sprite = plowSprite;
        currentTarget.tag = "Plow";
    }
    private void updateScoreUI()
    {
        if(pointText!=null)
        {
            pointText.text = $"Point:{currentPoint}";
        }
        else
        {
            Debug.LogWarning("textUIがありません");
        }
    }
    void OnTriggerStay2D(Collider2D other)
    {
        Debug.Log("接触中のタグ: " + other.tag);
        currentTarget = other; // 現在触れているオブジェクトを記録
    }
}

