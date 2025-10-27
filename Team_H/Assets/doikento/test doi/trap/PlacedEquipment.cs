using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private Dictionary<RabbitAI_Complete, Coroutine> damageCoroutines = new Dictionary<RabbitAI_Complete, Coroutine>();
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        // Collider がない場合は追加（トラップとして機能させるため）
        if (col == null)
            col = gameObject.AddComponent<BoxCollider2D>();

        col.isTrigger = true; // 敵を通過させる場合はTrigger

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
        if (other.CompareTag("Enemy"))
        {
            RabbitAI_Complete rabbit = other.GetComponent<RabbitAI_Complete>();
            if (rabbit != null)
            {
                // ダメージを一度だけコルーチンで処理
                if (!damageCoroutines.ContainsKey(rabbit))
                {
                    Coroutine c = StartCoroutine(DealDamageOverTime(rabbit));
                    damageCoroutines.Add(rabbit, c);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            RabbitAI_Complete rabbit = other.GetComponent<RabbitAI_Complete>();
            if (rabbit != null && damageCoroutines.ContainsKey(rabbit))
            {
                StopCoroutine(damageCoroutines[rabbit]);
                damageCoroutines.Remove(rabbit);
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

    private IEnumerator DealDamageOverTime(RabbitAI_Complete rabbit)
    {
        while (rabbit != null)
        {
            rabbit.TakeDamage(damage);
            Debug.Log($"[PlacedEquipment] {gameObject.name} が {rabbit.name} に {damage} ダメージを与えました！");
            yield return new WaitForSeconds(damageInterval);
        }
    }
}
