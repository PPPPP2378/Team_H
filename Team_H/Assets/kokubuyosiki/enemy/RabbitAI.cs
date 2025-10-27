

using UnityEngine;
using System.Collections; // 👈 コルーチンに必要

public class RabbitAI : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 2f;
    public float detectionRange = 5f;
    public LayerMask groundLayer;
    public LayerMask obstacleLayer;

    [Header("HP設定")]
    public int maxHP = 3;
    private int currentHP;

    [Header("ターゲット変換設定")]
    public Sprite plowedSoilSprite; // InspectorでPlow_soil_0のSpriteを設定

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
            // ⭐ 修正: rb.linearVelocity -> rb.velocity ⭐
            rb.linearVelocity = Vector2.zero;
        }
    }

    void FindTarget()
    {
        // ターゲットを検索するロジック (既存の通り)
        GameObject[] seeds = GameObject.FindGameObjectsWithTag("Seed");
        GameObject[] wheats = GameObject.FindGameObjectsWithTag("Grown");

        Transform closestTarget = null;
        float minDistance = detectionRange;

        GameObject[] allTargets = new GameObject[seeds.Length + wheats.Length];
        seeds.CopyTo(allTargets, 0);
        wheats.CopyTo(allTargets, seeds.Length);

        foreach (GameObject target in allTargets)
        {
            // ターゲットが既に耕されているか確認する（オプション）
            SpriteRenderer targetSr = target.GetComponent<SpriteRenderer>();
            if (targetSr != null && targetSr.sprite == plowedSoilSprite)
            {
                continue;
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

        // --- 障害物チェックロジック（省略） ---

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

    // ----------------------------------------------------------------
    // ⭐ NEW: 畑のグラフィック変更を2秒後に実行するロジック ⭐
    // ----------------------------------------------------------------
    void OnTriggerEnter2D(Collider2D other)
    {
        string tag = other.gameObject.tag;

        // ターゲットに接触した場合
        if (tag == "Seed" || tag == "Grown")
        {
            // ターゲットオブジェクトをコルーチンに渡す
            GameObject tileObject = other.gameObject;

            // 2秒後にスプライトを変更するコルーチンを開始
            // duration: 2f
            StartCoroutine(ChangeTileSpriteOverTime(tileObject, plowedSoilSprite, 2f));

            // ⭐ オプション: ターゲットを即座に無効にし、追跡をやめる
            // other.enabled = false;
            targetTransform = null;
        }
    }

    IEnumerator ChangeTileSpriteOverTime(GameObject tileObject, Sprite targetSprite, float duration)
    {
        // 指定された時間（2秒）待機する
        yield return new WaitForSeconds(duration);

        // 待機後、オブジェクトがまだ存在するか確認する
        if (tileObject != null)
        {
            SpriteRenderer sr = tileObject.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                // 2秒後にスプライトを最終形（Plowed Soil Sprite）に設定する
                sr.sprite = targetSprite;
            }
        }
    }

    // ... (OnDrawGizmosSelected() は省略)
}