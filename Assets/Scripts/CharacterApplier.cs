using UnityEngine;

/// <summary>
/// Gắn script này trực tiếp vào Player Prefab hiện tại của bạn.
/// Nhiệm vụ: Đọc nhân vật được chọn từ "Menu" và tự động thay đổi Animator Controller (hoạt ảnh)
/// cho Player mà KHÔNG LÀM ẢNH HƯỞNG ĐẾN CODE CŨ (PlayerController).
/// </summary>
public class CharacterApplier : MonoBehaviour
{
    [System.Serializable]
    public class CharacterSkin
    {
        public string skinName;
        // Kéo thả file .controller tương ứng của từng Skin vào đây
        public RuntimeAnimatorController animatorController; 
    }

    // index 0 -> skin mặc định
    // index 1 -> skin mới số 1
    // index 2 -> skin mới số 2... (Thứ tự phải giống mảng trong CharacterSelectionShop)
    public CharacterSkin[] skins;

    void Start()
    {
        int selected = GlobalDataManager.SelectedCharacterIndex;

        // Nếu người chơi chọn 1 skin có tồn tại trong danh sách
        if (selected > 0 && selected < skins.Length)
        {
            Animator anim = GetComponent<Animator>();
            
            if (anim != null && skins[selected].animatorController != null)
            {
                // Thay đổi Animation Controller (Animation của Player sẽ đổi ngay lập tức)
                anim.runtimeAnimatorController = skins[selected].animatorController;
                Debug.Log("Thay đổi skin nhân vật thành: " + skins[selected].skinName);
            }
        }
    }
}
