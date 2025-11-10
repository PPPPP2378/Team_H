using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    public string nextScene;
    public float minimumLoadTime = 2.5f; // ← ここで「最低ロード時間」を指定

    IEnumerator Start()
    {
        float startTime = Time.time;
        var async = SceneManager.LoadSceneAsync(nextScene);
        async.allowSceneActivation = false;

        while (!async.isDone)
        {
            // 読み込み完了手前 (90%)
            if (async.progress >= 0.9f)
            {
                // 経過時間が minimumLoadTime になるまで待つ
                if (Time.time - startTime >= minimumLoadTime)
                {
                    async.allowSceneActivation = true;
                }
            }
            yield return null;
        }
    }
}