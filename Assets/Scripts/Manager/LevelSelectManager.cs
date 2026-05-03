using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectManager : MonoBehaviour
{
    public void LoadLevel1()
    {
        SceneManager.LoadScene("Level1");
    }

    public void LoadLevel2()
    {
        SceneManager.LoadScene("Level2");
    }

    public void LoadLevel3()
    {
        SceneManager.LoadScene("Level3");
    }

    public void LoadLevel4()
    {
        SceneManager.LoadScene("Level4");
    }

    public void LoadLevel5()
    {
        SceneManager.LoadScene("Level5");
    }

    // add more in the future

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}