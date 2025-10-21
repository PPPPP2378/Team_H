using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player_move : MonoBehaviour
{
    public float speed = 0.01f;//プレイヤーの移動速度
    private Rigidbody2D rb;
    private Collider2D currentTarget; // 今触れているオブジェクトを記録する
    private bool canmove = true;

    [Header("畑のスプライト差し替え用")]
    [SerializeField] private Sprite plowedSprite;  // 耕した後の見た目
    [SerializeField] private Sprite wateredSprite; // 水やり後の見た目
    [SerializeField] private Sprite seedSprite;　　//種を植えた後の見た目
    [SerializeField] private Sprite grownSprite;   //成長後
    [SerializeField] private Sprite plowSprite;    //収穫後の見た目
    [SerializeField] private Sprite dischargeSprite;     //放電罠設置後

    [Header("成長にかかる時間(秒)")]
    [SerializeField] private float growTime = 5f;   // 種が育つまでの時間

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Rigidbody2D の設定
        rb.gravityScale = 0;  // 2Dの上方向重力が不要ならオフ
        rb.freezeRotation = true; // 回転を固定（衝突時に回転しない）
    }

    // Update is called once per frame
    void Update()
    {
        //移動処理
        Vector2 position = transform.position;

        if (Input.GetKey("a"))
        {
            position.x -= speed;
        }
        else if (Input.GetKey("d"))
        {
            position.x += speed;
        }
        else if (Input.GetKey("w"))
        {
            position.y += speed;
        }
        else if (Input.GetKey("s"))
        {
            position.y -= speed;
        }

        transform.position = position;

        
    }
    void DoActon()
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
                    sr.sprite = plowSprite;//グラフィック変更
                    currentTarget.tag = "Plow";//タグも変更
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

    void OnTriggerStay2D(Collider2D other)
    {
        Debug.Log("接触中のタグ: " + other.tag);
        currentTarget = other; // 現在触れているオブジェクトを記録
    }

    // 離れたときに解除
    void OnTriggerExit2D(Collider2D other)
    {
        if (currentTarget == other)
        {
            currentTarget = null;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject .CompareTag("Wall"))
        {
            Debug.Log("障害物に接触");
            canmove = false;
            rb.linearVelocity = Vector2.zero;
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("障害物から離れました");
            canmove = true;
        }
    }


}

