using UnityEngine;

public class EndButton : MonoBehaviour
{
    // ボタンが押されたときに呼ぶ関数
    public void OnEndButtonClicked()
    {
        // エディタ上では停止、ビルド後はアプリ終了
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}