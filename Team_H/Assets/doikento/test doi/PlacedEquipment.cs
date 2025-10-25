using UnityEngine;

public class PlacedEquipment : MonoBehaviour
{
    public EquipmentData data; // ���̐ݔ��f�[�^
    public int level = 0;      // ���݂̃��x���i0�X�^�[�g�j

    private SpriteRenderer sr;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        ApplyLevelStats();
    }

    // ���x���A�b�v����
    public bool TryUpgrade(ref int currentScore)
    {
        if (level >= data.levels.Length - 1)
        {
            Debug.Log($"{data.name} �͍ő僌�x���ł��I");
            return false;
        }

        int nextCost = data.levels[level + 1].cost;
        if (currentScore < nextCost)
        {
            Debug.Log("�X�R�A������܂���I");
            return false;
        }

        // �R�X�g������
        currentScore -= nextCost;
        level++;
        ApplyLevelStats();

        Debug.Log($"{data.name} �����x�� {level + 1} �ɋ������܂����I");
        return true;
    }

    private void ApplyLevelStats()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        var stats = data.levels[level];
        sr.sprite = stats.sprite;

        // DamageTrap�����Ă���ꍇ�A�U���͂��X�V
        DamageTrap trap = GetComponent<DamageTrap>();
        if (trap != null)
            trap.damage = stats.damage;
    }
}
