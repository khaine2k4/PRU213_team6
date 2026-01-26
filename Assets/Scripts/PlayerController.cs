using UnityEngine;

public class PlayerController : MonoBehaviour
{
     [SerializeField] private float moveSpeed = 5f;
     [SerializeField] private float jumpForce = 15f;
     [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    private bool isGrounded;    
    private  Animator animator;
     private Rigidbody2D rb;
     private GameManager gameManager;   
     private AudioManager audioManager;     
     private  void Awake()
     {
         animator = GetComponent<Animator>();
         rb = GetComponent<Rigidbody2D>();
         gameManager = FindAnyObjectByType<GameManager>();  
         audioManager = FindAnyObjectByType<AudioManager>();  
     }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    { 
        if(gameManager.IsGameOver() || gameManager.IsGameWin()  )
        {
            return;
        }
        HandleMovement(); 
        HandleJump();
        UpdateAnimation();
    }

    private void HandleMovement()
    {
        float moveInput = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        if(moveInput>0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if(moveInput<0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
    
        }  
    }
    private void HandleJump()
    {
       if(Input.GetButtonDown("Jump")&&isGrounded)
       {
           rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
           audioManager.playjumpsound();    
       }
       isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }
    private void UpdateAnimation()
    {
        bool isRunning = Mathf.Abs(rb.linearVelocity.x) > 0.1f;
        animator.SetBool("isRunning", isRunning);   
        bool isJumping = !isGrounded;
        animator.SetBool("isJumping", isJumping);   
    }
}
