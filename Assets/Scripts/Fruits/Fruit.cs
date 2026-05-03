using UnityEngine;

public class Fruit : MonoBehaviour
{
    private int value;
    private Animator animator;
    private Level1Manager levelManager;
    public AudioClip collectSound;       
    private AudioSource audioSource;

    public int Value => value;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        levelManager = FindObjectOfType<Level1Manager>();
    }

    public void Initialize(int fruitValue)
    {
        value = fruitValue;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }

    private void Collect()
    {
        levelManager.OnFruitCollected(value);

        animator.SetTrigger("Collect");
        audioSource.PlayOneShot(collectSound);
    }

    private void FruitDestroy() 
    {
        Destroy(gameObject);
    }

}