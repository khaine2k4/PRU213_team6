using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Health playerHealth;
    [SerializeField] private Image totalhealthBar;
    [SerializeField] private Image currentHealthBar; // Đây là ảnh có Fill Method: Horizontal

    void Start()
    {
        // Lúc bắt đầu, thanh tổng và thanh hiện tại đều đầy (100%)
        totalhealthBar.fillAmount = playerHealth.GetHealthPercentage();
    }
    private void Update()
    {
        // Cập nhật Fill Amount liên tục dựa trên % máu hiện tại
        // Ví dụ: 9 máu / 10 máu = 0.9 (khớp với yêu cầu của bạn)
        currentHealthBar.fillAmount = playerHealth.GetHealthPercentage();
    }
}