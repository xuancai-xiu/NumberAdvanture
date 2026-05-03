using UnityEngine;

public class Trampoline : MonoBehaviour
{
    [SerializeField] private float bounceForce = 15f;
    [SerializeField] private Animator animator;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            
            rb.velocity = new Vector2(rb.velocity.x, 0); 
            rb.AddForce(Vector2.up * bounceForce, ForceMode2D.Impulse);

            animator.SetTrigger("Bounce");
        }
    }
}