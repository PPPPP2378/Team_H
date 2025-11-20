using UnityEngine;
using UnityEngine.UI;

public class StageSelect : MonoBehaviour
{
    public Button[] stageButtons; // Stage1, Stage2

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 解放されている最高ステージ番号（初期値は1）
        int unlocked = PlayerPrefs.GetInt("UnlockedStage", 1);
        Debug.Log("UnlockedStage 現在の値 = " + unlocked);


        for (int i = 0; i < stageButtons.Length; i++)
        {
            int stageNum = i + 1;
            // 解放されているステージ以下のボタンだけ押せるようにする
            stageButtons[i].interactable = (stageNum <= unlocked);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
