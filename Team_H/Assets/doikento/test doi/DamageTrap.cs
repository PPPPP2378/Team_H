using UnityEngine;

public class DamageTrap : MonoBehaviour
{
    public int damage = 10;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[DamageTrap] Trigger Enter: {other.name} (tag:{other.tag})");

        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log($"�G�� {damage} �_���[�W��^���܂����I");
            }
            else
            {
                Debug.LogWarning($"[DamageTrap] {other.name} �� Enemy �X�N���v�g��������܂���");
            }
        }
    }

  
}
