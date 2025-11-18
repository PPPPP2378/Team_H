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

    private Dictionary<MonoBehaviour, Coroutine> damageCoroutines = new Dictionary<MonoBehaviour, Coroutine>();

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
    }


    // レベルアップ処理
    public bool TryUpgrade(ref int currentScore)
    {
        if (level >= data.levels.Length - 1)
        {
            Debug.Log($"{data.name} は最大レベルです！");
            return false;
        }

        // レベル制限
        player_move player = FindAnyObjectByType<player_move>();
        if (player != null)
        {
            int nextEquipmentLevel = level + 1;

            if (player.PlayerLevel < nextEquipmentLevel)
            {
                Debug.Log($"プレイヤーレベルが足りません！ (必要Lv: {nextEquipmentLevel})");
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
        return true;
    }

    private void ApplyLevelStats()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        var stats = data.levels[level];
        sr.sprite = stats.sprite;

        // 攻撃力更新
        damage = stats.damage;
    }

    // --- DamageTrap の機能統合 ---
    private int damage = 0;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;
        {
            RabbitAI_Complete rabbit = other.GetComponent<RabbitAI_Complete>();
            CrowFlockAI crow = other.GetComponent<CrowFlockAI>();
            MonoBehaviour target = rabbit != null ? (MonoBehaviour)rabbit : crow != null ? (MonoBehaviour)crow : null;
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

            MonoBehaviour target = rabbit != null ? (MonoBehaviour)rabbit : crow != null ? (MonoBehaviour)crow : null;
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
        while (target != null)
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

            yield return new WaitForSeconds(damageInterval);
        }
    }
}
