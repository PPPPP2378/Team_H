using UnityEngine;
using UnityEngine.SceneManagement; // シーン切り替えに必要
using System.Collections; // コルーチンに必要

public class ChangeScene : MonoBehaviour
{
    public string scenename; // 読み込むシーン名

    // 追加: AudioSourceコンポーネントを格納するための変数
    private AudioSource audioSource;

    // Start is called before the first frame update
    private void Start()
    {
        // 追加: 自身にアタッチされているAudioSourceコンポーネントを取得する
        // Startボタンなど、このスクリプトがアタッチされているオブジェクトに
        // AudioSourceコンポーネントと音源ファイルを必ずアタッチしてください。
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    private void Update()
    {

    }

    // シーンを読み込む（ボタンなどから呼び出す関数）
    // 従来のLoad()関数を、コルーチンを呼び出す関数に置き換えます。
    public void Load()
    {
        // シーン切り替え処理をコルーチンとして開始
        StartCoroutine(LoadAfterSound());
    }

    // 追加: 効果音が鳴り終わるのを待ってからシーンを読み込むコルーチン
    private IEnumerator LoadAfterSound()
    {
        // AudioSourceが設定されていれば
        if (audioSource != null)
        {
            // 1. 効果音を鳴らす
            audioSource.Play();

            // 2. 効果音の再生時間だけ待機する
            // clip.lengthでアタッチされた音源の再生時間が取得できます
            yield return new WaitForSeconds(audioSource.clip.length);
        }
        LoadingManager.nextSceneName = scenename;

        // 3. シーンを読み込む
        SceneManager.LoadScene("LoadingScene");
    }
}