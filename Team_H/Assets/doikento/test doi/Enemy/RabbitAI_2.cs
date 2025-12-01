using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

/*ウサギの動きを決めるコード
 * 倒すとグラフィックが削除されポイントが加算される
 */
public class RabbitAI_Complete : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 2f;// 通常の移動速度
    public float detectionRange = 8f; // 畑を探す範囲
    public LayerMask WallLayer; // 壁レイヤー（

    [Header("HP設定")]
    public int maxHP = 100; //最大HP
    private int currentHP;  //現在のHP

    [Header("ターゲット変換設定")]
    public Sprite plowedSoilSprite; // 食べ終わった畑のスプライト
    [Header("荒らした後の設定")]
    public string destroyedFieldTag = "Rough"; // 荒らされた畑のタグ
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Transform targetTransform;

    [Header("回避設定")]
    public float obstacleCheckDistance = 2.0f;
    public float avoidStrength = 5f; //壁を避ける強さ（数値を上げると強く避ける）
    public float slideStrength = 2.0f;

    public int scoreValue = 50; // 倒したときのスコア値

    [Header("死亡時のグラフィック")]
    public Sprite deadSprite;     // 死亡した時のスプライト
    public float deathDisappearTime = 1.0f; // 消えるまでの時間
    private bool isDead = false;

    private List<Transform> waypoints = new List<Transform>(); // 経路上のチェックポイント
    private int currentWaypointIndex = 0;                      // 現在のチェックポイント番号
    private Transform finalTarget;

    public GameObject damageTextPrefab;

    [Header("方向ごとスプライト")]
    public Sprite spriteUp;
    public Sprite spriteDown;
    public Sprite spriteLeft;
    public Sprite spriteRight;

    [Header("効果音")]
    public AudioClip deathSE;      // 死亡時の効果音
    private AudioSource audioSource;
    public AudioClip damageSoilSE;   // 畑を荒らした時のSE
  

    [Header("移動経路（手動設定）")]
    public List<Transform> manualWaypoints = new List<Transform>();
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        currentHP = maxHP;

        // オーディオソースを取得／追加
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Waypointをすべて取得
        GameObject[] waypointObjects = GameObject.FindGameObjectsWithTag("Waypoint");
        foreach (GameObject wp in waypointObjects)
        {
            waypoints.Add(wp.transform);
        }

        // 近い順にソート（ウサギに近い順で進む）
        waypoints.Sort((a, b) =>
            Vector2.Distance(transform.position, a.position)
            .CompareTo(Vector2.Distance(transform.position, b.position))
        );

        // 最終目的地（畑）を探索
        FindFinalTarget();

        // 両方ないと動けない
        if (waypoints.Count == 0 && finalTarget == null)
        {
            Debug.LogWarning("RabbitAI: Waypoint も Target も見つかりませんでした。");
        }
    }

    private float targetCheckTimer = 0f;

    void FixedUpdate()
    {
        // 一定間隔で畑の再探索を行う
        targetCheckTimer += Time.fixedDeltaTime;
        if (targetCheckTimer >= 1.0f) // 1秒ごとに再探索
        {
            FindFinalTarget();
            targetCheckTimer = 0f;
        }

        // 現在の目的地（Waypoint or 畑）を取得/
        Transform currentTarget = GetCurrentTarget();

        // ターゲットがなければ停止
        if (currentTarget == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // ターゲット方向へ移動処理
        MoveTowards(currentTarget);

        // 目標に近づいたら次へ
        float distance = Vector2.Distance(transform.position, currentTarget.position);
        if (distance < 0.5f)
        {
            // Waypointを通過したら次のWaypointへ
            if (waypoints.Count > 0 && currentWaypointIndex < waypoints.Count)
            {
                currentWaypointIndex++;
            }
            // 最終地点（畑）に到達した場合
            else if (currentTarget == finalTarget)
            {
                // 最終地点到達
                StartCoroutine(DisappearAfter(0.95f));
            }
        }

    }

    // -------------------------------
    // 現在のターゲットを決定
    // -------------------------------
    Transform GetCurrentTarget()
    {
        // Waypointがまだ残っている場合はそちらを優先
        if (waypoints.Count > 0 && currentWaypointIndex < waypoints.Count)
            return waypoints[currentWaypointIndex];

        // Waypointをすべて通過 or 消去 → 最終ターゲットへ
        if (finalTarget != null)
            return finalTarget;

        // 念のため畑を再探索
        FindFinalTarget();
        return finalTarget;
    }

    // -------------------------------
    // 畑のターゲット探索
    // -------------------------------
    void FindFinalTarget()
    {
        //ターゲットタグを取得
        GameObject[] seeds = GameObject.FindGameObjectsWithTag("Seed");
        GameObject[] wheats = GameObject.FindGameObjectsWithTag("Grown");
        GameObject[] plow = GameObject.FindGameObjectsWithTag("Plow");
        GameObject[] plowed = GameObject.FindGameObjectsWithTag("Plowed");
        GameObject[] moist_plowe = GameObject.FindGameObjectsWithTag("Moist_Plowe");

        List<GameObject> allTargets = new List<GameObject>();
        allTargets.AddRange(seeds);
        allTargets.AddRange(wheats);
        allTargets.AddRange(plow);
        allTargets.AddRange(plowed);
        allTargets.AddRange(moist_plowe);

        Transform closest = null;
        float minDistance = detectionRange;

        // 最も近いターゲットを探す
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
        if (closest != null)
        {
            finalTarget = closest;// 最も近いターゲットを設定

            //畑を見つけたら Waypoint を全削除して直行モードへ
            if (waypoints.Count > 0)
            {
                Debug.Log("畑を検知！Waypoint経由を中断して直行します。");
                waypoints.Clear();
                currentWaypointIndex = 0;
            }
        }
    }
    // -------------------------------
    // 移動処理（壁を避ける）
    // -------------------------------
    void MoveTowards(Transform target)
    {
        if (target == null) return;

        Vector2 currentPos = rb.position;
        Vector2 targetPos = target.position;
        Vector2 direction = (targetPos - currentPos).normalized;

        // Raycastで壁を検出
        RaycastHit2D frontHit = Physics2D.Raycast(currentPos, direction, obstacleCheckDistance, WallLayer);
        if (frontHit.collider != null)
        {
            Vector2 wallNormal = frontHit.normal;

            // 壁を避ける
            direction += wallNormal * avoidStrength;

            // 壁に沿って滑る
            Vector2 slideDir = Vector2.Perpendicular(wallNormal);
            direction += slideDir * slideStrength;

            Debug.DrawRay(currentPos, direction * obstacleCheckDistance, Color.red);
        }

        direction = direction.normalized;
        rb.linearVelocity = direction * moveSpeed;

        UpdateSpriteByDirection(direction);

       
    }

    // -------------------------------
    // 畑やトラップの当たり判定
    // -------------------------------
    void OnTriggerEnter2D(Collider2D other)
    {
        string tag = other.gameObject.tag;

        if (tag == "Seed" || tag == "Grown"||tag=="Plow"||tag=="Plowed"||tag=="Moist_Plowe"||tag=="Seed_")
        {
            GameObject tile = other.gameObject;
            if (tile == null) return;

            SpriteRenderer tileRendere = tile.GetComponent<SpriteRenderer>();
            if (tileRendere != null && tileRendere.sprite != plowedSoilSprite)
            {
                Debug.Log("畑を荒らしました！");
               
                StartCoroutine(ChangeTileSpriteOverTime(tile, plowedSoilSprite, 1.0f));
                
                StartCoroutine(DisappearAfter(1.5f));
            }

          
        }

    }

    // -------------------------------
    // ダメージ処理
    // -------------------------------
    public void TakeDamage(int dmg)
    {
        currentHP -= dmg;

        ShowDamageText(dmg);

        if (currentHP <= 0)
        {
            // プレイヤーにスコア加算
            player_move player = FindAnyObjectByType<player_move>();
            if (player != null)
            {
                player.AddScore(scoreValue);
            }

            StartCoroutine(PlayDeathAnimation());
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
            SpriteRenderer sr = tile.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                Debug.Log("スプライトを変更します：" + sr.name);
                sr.sprite = targetSprite;
                if (audioSource != null && damageSoilSE != null)
                {
                    audioSource.PlayOneShot(damageSoilSE);
                }
            }
            else
            {
                Debug.LogWarning("SpriteRenderer が見つかりません: " + tile.name);
            }

            // --- タグを変更 ---
            if (!string.IsNullOrEmpty(destroyedFieldTag))
            {
                Debug.Log($"タグを変更します：{tile.tag} → {destroyedFieldTag}");
                tile.tag = destroyedFieldTag;
            }
        }
    }

    IEnumerator PlayDeathAnimation()
    {
        isDead = true;
        // 移動停止
        rb.linearVelocity = Vector2.zero;
        // 死亡SE再生（AudioClip が設定されている場合のみ）
        if (deathSE != null)
        {
            audioSource.PlayOneShot(deathSE);
        }

        rb.simulated = false; // 衝突判定オフ

        // スプライト切り替え
        if (sr != null && deadSprite != null)
        {
            sr.sprite = deadSprite;
        }

        // 少し待ってから削除
        yield return new WaitForSeconds(deathDisappearTime);

        Destroy(gameObject);
    }

    // 一定時間後にウサギを削除
    IEnumerator DisappearAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (gameObject != null)
            Destroy(gameObject);
    }

    // すべてのウサギを削除
    public static void RemoveAllRabbits()
    {
        GameObject[] rabbits = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject rabbit in rabbits)
        {
            Destroy(rabbit);
        }
    }

    void ShowDamageText(int dmg)
    {
        Debug.Log("DamageText を生成します");

        if (damageTextPrefab == null) return;

        // Canvas を取得（名前は Canvas であること）
        Canvas canvas = GameObject.Find("Canvas_2").GetComponent<Canvas>();
        if (canvas == null) return;

        // ワールド座標 → 画面座標へ変換する
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, 0.7f, 0));

        // UI を生成（Canvas の子）
        GameObject obj = Instantiate(damageTextPrefab, screenPosition, Quaternion.identity, canvas.transform);

        // テキスト内容を設定
        obj.GetComponent<DamageText>().SetText("-" + dmg);
    }

    void UpdateSpriteByDirection(Vector2 dir)
    {
        if (isDead) return;
        // 移動していないときは切り替えない
        if (dir.magnitude < 0.1f) return;

        // 横方向の方が大きい → 左右
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            if (dir.x > 0)
            {
                sr.sprite = spriteRight;
            }
            else
            {
                sr.sprite = spriteLeft;
            }
        }
        else
        {
            // 上下方向の方が大きい → 上下
            if (dir.y > 0)
            {
                sr.sprite = spriteUp;
            }
            else
            {
                sr.sprite = spriteDown;
            }
        }
    }
}

