
using System.Collections;
using UnityEngine;

// 敵のスポーン（出現）を管理するスクリプト。
//WaveManager から「StartSpawning / StopSpawning」を呼び出して制御する。
public class EnemySpawner : MonoBehaviour
{
    [Header("敵設定")]
    public GameObject[] enemyPrefab;    // 出現させる敵のプレハブ

    [Header("スポーン位置設定")]
    public Transform[] spawnPoints;   // 敵を出す座標（複数指定可）

    [Header("出現間隔設定")]
    public float spawnInterval = 2f;  // 基本の出現間隔（秒）

    private bool spawning = false;    // スポーン中かどうかを管理するフラグ

    // 敵の出現を開始（WaveManagerから呼ばれる）
    public void StartSpawning(int wave)
    {
        // 既にスポーン中でなければコルーチン(Coroutine)を開始
        if (!spawning)
            StartCoroutine(SpawnEnemies(wave));
    }

    // 敵の出現を停止（WaveManagerから呼ばれる）
    public void StopSpawning()
    {
        // コルーチン(Coroutine)内のループを抜けるためにフラグをfalseに
        spawning = false;
    }

    // 敵を一定間隔で出現させるコルーチン(Coroutine)
    private IEnumerator SpawnEnemies(int wave)
    {
        spawning = true;
        while (spawning)
        {
            // --- 敵を出現させる処理 ---

            // ランダムなスポーン位置
            int spawnIndex = Random.Range(0, spawnPoints.Length);

            // 出現位置をランダムに選択
            int enemyIndex = Random.Range(0, enemyPrefab.Length);

            // 指定位置に敵プレハブを生成
            Instantiate(enemyPrefab[enemyIndex], spawnPoints[spawnIndex].position, Quaternion.identity);

            // ウェーブが進むごとに出現頻度を上げる
            float interval = Mathf.Max(0.5f, spawnInterval - (wave * 0.2f));
            
            // 指定した間隔待機
            yield return new WaitForSeconds(interval);
        }
    }
}