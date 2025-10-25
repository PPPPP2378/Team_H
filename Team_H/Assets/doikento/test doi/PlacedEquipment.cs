using UnityEngine;

public class PlacedEquipment : MonoBehaviour
{
    public EquipmentData data; // 元の設備データ
    public int level = 0;      // 現在のレベル（0スタート）

    private SpriteRenderer sr;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
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

        // DamageTrapがついている場合、攻撃力を更新
        DamageTrap trap = GetComponent<DamageTrap>();
        if (trap != null)
            trap.damage = stats.damage;
    }
}
