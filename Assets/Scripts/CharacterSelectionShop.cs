using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
public class CharacterInfo
{
    public string name;
    public Sprite artwork;
    public int price;
    public string description; 
}

public class CharacterSelectionShop : MonoBehaviour
{
    public CharacterInfo[] characters;
    
    [Header("UI Grid Elements (Kéo thả 4 ô vào đây)")]
    public GameObject[] characterPanels; // Kéo 4 cái Khung_Character vào đây (để ẩn đi nếu trang cuối bị thiếu)
    public TextMeshProUGUI[] nameTexts;  
    public Image[] characterDisplays;     // Tuỳ chọn: Nếu bạn có hình nhân vật trong ô lưới
    public TextMeshProUGUI[] priceTexts; 
    public TextMeshProUGUI[] descriptionTexts; 
    public Button[] buyButtons;
    public Button[] selectButtons;

    [Header("Tổng Vàng")]
    public TextMeshProUGUI totalCoinText;
    
    // Trang hiện tại (Bắt đầu từ 0)
    private int currentPage = 0;
    private int itemsPerPage = 4; // Cố định 4 ô

    void Start()
    {
        GlobalDataManager.AddCoins(10000);
        currentPage = 0;
        UpdateUI();
    }

    public void NextCharacter()
    {
        // Tính số trang tối đa
        int maxPage = (characters.Length - 1) / itemsPerPage;
        
        if (currentPage < maxPage)
        {
            currentPage++;
            UpdateUI();
        }
    }

    public void PreviousCharacter()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (characters.Length == 0) return;

        // Cập nhật Tổng vàng
        if (totalCoinText != null) totalCoinText.text = "" + GlobalDataManager.GetTotalCoins();

        int startIndex = currentPage * itemsPerPage;

        // Lặp qua 4 ô hiển thị
        for (int i = 0; i < itemsPerPage; i++)
        {
            int charIndex = startIndex + i;

            // Nếu vượt quá số lượng nhân vật thực tế -> Ẩn ô đó đi
            if (charIndex >= characters.Length)
            {
                if (characterPanels != null && characterPanels.Length > i)
                    characterPanels[i].SetActive(false);
                continue;
            }

            // Nếu nằm trong phạm vi nhân vật -> Hiện lên và điền dữ liệu
            if (characterPanels != null && characterPanels.Length > i)
                characterPanels[i].SetActive(true);

            CharacterInfo info = characters[charIndex];
            
            // Cập nhật Tên, Hình ảnh, Mô tả
            if (nameTexts != null && nameTexts.Length > i) nameTexts[i].text = info.name;
            if (descriptionTexts != null && descriptionTexts.Length > i) descriptionTexts[i].text = info.description;
            if (characterDisplays != null && characterDisplays.Length > i && characterDisplays[i] != null) 
                characterDisplays[i].sprite = info.artwork;

            // Trạng thái mở khóa
            bool isUnlocked = GlobalDataManager.IsCharacterUnlocked(charIndex);

            if (isUnlocked)
            {
                // Đã sở hữu
                if (priceTexts != null && priceTexts.Length > i) priceTexts[i].text = "Đã Có";
                
                if (buyButtons != null && buyButtons.Length > i) buyButtons[i].gameObject.SetActive(false);
                
                if (selectButtons != null && selectButtons.Length > i) 
                {
                    selectButtons[i].gameObject.SetActive(true);
                    // Xám nút nếu đang được chọn
                    selectButtons[i].interactable = (charIndex != GlobalDataManager.SelectedCharacterIndex);
                }
            }
            else
            {
                // Chưa mua
                if (priceTexts != null && priceTexts.Length > i) priceTexts[i].text = "" + info.price;
                
                if (buyButtons != null && buyButtons.Length > i) 
                {
                    buyButtons[i].gameObject.SetActive(true);
                    // Không đủ tiền -> Khóa nút mua
                    buyButtons[i].interactable = GlobalDataManager.GetTotalCoins() >= info.price;
                }
                
                if (selectButtons != null && selectButtons.Length > i) selectButtons[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Gắn hàm này vào Nút Buy ở ô thứ i (0,1,2,3) trên giao diện.
    /// Giá trị truyền vào (panelIndex) từ 0 đến 3 tương ứng vị trí Khung
    /// </summary>
    public void BuyCharacterInPanel(int panelIndex)
    {
        int charIndex = (currentPage * itemsPerPage) + panelIndex;
        if (charIndex >= characters.Length) return;

        CharacterInfo info = characters[charIndex];
        if (GlobalDataManager.GetTotalCoins() >= info.price)
        {
            GlobalDataManager.SpendCoins(info.price);
            GlobalDataManager.UnlockCharacter(charIndex);
            
            // Mua xong cho phép tự chọn luôn
            GlobalDataManager.SelectedCharacterIndex = charIndex; 
            UpdateUI();
        }
    }

    /// <summary>
    /// Gắn hàm này vào Nút Select ở ô thứ i (0,1,2,3) trên giao diện.
    /// Giá trị truyền vào (panelIndex) từ 0 đến 3 tương ứng vị trí Khung
    /// </summary>
    public void SelectCharacterInPanel(int panelIndex)
    {
        int charIndex = (currentPage * itemsPerPage) + panelIndex;
        if (charIndex >= characters.Length) return;

        GlobalDataManager.SelectedCharacterIndex = charIndex;
        UpdateUI();
    }

    /// <summary>
    /// Hàm dùng cho nút quay lại để về Scene Menu
    /// </summary>
    public void BackToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
