using UnityEngine;
public class EndButton : MonoBehaviour
{
    // 追加: AudioSourceコンポーネントを格納するための変数
    private AudioSource audioSource;

    void Start()
    {
        // 追加: 自身にアタッチされているAudioSourceコンポーネントを取得する
        audioSource = GetComponent<AudioSource>();
    }

    // ボタンが押されたときに呼ぶ関数
    public void OnEndButtonClicked()
    {
        // 追加: 効果音を鳴らす
        if (audioSource != null)
        {
            audioSource.Play();
        }

        // 遅延を入れて、音が鳴り終わってから終了させる工夫もできますが、
        // 今回はすぐに終了処理を続行します。

        // エディタ上では停止、ビルド後はアプリ終了
#if UNITY_EDITOR
        // エディタでは即座に停止
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // ビルド版では即座に終了
        Application.Quit();
#endif
    }
}