using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int hp = 100;               // �̗�
    public float damageInterval = 0.5f; // �_���[�W���󂯂�Ԋu�i�b�j

    private float damageTimer = 0f;     // �_���[�W�Ԋu�p�^�C�}�[
    private bool onTrap = false;        // �g���b�v��ɂ��邩�ǂ���
    private int trapDamage = 0;         // ���݂̃g���b�v�̃_���[�W��

    void Update()
    {
        if (onTrap)
        {
            damageTimer -= Time.deltaTime;
            if (damageTimer <= 0f)
            {
                TakeDamage(trapDamage);
                damageTimer = damageInterval; // �^�C�}�[�����Z�b�g
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

            Debug.Log($"�g���b�v�ɏ�����I �_���[�W:{trapDamage}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Discharge"))
        {
            onTrap = false;
            Debug.Log("�g���b�v���痣�ꂽ");
        }
    }

    public void TakeDamage(int dmg)
    {
        hp -= dmg;
        Debug.Log($"�G��{dmg}�_���[�W���󂯂��I �c��HP: {hp}");

        if (hp <= 0)
        {
            Debug.Log("�G��|���܂����I");
            Destroy(gameObject);
        }
    }
}