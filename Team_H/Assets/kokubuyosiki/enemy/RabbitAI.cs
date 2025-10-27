using UnityEngine;

public class RabbitAI : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 2f;
    public float detectionRange = 5f;
    public LayerMask groundLayer;
    public LayerMask obstacleLayer; // ← wood用（Inspectorで指定）

    [Header("HP設定")]
    public int maxHP = 3;
    private int currentHP;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Transform targetTransform;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
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
            rb.linearVelocity = Vector2.zero;
        }
    }

    void FindTarget()
    {
        GameObject[] seeds = GameObject.FindGameObjectsWithTag("Seed");
        GameObject[] wheats = GameObject.FindGameObjectsWithTag("Grown");

        Transform closestTarget = null;
        float minDistance = detectionRange;

        GameObject[] allTargets = new GameObject[seeds.Length + wheats.Length];
        seeds.CopyTo(allTargets, 0);
        wheats.CopyTo(allTargets, seeds.Length);

        foreach (GameObject target in allTargets)
        {
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

        // --- ★ 進行方向にRayを飛ばして障害物をチェック ---
        RaycastHit2D hit = Physics2D.Raycast(currentPosition, directionVector, 5f, obstacleLayer);
        if (hit.collider != null)
        {
            // 木などにぶつかりそうなら、少し横に避ける方向に変更
            Vector2 avoidDir = Vector2.Perpendicular(directionVector) * (Random.value > 0.5f ? 1 : -1);
            directionVector = (directionVector + avoidDir * 0.5f).normalized;
        }

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

    void OnDrawGizmosSelected()
    {
        // デバッグ用にRayを可視化
        if (targetTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)(targetTransform.position - transform.position).normalized);
        }
    }
}