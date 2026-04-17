using UnityEngine;

public class GoalPoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Level1Manager levelManager = FindObjectOfType<Level1Manager>();
            levelManager.LevelComplete();
        }
    }
}