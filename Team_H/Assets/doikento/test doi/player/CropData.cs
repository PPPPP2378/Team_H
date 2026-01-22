using UnityEngine;

/// <summary>
/// 作物のステータス
/// インスペクターで個別に設定できる
/// </summary>
[System.Serializable]
public class CropData
{
    public string cropName;         // 作物名
    public Sprite seedSprite;       // 種まき時の見た目
    public Sprite growingSprite;    // 成長途中の見た目
    public Sprite grownSprite;      // 収穫時の見た目
    public float growTime = 5f;     // 発芽までの時間
    public float fullGrowTime = 5f; // 成熟までの時間
    public int harvestPoints = 10;  // 収穫時のスコア
    public int expGain = 10;        // 成長・収穫時の経験値
}