using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isGrounded;
    private float horizontalInput;
    private bool jumpPressed;
    private bool die;
    private bool respawnEnd;
    private Animator animator;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        die = false;
        respawnEnd = false;
    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpPressed = true;
        }

        CheckGround();

        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        if (die || !respawnEnd) return;
        if (jumpPressed && isGrounded)
        {
            Jump();
        }

        MoveHorizontal();

        jumpPressed = false;
    }

    private void CheckGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        isGrounded = hit.collider != null;
    }

    private void MoveHorizontal()
    {
        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }

    private void RespawnEnded()
    {
        respawnEnd = true;
        animator.SetTrigger("respawnEnd");
    }

    public void Die()
    {
        if (die) return; 
        die = true;
        rb.velocity = Vector2.zero;
        animator.SetTrigger("die");
    }

    private void RestartLevel()
    {
        Level1Manager levelManager = FindObjectOfType<Level1Manager>();
        levelManager.RestartLevel();
    }

    private void UpdateAnimation()
    {
        if (die || !respawnEnd) return;
        animator.SetBool("isIdle", false);
        animator.SetBool("isRunning", false);
        animator.SetBool("isJumping", false);
        animator.SetBool("isFalling", false);


        bool isIdle = isGrounded && Mathf.Approximately(horizontalInput, 0);
        bool isRunning = isGrounded && !Mathf.Approximately(horizontalInput, 0);
        bool isJumping = !isGrounded && rb.velocity.y > 0;
        bool isFalling = !isGrounded && rb.velocity.y <= 0;

        animator.SetBool("isIdle", isIdle);
        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isJumping", isJumping);
        animator.SetBool("isFalling", isFalling);

        if (horizontalInput > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (horizontalInput < 0)
        {
            spriteRenderer.flipX = true;
        }
        
    }
}