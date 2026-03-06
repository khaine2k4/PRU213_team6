using UnityEngine;

/// <summary>
/// Gắn script này vào một GameObject rỗng trong Scene "Game".
/// Nhiệm vụ: Tự động cộng số điểm (hoặc số coin) vào "Tổng vàng" khi thua hoặc thắng, 
/// mà không càn chỉnh sửa code cũ của GameManager.
/// </summary>
public class CoinCollector : MonoBehaviour
{
    private GameManager gameManager;
    private bool coinsCollected = false;

    void Start()
    {
        // Tự động tìm GameManager trong scene
        gameManager = FindFirstObjectByType<GameManager>();
    }

    void Update()
    {
        if (gameManager == null) return;

        // Nếu game kết thúc (thua hoặc thắng) mà chưa cộng tiền thì tự động cộng
        if ((gameManager.IsGameOver() || gameManager.IsGameWin()) && !coinsCollected)
        {
            GlobalDataManager.AddCoins(gameManager.score);
            coinsCollected = true;
        }
    }
}
