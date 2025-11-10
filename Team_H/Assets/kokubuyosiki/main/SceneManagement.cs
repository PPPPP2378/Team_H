using UnityEngine;
using UnityEngine.SceneManagement;

public class GoGame : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("LoadingScene");
    }
}