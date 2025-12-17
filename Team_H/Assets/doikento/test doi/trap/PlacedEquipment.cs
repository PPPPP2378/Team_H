using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlacedEquipment;
using static UnityEngine.GraphicsBuffer;


[System.Serializable]
public class EquipmentLevelData
{
    public Sprite sprite; // グラフィック
    public int cost;      // 設置コスト
    public int damage;    // 敵に与えるダメージ量
    public float damageInterval = 0.5f;//ダメージインターバル
    public int requiredPlayerLevel;
    public GameObject hitEffect;

    // --- 遠距離用 ---
    public bool isRanged;            // 遠距離設備なら true
    public float attackRange = 3f;   // 射程
    public float projectileSpeed = 6f;
    public GameObject projectilePrefab; // 弾のPrefab

   
}

public class PlacedEquipment : MonoBehaviour
{

    [Header("設備データ")]
    public EquipmentData data; // 元の設備データ
    public int level = 0;      // 現在のレベル（0スタート）

    [Header("トラップ設定")]
    public float damageInterval = 0.5f;

    private SpriteRenderer sr;
    private Collider2D col;
    private Coroutine damageCoroutine;
    private float shootCooldown = 0f;

    private Dictionary<MonoBehaviour, Coroutine> damageCoroutines = new Dictionary<MonoBehaviour, Coroutine>();
    private GameObject currentEffectPrefab;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        // Collider がない場合は追加（トラップとして機能させるため）
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider2D>();
        }

        if (data.isWallEquipment)
        {
            col.isTrigger = false;
        }
        else
        {
            col.isTrigger = true;
        }

        ApplyLevelStats();

        // もし遠距離設備なら、専用コルーチンを開始
        if (data.levels[level].isRanged)
        {
            StartCoroutine(RangedAttackLoop());
        }
    }


    // レベルアップ処理
    public bool TryUpgrade(ref int currentScore)
    {

        if (level >= data.levels.Length - 1)
        {
            Debug.Log($"{data.name} は最大レベルです！");
            return false;
        }

        int required = data.levels[level + 1].requiredPlayerLevel;
       

        // レベル制限
        player_move player = FindAnyObjectByType<player_move>();
        if (player != null)
        {
            if (player.PlayerLevel < required)
            {
                Debug.Log($"プレイヤーレベルが足りません！ (必要Lv: {required})");
                return false;
            }
        }

        int nextCost = data.levels[level + 1].cost;
        if (currentScore < nextCost)
        {
            Debug.Log("スコアが足りません！");
            return false;
        }

        // コストを消費
        currentScore -= nextCost;
        level++;
        ApplyLevelStats();

        Debug.Log($"{data.name} をレベル {level + 1} に強化しました！");

        UIManager ui = FindAnyObjectByType<UIManager>();
        if (ui != null)
        {
            ui.ShowTooltip($"Lv {level + 1}", transform.position);
        }

        return true;
    }

    private void ApplyLevelStats()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        var stats = data.levels[level];
        sr.sprite = stats.sprite;

        // 攻撃力更新
        damage = stats.damage;

        //インターバル更新
        damageInterval = stats.damageInterval;

        // レベルごとのエフェクトを更新
        currentEffectPrefab = stats.hitEffect;
    }

    // --- DamageTrap の機能統合 ---
    private int damage = 0;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;
        {
            RabbitAI_Complete rabbit = other.GetComponent<RabbitAI_Complete>();
            CrowFlockAI crow = other.GetComponent<CrowFlockAI>();
            bear_Ai bear = other.GetComponent<bear_Ai>();

            MonoBehaviour target = rabbit != null ? (MonoBehaviour)rabbit : crow != null ? (MonoBehaviour)crow : bear != null ? (MonoBehaviour)bear : null;
            if (target != null&& !damageCoroutines.ContainsKey(target))
            {
                // ダメージを一度だけコルーチンで処理
                if (!damageCoroutines.ContainsKey(target))
                {
                    Coroutine c = StartCoroutine(DealDamageOverTime(target));
                    damageCoroutines.Add(target, c);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            RabbitAI_Complete rabbit = other.GetComponent<RabbitAI_Complete>();
            CrowFlockAI crow = other.GetComponent<CrowFlockAI>();
            bear_Ai bear = other.GetComponent<bear_Ai>();

            MonoBehaviour target = rabbit != null ? (MonoBehaviour)rabbit : crow != null ? (MonoBehaviour)crow : bear!=null?(MonoBehaviour)bear:null;
            if (target != null && damageCoroutines.ContainsKey(target))
            {
                StopCoroutine(damageCoroutines[target]);
                damageCoroutines.Remove(target);
            }
        }
    }

    private IEnumerator ApplyDamageOverTime(Collider2D target)
    {
        RabbitAI_Complete rabbit = target.GetComponent<RabbitAI_Complete>();
        if (rabbit == null) yield break;

        while (target != null && col.bounds.Intersects(target.bounds))
        {
            rabbit.TakeDamage(damage);
            Debug.Log($"[PlacedEquipment] {target.name} に {damage} ダメージを与えました！");
            yield return new WaitForSeconds(damageInterval);
        }
    }

    private IEnumerator DealDamageOverTime(MonoBehaviour target)
    {
        // 敵の Collider を取得
        Collider2D targetCol = target.GetComponent<Collider2D>();

        while (target != null && targetCol != null && col.bounds.Intersects(targetCol.bounds))
        {
           

            if (target is RabbitAI_Complete rabbit)
            {
                rabbit.TakeDamage(damage);
                Debug.Log($"[Trap] {gameObject.name} が {rabbit.name} に {damage} ダメージ！");
            }
            else if (target is CrowFlockAI crow)
            {
                crow.TakeDamage(damage);
                Debug.Log($"[Trap] {gameObject.name} が {crow.name} に {damage} ダメージ！");
            }
            else if (target is bear_Ai bear)
            {
                bear.TakeDamage(damage);
                Debug.Log($"[Trap] {gameObject.name} が {bear.name} に {damage} ダメージ！");
            }

            var effectPrefab = data.levels[level].hitEffect;
            if (effectPrefab != null)
            {
                // 「罠の中心」からエフェクトを出す
                GameObject effect = Instantiate(effectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 0.4f); // エフェクトの残骸を消去
            }
            yield return new WaitForSeconds(damageInterval);
          
        }
        if (damageCoroutines.ContainsKey(target))
            damageCoroutines.Remove(target);

    }

   
    private IEnumerator RangedAttackLoop()
    {
        while (true)
        {
            var lv = data.levels[level];
            if (!lv.isRanged)
            {
                yield return null;
                continue;
            }

            // クールタイム更新
            if (shootCooldown > 0f)
            {
                shootCooldown -= Time.deltaTime;
            }

            Vector2 dir = transform.up; //向いている方向

            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                dir,
                lv.attackRange,
                LayerMask.GetMask("Enemy")
            );
            // 射線に敵がいて、クールタイム終了してたら撃つ
            if (hit.collider != null && shootCooldown <= 0f)
            {
                FireProjectile(dir);
                shootCooldown = damageInterval;
            }

            yield return null;
        }
    }

    private void FireProjectile(Vector2 dir)
    {
        var lv = data.levels[level];
        if (lv.projectilePrefab == null) return;

        GameObject bullet = Instantiate(
            lv.projectilePrefab,
            transform.position,
            Quaternion.identity
        );

        StartCoroutine(MoveProjectileStraight(bullet, dir, lv));
    }
    //弾丸発射処理
    private IEnumerator MoveProjectile(GameObject bullet, Transform target, EquipmentLevelData lv)
    {
        while (bullet != null && target != null)
        {
            bullet.transform.position =
                Vector3.MoveTowards(
                    bullet.transform.position,
                    target.position,
                    lv.projectileSpeed * Time.deltaTime
                );

            // 命中
            if (Vector3.Distance(bullet.transform.position, target.position) < 0.1f)
            {
                // 敵にダメージ
                var rabbit = target.GetComponent<RabbitAI_Complete>();
                var crow = target.GetComponent<CrowFlockAI>();
                var bear = target.GetComponent<bear_Ai>();

                if (rabbit) rabbit.TakeDamage(lv.damage);
                if (crow) crow.TakeDamage(lv.damage);
                if (bear) bear.TakeDamage(lv.damage);

                // ヒットエフェクト
                if (lv.hitEffect != null)
                    Instantiate(lv.hitEffect, target.position, Quaternion.identity);

                Destroy(bullet);
                yield break;
            }

            yield return null;
        }

        if (bullet) Destroy(bullet);
    }

    private void OnMouseEnter()
    {
        UIManager ui = FindAnyObjectByType<UIManager>();
        ui.ShowTooltip($"Lv {level + 1}", transform.position);
    }

    private void OnMouseExit()
    {
        UIManager ui = FindAnyObjectByType<UIManager>();
        ui.HideTooltip();
    }

    private IEnumerator MoveProjectileStraight(
    GameObject bullet,
    Vector2 dir,
    EquipmentLevelData lv
)
    {
        float traveled = 0f;

        while (bullet != null && traveled < lv.attackRange)
        {
            float move = lv.projectileSpeed * Time.deltaTime;
            bullet.transform.position += (Vector3)(dir * move);
            traveled += move;

            // 命中判定
            Collider2D hit = Physics2D.OverlapPoint(
                bullet.transform.position,
                LayerMask.GetMask("Enemy")
            );

            if (hit != null)
            {
                var rabbit = hit.GetComponent<RabbitAI_Complete>();
                var crow = hit.GetComponent<CrowFlockAI>();
                var bear = hit.GetComponent<bear_Ai>();

                if (rabbit) rabbit.TakeDamage(lv.damage);
                if (crow) crow.TakeDamage(lv.damage);
                if (bear) bear.TakeDamage(lv.damage);

                if (lv.hitEffect != null)
                    Instantiate(lv.hitEffect, hit.transform.position, Quaternion.identity);

                Destroy(bullet);
                yield break;
            }

            yield return null;
        }

        if (bullet) Destroy(bullet);
    }
}
