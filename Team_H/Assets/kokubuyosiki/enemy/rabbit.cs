using UnityEngine;

public class RabbitAI : MonoBehaviour
{
    [Header("�ړ��ݒ�")]
    public float moveSpeed = 2f;           // �ړ����x
    private int direction = 1;             // 1=�E, -1=��
    public float groundCheckDistance = 0.5f;
    public LayerMask groundLayer;

    [Header("HP�ݒ�")]
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
        // �����E�O���̒n�ʂ�ǂ����m
        Vector2 front = new Vector2(transform.position.x + direction * 0.5f, transform.position.y);
        Vector2 down = new Vector2(transform.position.x + direction * 0.5f, transform.position.y - 1f);

        bool wallAhead = Physics2D.Raycast(front, Vector2.right * direction, 0.2f, groundLayer);
        bool groundAhead = Physics2D.Raycast(down, Vector2.down, groundCheckDistance, groundLayer);

        // �� or �R�ɗ����甽�]
        if (wallAhead || !groundAhead)
        {
            direction *= -1;
            sr.flipX = !sr.flipX;
        }

        // �ړ�����
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // TODO: �G�t�F�N�g��X�R�A���Z��ǉ����Ă�OK
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // �f�o�b�O�p Ray ����
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + Vector3.right * direction * 0.5f,
                        transform.position + Vector3.right * direction * 0.7f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position + new Vector3(direction * 0.5f, 0, 0),
                        transform.position + new Vector3(direction * 0.5f, -groundCheckDistance, 0));
    }
}
