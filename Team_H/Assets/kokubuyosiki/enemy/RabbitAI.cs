using UnityEngine;

public class RabbitAI : MonoBehaviour
{
    // ... (既存のpublic, private変数は変更なし) ...
    [Header("移動設定")]
    public float moveSpeed = 2f;
    public float detectionRange = 5f;
    public LayerMask groundLayer;
    public LayerMask obstacleLayer;

    [Header("HP設定")]
    public int maxHP = 3;
    private int currentHP;

    [Header("ターゲット変換設定")]
    // ⭐ Inspectorで設定するPlow_soil_0のSprite
    public Sprite plowedSoilSprite;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Transform targetTransform;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>(); // SpriteRendererがあることを画像で確認
        currentHP = maxHP;
    }

    void FixedUpdate()
    {
        FindTarget();

        if (targetTransform != null)
        {
            ChaseTarget();
        }
        else
        {
            // ⭐ 修正: rb.linearVelocity -> rb.velocity ⭐
            rb.linearVelocity = Vector2.zero;
        }
    }

    void FindTarget()
    {
        // ... (FindTarget関数は以前のコードと変更なし) ...
        GameObject[] seeds = GameObject.FindGameObjectsWithTag("Seed");
        GameObject[] wheats = GameObject.FindGameObjectsWithTag("Grown");

        Transform closestTarget = null;
        float minDistance = detectionRange;

        GameObject[] allTargets = new GameObject[seeds.Length + wheats.Length];
        seeds.CopyTo(allTargets, 0);
        wheats.CopyTo(allTargets, seeds.Length);

        foreach (GameObject target in allTargets)
        {
            // ターゲットが既に耕されているか確認 (オプション)
            SpriteRenderer targetSr = target.GetComponent<SpriteRenderer>();
            if (targetSr != null && targetSr.sprite == plowedSoilSprite)
            {
                continue; // 耕されていたら無視
            }

            float distance = Vector2.Distance(transform.position, target.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestTarget = target.transform;
            }
        }
        targetTransform = closestTarget;
    }

    void ChaseTarget()
    {
        Vector2 targetPosition = targetTransform.position;
        Vector2 currentPosition = transform.position;
        Vector2 directionVector = (targetPosition - currentPosition).normalized;

        // --- 障害物チェック ---
        RaycastHit2D hit = Physics2D.Raycast(currentPosition, directionVector, moveSpeed * Time.fixedDeltaTime * 1.5f, obstacleLayer);
        if (hit.collider != null)
        {
            // 障害物回避ロジック
            Vector2 avoidDir = Vector2.Perpendicular(directionVector) * (Random.value > 0.5f ? 1 : -1);
            directionVector = (avoidDir).normalized;
        }

        // ⭐ 修正: rb.linearVelocity -> rb.velocity ⭐
        rb.linearVelocity = directionVector * moveSpeed;

        if (sr != null)
        {
            float xDirection = directionVector.x;
            if (Mathf.Abs(xDirection) > 0.05f)
            {
                sr.flipX = xDirection < 0;
            }
        }
    }

    // ⭐ NEW: 接触検出ロジック (画像変更) ⭐
    void OnTriggerEnter2D(Collider2D other)
    {
        string tag = other.gameObject.tag;

        if (tag == "Seed" || tag == "Grown")
        {
            SpriteRenderer targetSr = other.GetComponent<SpriteRenderer>();

            if (targetSr != null && plowedSoilSprite != null)
            {
                // 画像をPlow_soil_0のスプライトに切り替える
                targetSr.sprite = plowedSoilSprite;

                // ターゲットを追跡中の場合、次のターゲットを探し始める
                targetTransform = null;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // ... (デバッグ用Gizmosは省略) ...
        if (targetTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, targetTransform.position);
        }
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}