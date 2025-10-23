using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int hp = 100;               // 体力
    public float damageInterval = 0.5f; // ダメージを受ける間隔（秒）

    private float damageTimer = 0f;     // ダメージ間隔用タイマー
    private bool onTrap = false;        // トラップ上にいるかどうか
    private int trapDamage = 0;         // 現在のトラップのダメージ量

    void Update()
    {
        if (onTrap)
        {
            damageTimer -= Time.deltaTime;
            if (damageTimer <= 0f)
            {
                TakeDamage(trapDamage);
                damageTimer = damageInterval; // タイマーをリセット
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Discharge"))
        {
            onTrap = true;

            DamageTrap trap = other.GetComponent<DamageTrap>();
            trapDamage = (trap != null) ? trap.damage : 10;

            Debug.Log($"トラップに乗った！ ダメージ:{trapDamage}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Discharge"))
        {
            onTrap = false;
            Debug.Log("トラップから離れた");
        }
    }

    public void TakeDamage(int dmg)
    {
        hp -= dmg;
        Debug.Log($"敵が{dmg}ダメージを受けた！ 残りHP: {hp}");

        if (hp <= 0)
        {
            Debug.Log("敵を倒しました！");
            Destroy(gameObject);
        }
    }
}