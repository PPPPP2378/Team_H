using UnityEngine;

public class RabbitAI : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 2f; 			// 移動速度
    private int direction = 1; 				// 1=右, -1=左
    public float groundCheckDistance = 0.5f;
    public LayerMask groundLayer;

    [Header("HP設定")]
    public int maxHP = 3;
    private int currentHP;

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // MissingComponentExceptionの解決に必須
        sr = GetComponent<SpriteRenderer>();
        currentHP = maxHP;
    }

    void FixedUpdate()
    {
        Patrol();
    }

    void Patrol()
    {
        // ⭐⭐ 修正推奨 ⭐⭐ Raycastの開始位置を0.3fから0.15fに変更
        float rayStartOffset = 0.15f;
        float wallRayLength = 0.1f;

        // ... (地面チェック)

        // 目の前に壁があるか判定
        Vector2 wallCheckPos = new Vector2(transform.position.x + direction * rayStartOffset, transform.position.y);
        bool isWallAhead = Physics2D.Raycast(wallCheckPos, Vector2.right * direction, wallRayLength, groundLayer);

        // ... (方向転換ロジック)

        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
    }

    public void TakeDamage(int dmg)
    {
        currentHP -= dmg;
        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }

    // Sceneで視覚デバッグ用 (調整後の値に合わせてGizmosも調整推奨)
    void OnDrawGizmosSelected()
    {
        // 崖チェックのRay (黄色)
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position + new Vector3(direction * 0.4f, -0.5f, 0),
                        transform.position + new Vector3(direction * 0.4f, -0.5f - groundCheckDistance, 0));

        // 壁チェックのRay (赤) - 調整後の値 (0.3fと0.1f) に合わせる
        float debugRayStartOffset = 0.3f;
        float debugRayLength = 0.1f;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + new Vector3(direction * debugRayStartOffset, 0, 0),
                        transform.position + new Vector3(direction * (debugRayStartOffset + debugRayLength), 0, 0));
    }
}