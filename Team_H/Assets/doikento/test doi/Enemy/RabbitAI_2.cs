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

    public int scoreValue = 50; // 倒したときのスコア値

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        currentHP = maxHP;
        FindTarget();
    }

    void FixedUpdate()
    {
        if (targetTransform == null)
        {
            FindTarget();
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // ターゲットが削除済みまたは荒らされている場合は再探索
        SpriteRenderer targetSr = targetTransform.GetComponent<SpriteRenderer>();
        if (targetSr == null || targetSr.sprite == plowedSoilSprite)
        {
            targetTransform = null;
            FindTarget();
            return;
        }

        MoveTowardsTarget(); // ← 変更点
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
        RaycastHit2D hit = Physics2D.Raycast(currentPos, direction, 0.5f, obstacleLayer);

        if (hit.collider == null)
        {
            // 壁がない → 移動
            Vector2 newPos = currentPos + direction * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPos);
        }
        else
        {
            // 壁がある → 少し横にずらす
            Vector2 avoid = new Vector2(direction.y, -direction.x);
            Vector2 newPos = currentPos + avoid * moveSpeed * 0.5f * Time.fixedDeltaTime;
            rb.MovePosition(newPos);
        }

        // 向き反転
        if (sr != null)
        {
            if (direction.x > 0.05f) sr.flipX = false;
            else if (direction.x < -0.05f) sr.flipX = true;
        }
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

