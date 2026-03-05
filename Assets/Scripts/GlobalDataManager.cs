using UnityEngine;

/// <summary>
/// Quản lý dữ liệu mua sắm nhân vật và tổng vàng toàn cục (cho toàn bộ game).
/// Không phụ thuộc vào scene, dùng PlayerPrefs để lưu cực nhanh.
/// </summary>
public static class GlobalDataManager
{
    private const string TOTAL_COINS_KEY = "TotalCoins";
    private const string SELECTED_CHAR_KEY = "SelectedCharacterIndex";
    private const string UNLOCKED_CHAR_PREFIX = "CharUnlocked_";

    // 1. Lấy tổng số vàng
    public static int GetTotalCoins()
    {
        return PlayerPrefs.GetInt(TOTAL_COINS_KEY, 0);
    }

    // 2. Thêm vàng vào tổng (gọi khi kết thúc game)
    public static void AddCoins(int amount)
    {
        int currentCoins = GetTotalCoins();
        PlayerPrefs.SetInt(TOTAL_COINS_KEY, currentCoins + amount);
        PlayerPrefs.Save();
        Debug.Log("Đã cộng " + amount + " vàng vào Tổng vàng. Tổng hiện tại: " + GetTotalCoins());
    }

    // 3. Trừ vàng khi mua nhân vật
    public static void SpendCoins(int amount)
    {
        int currentCoins = GetTotalCoins();
        if (currentCoins >= amount)
        {
            PlayerPrefs.SetInt(TOTAL_COINS_KEY, currentCoins - amount);
            PlayerPrefs.Save();
        }
    }

    // 4. Kiểm tra nhân vật đã mở khóa chưa (nhân vật 0 mặc định luôn mở)
    public static bool IsCharacterUnlocked(int characterIndex)
    {
        if (characterIndex == 0) return true; 
        return PlayerPrefs.GetInt(UNLOCKED_CHAR_PREFIX + characterIndex, 0) == 1;
    }

    // 5. Mở khóa nhân vật mới
    public static void UnlockCharacter(int characterIndex)
    {
        PlayerPrefs.SetInt(UNLOCKED_CHAR_PREFIX + characterIndex, 1);
        PlayerPrefs.Save();
        Debug.Log("Mở khóa nhân vật số: " + characterIndex);
    }

    // 6. Lưu và lấy ID nhân vật đang được chọn
    public static int SelectedCharacterIndex
    {
        get => PlayerPrefs.GetInt(SELECTED_CHAR_KEY, 0);
        set
        {
            PlayerPrefs.SetInt(SELECTED_CHAR_KEY, value);
            PlayerPrefs.Save();
            Debug.Log("Đã chọn nhân vật số: " + value);
        }
    }
}
