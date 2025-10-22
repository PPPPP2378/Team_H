using UnityEngine;

public class RabbitAI : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 2f;           // 移動速度
    private int direction = 1;             // 1=右, -1=左
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
        sr = GetComponent<SpriteRenderer>();
        currentHP = maxHP;
    }

    void FixedUpdate()
    {
        Patrol();
    }

    void Patrol()
    {
        // 足元の先に地面があるか判定
        Vector2 groundCheckPos = new Vector2(transform.position.x + direction * 0.4f, transform.position.y - 0.5f);
        bool isGroundAhead = Physics2D.Raycast(groundCheckPos, Vector2.down, groundCheckDistance, groundLayer);

        // 目の前に壁があるか判定
        Vector2 wallCheckPos = new Vector2(transform.position.x + direction * 0.5f, transform.position.y);
        bool isWallAhead = Physics2D.Raycast(wallCheckPos, Vector2.right * direction, 0.2f, groundLayer);

        // 壁 or 崖 で方向転換
        if (!isGroundAhead || isWallAhead)
        {
            direction *= -1;
            sr.flipX = direction < 0; // 見た目を反転
        }

        // 移動
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
    }

    // ダメージ受ける処理（あとで使う用）
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
        // 今は削除だけ（あとでアニメーション追加可）
        Destroy(gameObject);
    }

    // Sceneで視覚デバッグ用
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position + new Vector3(direction * 0.4f, -0.5f, 0),
                        transform.position + new Vector3(direction * 0.4f, -0.5f - groundCheckDistance, 0));
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + new Vector3(direction * 0.5f, 0, 0),
                        transform.position + new Vector3(direction * 0.7f, 0, 0));
    }
}