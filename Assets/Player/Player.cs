using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 moveInput;

    private bool isLocked = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isLocked) return; 

        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(KeyCode.J)) 
        {
            StartCoroutine(AttackRoutine());
        }
    }

    private void FixedUpdate()
    {
        if (isLocked) return;

        rb.linearVelocity = moveInput.normalized * moveSpeed;

        animator.SetFloat("Speed", moveInput.magnitude);

        if (moveInput.x > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput.x < 0) transform.localScale = new Vector3(-1, 1, 1);
    }

    private System.Collections.IEnumerator AttackRoutine()
    {
        isLocked = true;
        rb.linearVelocity = Vector2.zero; 

        animator.SetTrigger("attack");

        yield return new WaitForSeconds(0.4f);

        isLocked = false;
    }

    public void TakeDamage()
    {
        StopAllCoroutines(); 
        StartCoroutine(HurtRoutine());
    }

    private System.Collections.IEnumerator HurtRoutine()
    {
        isLocked = true;
        rb.linearVelocity = Vector2.zero;

        animator.SetTrigger("hurt");

        yield return new WaitForSeconds(0.3f); 

        isLocked = false;
    }
}