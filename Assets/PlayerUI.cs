using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    [Header("Health UI")]
    public Slider healthSlider;
    public TextMeshProUGUI healthText;
    
    [Header("Game Stats")]
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI levelText;
    
    void Start()
    {
        // Set level text
        if (levelText != null)
        {
            int level = GameManager.Instance != null ? GameManager.Instance.currentLevel : 1;
            levelText.text = $"Crystal Caverns - Level {level}";
        }
        
        Debug.Log("📱 UI System ready!");
    }
    
    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.value = (float)currentHealth / maxHealth;
        }
        
        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }
    }
    
    public void UpdateCoins(int coins)
    {
        if (coinText != null)
        {
            coinText.text = $"Coins: {coins}";
        }
    }
    
    public void UpdateLives(int lives)
    {
        if (livesText != null)
        {
            livesText.text = $"Lives: {lives}";
        }
    }
}