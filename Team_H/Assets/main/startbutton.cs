using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; //シーン切り替えに必要

public class ChangeScene : MonoBehaviour
{
    public string scenename;//読み込むシーン名

    //start is called before the first frame update
    private void Start()
    {
        
    }
    //UPdate is called once per frame
    private void Update()
    {
        
    }
    //シーンを読み込む
    public void Load()
    {
        SceneManager.LoadScene(scenename);
    }

}
