using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RabbitAI_Complete : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 2f;
    public float detectionRange = 8f;
    public LayerMask obstacleLayer;

    [Header("HP設定")]
    public int maxHP = 100;
    private int currentHP;

    [Header("ターゲット変換設定")]
    public Sprite plowedSoilSprite;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Transform targetTransform;

    [Header("回避設定")]
    public float obstacleCheckDistance = 0.5f;

    public int scoreValue = 50; // 倒したときのスコア値

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        currentHP = maxHP;
        FindTarget();
    }

    private Vector2 lastPosition;
    private float stuckTimer = 0f;
    void FixedUpdate()
    {
        if (Vector2.Distance(rb.position, lastPosition) < 0.01f)
            stuckTimer += Time.fixedDeltaTime;
        else
            stuckTimer = 0f;

        if (stuckTimer > 2f)
        {
            targetTransform = null;
            stuckTimer = 0f;
        }

        lastPosition = rb.position;

        if (targetTransform == null)
        {
            rb.linearVelocity = Vector2.zero;
            FindTarget();
            return;
        }

        MoveTowardsTarget();
    }

    // -------------------------------
    // ターゲット探索
    // -------------------------------
    void FindTarget()
    {
        GameObject[] seeds = GameObject.FindGameObjectsWithTag("Seed");
        GameObject[] wheats = GameObject.FindGameObjectsWithTag("Grown");

        List<GameObject> allTargets = new List<GameObject>();
        allTargets.AddRange(seeds);
        allTargets.AddRange(wheats);

        Transform closest = null;
        float minDistance = detectionRange;

        foreach (GameObject obj in allTargets)
        {
            if (obj == null) continue;

            SpriteRenderer s = obj.GetComponent<SpriteRenderer>();
            if (s != null && s.sprite == plowedSoilSprite)
                continue;

            float distance = Vector2.Distance(transform.position, obj.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = obj.transform;
            }
        }

        targetTransform = closest;
    }

    // -------------------------------
    // 移動処理（壁を避ける）
    // -------------------------------
    void MoveTowardsTarget()
    {
        if (targetTransform == null) return;

        Vector2 currentPos = rb.position;
        Vector2 targetPos = targetTransform.position;
        Vector2 direction = (targetPos - currentPos).normalized;

        // Raycastで前方の壁をチェック
        RaycastHit2D fronHit = Physics2D.Raycast(currentPos, direction, obstacleCheckDistance, obstacleLayer);

        if(fronHit.collider!=null)
        {
            // 壁に当たったら壁の法線方向を使ってスライド
            Vector2 wallNormal = fronHit.normal;
            // 法線に垂直な方向へ滑る（壁沿いに動く）
            direction = Vector2.Perpendicular(wallNormal) * Mathf.Sign(Vector2.Dot(direction, Vector2.Perpendicular(wallNormal)));
        }
        rb.linearVelocity = direction * moveSpeed;

        if (Mathf.Abs(direction.x) > 0.1)
            sr.flipX = direction.x < 0;
    }

    // -------------------------------
    // 畑やトラップの当たり判定
    // -------------------------------
    void OnTriggerEnter2D(Collider2D other)
    {
        string tag = other.gameObject.tag;

        if (tag == "Seed" || tag == "Grown")
        {
            GameObject tile = other.gameObject;
            if (tile == null) return;

            SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != plowedSoilSprite)
            {
                StartCoroutine(ChangeTileSpriteOverTime(tile, plowedSoilSprite, 1.5f));
                StartCoroutine(DisappearAfter(1.6f));
            }

            targetTransform = null;
        }

    }

    // -------------------------------
    // ダメージ処理
    // -------------------------------
    public void TakeDamage(int dmg)
    {
        currentHP -= dmg;
        if (currentHP <= 0)
        {
            // プレイヤーにスコア加算
            player_move player = FindAnyObjectByType<player_move>();
            if (player != null)
            {
                player.AddScore(scoreValue);
            }
            Destroy(gameObject);
        }


    }

    // -------------------------------
    // スプライト変更コルーチン
    // -------------------------------
    IEnumerator ChangeTileSpriteOverTime(GameObject tile, Sprite targetSprite, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (tile != null)
        {
            SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.sprite = targetSprite;
        }
    }

    IEnumerator DisappearAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (gameObject != null)
            Destroy(gameObject);
    }

    public static void RemoveAllRabbits()
    {
        GameObject[] rabbits = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject rabbit in rabbits)
        {
            Destroy(rabbit);
        }
    }
}

