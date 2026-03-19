using UnityEngine;

public class Projectile : MonoBehaviour
{
 private float speed = 10; // Tăng tốc độ lên vì dùng vật lý
    private float _direction;
    private bool hit;

    private Animator animator;
    private BoxCollider2D boxCollider;
    private Rigidbody2D body; // Thêm biến Rigidbody2D

    private void Awake()
    {
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        body = GetComponent<Rigidbody2D>(); // Khởi tạo Rigidbody2D
    }

    private void FixedUpdate() // Dùng FixedUpdate cho các lệnh liên quan đến vật lý
    {
        if (hit) return;

        // Cập nhật vận tốc trực tiếp cho Rigidbody2D
        body.linearVelocity = new Vector2(_direction * speed, 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Tránh va chạm với chính người chơi
        if (collision.CompareTag("Player")) return;

        // 2. Kiểm tra nếu chạm vào Kẻ địch (Enemy) hoặc Boss
        if (collision.CompareTag("Enemy"))
        {
            // Thử lấy script Health hoặc Enemy/Boss để trừ máu
            // Giả sử bạn dùng hệ thống Health chung cho tất cả
            Health enemyHealth = collision.GetComponent<Health>();

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(17); // Gây 1 sát thương (hoặc dùng biến damage tùy chỉnh)
            }
            else
            {
                // Nếu bạn không dùng script Health chung, hãy gọi trực tiếp script Enemy
                Enemy enemy = collision.GetComponent<Enemy>();
                if (enemy != null) enemy.TakeDamage(17);

                BossController boss = collision.GetComponent<BossController>();
                if (boss != null) boss.TakeDamage(17);
            }
        }

        // 3. Logic xử lý khi trúng mục tiêu (bất kể là tường hay kẻ địch)
        hit = true;
        body.linearVelocity = Vector2.zero;
        boxCollider.enabled = false;

        if (animator != null)
            animator.SetTrigger("explode"); // Chạy hiệu ứng nổ
        else
            gameObject.SetActive(false); // Biến mất ngay nếu không có animation nổ
    }

    public void SetDirection(float direction)
    {
        _direction = direction;
        gameObject.SetActive(true);
        hit = false;
        boxCollider.enabled = true;

        // Xoay hướng hình ảnh quả đạn
        float localScaleX = Mathf.Abs(transform.localScale.x) * direction;
        transform.localScale = new Vector3(localScaleX, transform.localScale.y, transform.localScale.z);
    }

    private void Deactivate()
    {
        gameObject.SetActive(false);
    }
}