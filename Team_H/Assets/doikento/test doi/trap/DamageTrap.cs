using UnityEngine;
using System.Collections;

public class DamageTrap : MonoBehaviour
{
    [Header("トラップ設定")]
    public int damage = 10;
    public float damageInterval = 0.5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            StartCoroutine(ApplyDamageOverTime(other));
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            StopCoroutine(ApplyDamageOverTime(other));
        }
    }

    private IEnumerator ApplyDamageOverTime(Collider2D target)
    {
        RabbitAI_Complete rabbit = target.GetComponent<RabbitAI_Complete>();
        if (rabbit == null) yield break;

        while (target != null && target.bounds.Intersects(GetComponent<Collider2D>().bounds))
        {
            rabbit.TakeDamage(damage);
            Debug.Log($"[DamageTrap] {target.name} に {damage} ダメージを与えました！");
            yield return new WaitForSeconds(damageInterval);
        }
    }
}
