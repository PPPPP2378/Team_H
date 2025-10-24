using System.Collections;
using UnityEngine;

// �G�̃X�|�[���i�o���j���Ǘ�����X�N���v�g�B
//WaveManager ����uStartSpawning / StopSpawning�v���Ăяo���Đ��䂷��B
public class EnemySpawner : MonoBehaviour
{
    [Header("�G�ݒ�")]
    public GameObject enemyPrefab;    // �o��������G�̃v���n�u

    [Header("�X�|�[���ʒu�ݒ�")]
    public Transform[] spawnPoints;   // �G���o�����W�i�����w��j

    [Header("�o���Ԋu�ݒ�")]
    public float spawnInterval = 2f;  // ��{�̏o���Ԋu�i�b�j

    private bool spawning = false;    // �X�|�[�������ǂ������Ǘ�����t���O

    // �G�̏o�����J�n�iWaveManager����Ă΂��j
    public void StartSpawning(int wave)
    {
        // ���ɃX�|�[�����łȂ���΃R���[�`��(Coroutine)���J�n
        if (!spawning)
            StartCoroutine(SpawnEnemies(wave));
    }

    // �G�̏o�����~�iWaveManager����Ă΂��j
    public void StopSpawning()
    {
        // �R���[�`��(Coroutine)���̃��[�v�𔲂��邽�߂Ƀt���O��false��
        spawning = false;
    }

    // �G�����Ԋu�ŏo��������R���[�`��(Coroutine)
    private IEnumerator SpawnEnemies(int wave)
    {
        spawning = true;
        while (spawning)
        {
            // --- �G���o�������鏈�� ---

            // �o���ʒu�������_���ɑI��
            int spawnIndex = Random.Range(0, spawnPoints.Length);
            
            // �w��ʒu�ɓG�v���n�u�𐶐�
            Instantiate(enemyPrefab, spawnPoints[spawnIndex].position, Quaternion.identity);

            // �E�F�[�u���i�ނ��Ƃɏo���p�x���グ��
            float interval = Mathf.Max(0.5f, spawnInterval - (wave * 0.2f));
            
            // �w�肵���Ԋu�ҋ@
            yield return new WaitForSeconds(interval);
        }
    }
}