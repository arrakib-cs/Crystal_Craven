using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Game Settings")]
    public int currentLevel = 1;
    public int totalLevels = 3;
    
    [Header("Level Objectives")]
    public int enemiesInLevel = 0;
    public int enemiesKilled = 0;
    public bool levelCompleteByExit = true;
    
    private bool gameIsOver = false;
    
    void Awake()
    {
        // Singleton pattern - only one GameManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        Time.timeScale = 1f;
        gameIsOver = false;
        CountEnemies();
        
        Debug.Log("🎮===== CRYSTAL CAVERNS: BATTLE RUN =====");
        Debug.Log($"🏰 Level {currentLevel} Started!");
        Debug.Log($"🤖 Enemies to defeat: {enemiesInLevel}");
        Debug.Log("🎯 Mission: Collect crystals, defeat enemies, find the exit!");
        Debug.Log("⚔️ Controls: WASD/Arrows=Move, Space=Jump, Mouse=Shoot");
    }
    
    void Update()
    {
        // Pause game
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame();
        }
        
        // Cheat codes for testing
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("🔄 Restarting level...");
            RestartLevel();
        }
        
        if (Input.GetKeyDown(KeyCode.N))
        {
            Debug.Log("⏭️ Skipping to next level...");
            CompleteLevel();
        }
        
        // Check win condition by enemy count
        if (!levelCompleteByExit && enemiesKilled >= enemiesInLevel && enemiesInLevel > 0)
        {
            CompleteLevel();
        }
    }
    
    void CountEnemies()
    {
        enemiesInLevel = GameObject.FindGameObjectsWithTag("Enemy").Length;
        enemiesKilled = 0;
    }
    
    public void EnemyKilled()
    {
        enemiesKilled++;
        Debug.Log($"🏆 Enemy defeated! Progress: {enemiesKilled}/{enemiesInLevel}");
        
        if (!levelCompleteByExit && enemiesKilled >= enemiesInLevel)
        {
            CompleteLevel();
        }
    }
    
    public void GameOver()
    {
        if (gameIsOver) return;
        
        gameIsOver = true;
        
        Debug.Log("💀 ===== GAME OVER ===== 💀");
        Debug.Log("😵 All lives lost!");
        Debug.Log("🔄 Press R to restart or wait for auto-restart...");
        
        // Auto restart after 3 seconds
        Invoke("RestartLevel", 3f);
    }
    
    public void CompleteLevel()
    {
        Debug.Log("🎉 ===== LEVEL COMPLETE! ===== 🎉");
        Debug.Log($"✨ Level {currentLevel} finished!");
        
        if (currentLevel >= totalLevels)
        {
            // Game completed!
            Debug.Log("🏆 ===== CONGRATULATIONS! ===== 🏆");
            Debug.Log("👑 YOU CONQUERED ALL CRYSTAL CAVERNS!");
            Debug.Log("🌟 You are the Crystal Cave Champion!");
            Debug.Log("🎊 Thanks for playing Crystal Caverns: Battle Run!");
        }
        else
        {
            // Next level
            Debug.Log($"🚀 Advancing to Level {currentLevel + 1}...");
            currentLevel++;
            Invoke("RestartLevel", 2f);
        }
    }
    
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void PauseGame()
    {
        if (Time.timeScale == 0)
        {
            Time.timeScale = 1f;
            Debug.Log("▶️ Game resumed!");
        }
        else
        {
            Time.timeScale = 0f;
            Debug.Log("⏸️ Game paused! Press Escape to resume.");
        }
    }
}