using UnityEngine;

/// <summary>
/// Gắn script này trực tiếp vào Player Prefab.
/// Nhiệm vụ: Đồng bộ Animator và khả năng bắn đạn dựa trên nhân vật đã chọn.
/// </summary>
public class CharacterApplier : MonoBehaviour
{
    [System.Serializable]
    public class CharacterSkin
    {
        public string skinName;
        // Kéo thả file .controller hoặc .overrideController vào đây
        public RuntimeAnimatorController animatorController;
    }

    [Header("Danh sách Skins (Thứ tự phải khớp với Shop)")]
    public CharacterSkin[] skins;

    void Start()
    {
        // 1. Lấy chỉ số nhân vật từ Data Manager
        int selected = GlobalDataManager.SelectedCharacterIndex;

        // 2. Cấu hình khả năng bắn cho PlayerController
        PlayerController pc = GetComponent<PlayerController>();
        if (pc != null)
        {
            // Reset lại canShoot: Chỉ nhân vật khác 0 mới được bắn
            // Điều này đảm bảo khi quay về Hiệp sĩ (0), canShoot sẽ luôn là false
            if (selected == 1) // Chỉ riêng con số 1 là được phun lửa
            {
                pc.canShoot = true;
            }
            else // Tất cả các con khác (0, 2, 3...) đều tắt phun lửa
            {
                pc.canShoot = false;
            }

            Debug.Log("Nhân vật số " + selected + " - Phun lửa: " + pc.canShoot);
        }

        // 3. Cấu hình ngoại hình (Animator)
        // Thay đổi từ (selected > 0) thành (selected >= 0) để cập nhật cả nhân vật mặc định
        if (selected >= 0 && selected < skins.Length)
        {
            Animator anim = GetComponent<Animator>();
            if (anim != null && skins[selected].animatorController != null)
            {
                // Gán bộ điều khiển hoạt ảnh tương ứng
                anim.runtimeAnimatorController = skins[selected].animatorController;

                // Buộc Animator cập nhật ngay lập tức để tránh lỗi hình ảnh cũ
                anim.Update(0);

                Debug.Log("Đã áp dụng ngoại hình: " + skins[selected].skinName);
            }
        }
        else
        {
            Debug.LogWarning("Chỉ số nhân vật không hợp lệ hoặc chưa thiết lập Skin trong mảng!");
        }
    }
}