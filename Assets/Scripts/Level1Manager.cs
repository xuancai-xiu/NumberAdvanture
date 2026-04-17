using UnityEngine;
using UnityEngine.SceneManagement;

public class Level1Manager : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform goalPoint;
    [SerializeField] private float fallDeathHeight = -10f;

    [SerializeField] private Player player;    
    private bool levelComplete = false;

    public Transform SpawnPoint => spawnPoint;

    private void Start()
    {
        if (spawnPoint != null && player != null)
        {
            player.transform.position = spawnPoint.position;
        }
    }

    private void Update()
    {
        // Check if player fell off the map
        if (player != null && player.transform.position.y < fallDeathHeight)
        {
            player.Die();
            RestartLevel();
        }

        // Allow restarting level with R key
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartLevel();
        }
    }

    public void LevelComplete()
    {
        if (!levelComplete)
        {
            levelComplete = true;
            Debug.Log("Level Complete!");
            Invoke("GoToLevelSelect", 2f);
        }
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void GoToLevelSelect()
    {
        SceneManager.LoadScene("LevelSelect");
    }
}
