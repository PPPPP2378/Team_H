using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * カラスAI（群れ行動対応）
 * ・壁を無視して畑に直線的に飛ぶ
 * ・他のカラスと少し離れて群れ行動する
 * ・倒されるとスコア加算
 */
public class CrowFlockAI : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 3f;                // 基本速度
    public float detectionRange = 10f;          // 畑を探す範囲
    public float separationDistance = 1.0f;     // 群れの中で他のカラスと離れる距離
    public float cohesionStrength = 0.3f;       // 群れのまとまり具合（0.2〜0.5推奨）
    public float alignmentStrength = 0.3f;      // 向きの揃え具合（0.2〜0.5推奨）

    [Header("HP設定")]
    public int maxHP = 80;
    private int currentHP;

    [Header("ターゲット関連")]
    public Sprite plowedSoilSprite;
    public string destroyedFieldTag = "Rough";
    private Transform finalTarget;

    [Header("スコア設定")]
    public int scoreValue = 100;

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private static List<CrowFlockAI> allCrows = new List<CrowFlockAI>(); // 群れ全体の参照
    private float targetCheckTimer = 0f;

    void OnEnable()
    {
        allCrows.Add(this);
    }

    void OnDisable()
    {
        allCrows.Remove(this);
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        currentHP = maxHP;

        FindFinalTarget();
    }

    void FixedUpdate()
    {
        // ターゲットを定期的に探す
        targetCheckTimer += Time.fixedDeltaTime;
        if (targetCheckTimer >= 1.0f)
        {
            FindFinalTarget();
            targetCheckTimer = 0f;
        }

        if (finalTarget == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // 群れ行動ベクトルを計算
        Vector2 moveDir = CalculateFlockDirection();

        // ターゲット方向を強めに加える
        Vector2 toTarget = ((Vector2)finalTarget.position - rb.position).normalized;
        Vector2 finalDir = (moveDir * 0.5f + toTarget).normalized;

        // 実際の移動
        rb.linearVelocity = finalDir * moveSpeed;

        // 向き調整
        if (Mathf.Abs(finalDir.x) > 0.1)
            sr.flipX = finalDir.x < 0;

        // ターゲット到達時
        float distance = Vector2.Distance(transform.position, finalTarget.position);
        if (distance < 0.5f)
        {
            StartCoroutine(DisappearAfter(0.8f));
        }
    }

    // -------------------------------
    // 群れ行動の方向ベクトルを計算
    // -------------------------------
    Vector2 CalculateFlockDirection()
    {
        Vector2 separation = Vector2.zero; // 他個体と距離を取る
        Vector2 alignment = Vector2.zero;  // 向きを揃える
        Vector2 cohesion = Vector2.zero;   // 群れの中心に寄る

        int neighborCount = 0;

        foreach (CrowFlockAI other in allCrows)
        {
            if (other == this) continue;

            float dist = Vector2.Distance(transform.position, other.transform.position);
            if (dist < separationDistance * 3) // 群れとして認識する範囲
            {
                neighborCount++;
                // 離れる力
                if (dist < separationDistance)
                    separation += (Vector2)(transform.position - other.transform.position).normalized / dist;

                // 向きを揃える
                alignment += other.rb.linearVelocity;

                // 群れの中心を取る
                cohesion += (Vector2)other.transform.position;
            }
        }

        if (neighborCount > 0)
        {
            separation /= neighborCount;
            alignment = (alignment / neighborCount).normalized;
            cohesion = ((cohesion / neighborCount) - (Vector2)transform.position).normalized;
        }

        return (separation + alignment * alignmentStrength + cohesion * cohesionStrength).normalized;
    }

    // -------------------------------
    // ターゲット探索（ウサギ共通）
    // -------------------------------
    void FindFinalTarget()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Seed");
        GameObject[] wheats = GameObject.FindGameObjectsWithTag("Grown");
        GameObject[] plows = GameObject.FindGameObjectsWithTag("Plow");
        GameObject[] plowed = GameObject.FindGameObjectsWithTag("Plowed");
        GameObject[] moist = GameObject.FindGameObjectsWithTag("Moist_Plowe");

        List<GameObject> allTargets = new List<GameObject>();
        allTargets.AddRange(targets);
        allTargets.AddRange(wheats);
        allTargets.AddRange(plows);
        allTargets.AddRange(plowed);
        allTargets.AddRange(moist);

        Transform closest = null;
        float minDistance = detectionRange;

        foreach (GameObject obj in allTargets)
        {
            if (obj == null) continue;
            SpriteRenderer s = obj.GetComponent<SpriteRenderer>();
            if (s != null && s.sprite == plowedSoilSprite)
                continue;

            float dist = Vector2.Distance(transform.position, obj.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = obj.transform;
            }
        }

        if (closest != null)
            finalTarget = closest;
    }

    // -------------------------------
    // 畑を荒らす
    // -------------------------------
    void OnTriggerEnter2D(Collider2D other)
    {
        string tag = other.gameObject.tag;
        if (tag == "Seed" || tag == "Grown" || tag == "Plow" || tag == "Plowed" || tag == "Moist_Plowe")
        {
            GameObject tile = other.gameObject;
            if (tile == null) return;

            SpriteRenderer tileRendere = tile.GetComponent<SpriteRenderer>();
            if (tileRendere != null && tileRendere.sprite != plowedSoilSprite)
            {
                StartCoroutine(ChangeTileSpriteOverTime(tile, plowedSoilSprite, 0.8f));
                StartCoroutine(DisappearAfter(1.0f));
            }
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
            player_move player = FindAnyObjectByType<player_move>();
            if (player != null)
            {
                player.AddScore(scoreValue);
            }
            Destroy(gameObject);
        }
    }

    // -------------------------------
    // スプライト変更
    // -------------------------------
    IEnumerator ChangeTileSpriteOverTime(GameObject tile, Sprite targetSprite, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (tile != null)
        {
            SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.sprite = targetSprite;

            if (!string.IsNullOrEmpty(destroyedFieldTag))
                tile.tag = destroyedFieldTag;
        }
    }

    IEnumerator DisappearAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (gameObject != null)
            Destroy(gameObject);
    }
}
