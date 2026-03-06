using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    private GameManager gameManager;
    private AudioManager audioManager;
    private Health playerHealth;

    // NEW: Biến này để nhớ xem nhân vật đã có chìa khóa trong người chưa
    public bool hasKey = false;

    private void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        audioManager = FindAnyObjectByType<AudioManager>();
        playerHealth = GetComponent<Health>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Coin"))
        {
            gameManager.addScore(1);
            Destroy(collision.gameObject);
            // Debug.Log("Coin");
            audioManager.playcoinsound();
        }

        if (collision.CompareTag("Trap"))
        {
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1); // Trừ 1 máu
            }
        }

        // Chạm Kẻ địch (Enemy) - CHỈ TRỪ MÁU
        if (collision.CompareTag("Enemy"))
        {
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1); // Trừ 1 máu
            }
        }

        // FIX: Chạm vào chìa khóa
        if (collision.CompareTag("Key"))
        {
            hasKey = true; // Cất chìa khóa vào túi
            Debug.Log("Đã nhặt được chìa khóa!");
            Destroy(collision.gameObject); // Xóa chìa khóa khỏi màn hình

        }
    }

    // NEW: Hàm xử lý khi tông vào Cánh Cửa (Vì Cửa là vật thể cứng, không có Is Trigger)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Door"))
        {
            if (hasKey == true)
            {
                Animator doorAnim = collision.gameObject.GetComponent<Animator>();
                doorAnim.SetBool("isOpen", true);

                Collider2D doorCollider = collision.gameObject.GetComponent<Collider2D>();
                doorCollider.enabled = false;

                hasKey = false;
                Debug.Log("Đã mở cửa thành công! Vào đánh Boss thôi!");
            }
            else
            {
                Debug.Log("Bạn cần chìa khóa để mở cửa này!");
            }
        }
    }
}