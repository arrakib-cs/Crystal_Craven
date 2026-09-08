using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float jumpForce = 12f;
    
    [Header("Shooting Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.2f;
    
    [Header("Player Stats")]
    public int maxHealth = 100;
    public int currentHealth;
    public int coins = 0;
    public int lives = 3;
    
    // Private variables
    private Rigidbody rb;
    private bool isGrounded = true;
    private bool facingRight = true;
    private float nextFireTime = 0f;
    private PlayerUI playerUI;
    
    void Start()
    {
        // Get Rigidbody component
        rb = GetComponent<Rigidbody>();
        
        if (rb == null)
        {
            Debug.LogError("❌ No Rigidbody found on Player!");
            return;
        }
        
        // Setup Rigidbody
        rb.mass = 1f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.freezeRotation = true;
        
        // Set initial rotation (facing right)
        transform.rotation = Quaternion.Euler(0, 90, 0);
        facingRight = true;
        
        // Initialize health
        currentHealth = maxHealth;
        
        // Find UI
        playerUI = FindObjectOfType<PlayerUI>();
        UpdateUI();
        
        Debug.Log("🎮 Player ready! Using ROTATION instead of scale!");
        Debug.Log($"🔫 Bullet Prefab assigned: {bulletPrefab != null}");
        Debug.Log($"🎯 Fire Point assigned: {firePoint != null}");
        
        if (bulletPrefab == null)
            Debug.LogError("❌ CRITICAL: No bullet prefab assigned to Player!");
        if (firePoint == null)
            Debug.LogError("❌ CRITICAL: No fire point assigned to Player!");
    }
    
    void Update()
    {
        HandleMovement();
        HandleShooting();
        
        // Debug stats with H key
        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log($"💖 Health: {currentHealth}/{maxHealth} | 🪙 Coins: {coins} | ❤️ Lives: {lives}");
            Debug.Log($"🏃 Grounded: {isGrounded} | 👉 Facing Right: {facingRight}");
            Debug.Log($"🔄 Rotation: {transform.rotation.eulerAngles}");
        }
    }
    
    void HandleMovement()
    {
        float moveInput = 0;
        
        // Left movement
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            moveInput = -1;
            if (facingRight) 
            {
                FlipWithRotation();
            }
        }
        
        // Right movement  
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            moveInput = 1;
            if (!facingRight) 
            {
                FlipWithRotation();
            }
        }
        
        // Apply horizontal movement
        Vector3 velocity = rb.linearVelocity;
        velocity.x = moveInput * moveSpeed;
        rb.linearVelocity = velocity;
        
        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, 0);
            isGrounded = false;
            Debug.Log("🚀 Jump!");
        }
    }
    
    void HandleShooting()
    {
        // Shoot with X key
        if (Input.GetKeyDown(KeyCode.X))
        {
            Debug.Log("❌ X key pressed! Attempting to shoot...");
            Shoot();
        }
    }
    
    void Shoot()
    {
        Debug.Log("🔫 Shoot method called!");
        
        if (Time.time < nextFireTime)
        {
            Debug.Log("⏰ Too soon to shoot again!");
            return;
        }
        
        if (bulletPrefab == null)
        {
            Debug.LogError("❌ NO BULLET PREFAB ASSIGNED!");
            return;
        }
        
        if (firePoint == null)
        {
            Debug.LogError("❌ NO FIRE POINT ASSIGNED!");
            return;
        }
        
        try
        {
            Debug.Log($"🚀 Creating bullet at position: {firePoint.position}");
            
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            
            if (bullet == null)
            {
                Debug.LogError("❌ Failed to create bullet!");
                return;
            }
            
            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
            if (bulletRb == null)
                bulletRb = bullet.AddComponent<Rigidbody>();
            
            bulletRb.useGravity = false;
            bulletRb.isKinematic = false;
            
            float direction = facingRight ? 1 : -1;
            Vector3 bulletVelocity = new Vector3(direction * 20f, 0, 0);
            bulletRb.linearVelocity = bulletVelocity;
            
            Debug.Log($"💥 Bullet moving! Direction: {direction}, Velocity: {bulletVelocity}");
            
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.direction = facingRight ? 1 : -1;
            }
            
            Destroy(bullet, 5f);
            nextFireTime = Time.time + fireRate;
            
            Debug.Log("🎯 SHOOTING SUCCESS!");
            
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ ERROR creating bullet: {e.Message}");
        }
    }
    
    // NO SCALE ISSUES: Use rotation instead
    void FlipWithRotation()
    {
        facingRight = !facingRight;
        
        if (facingRight)
        {
            transform.rotation = Quaternion.Euler(0, 90, 0);  // Face right
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, -90, 0); // Face left
        }
        
        Debug.Log($"🔄 Player rotated! Now facing: {(facingRight ? "Right" : "Left")}");
        Debug.Log($"🔄 Rotation set to: {transform.rotation.eulerAngles}");
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        UpdateUI();
        
        Debug.Log($"😵 Ouch! Took {damage} damage! Health: {currentHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void AddCoins(int amount)
    {
        coins += amount;
        UpdateUI();
        Debug.Log($"🪙 Collected {amount} coins! Total: {coins}");
    }
    
    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(maxHealth, currentHealth);
        UpdateUI();
        Debug.Log($"💚 Healed {amount}! Health: {currentHealth}");
    }
    
    void Die()
    {
        lives--;
        Debug.Log($"💀 Player died! Lives left: {lives}");
        
        if (lives > 0)
        {
            transform.position = new Vector3(0, 3, 0);
            rb.linearVelocity = Vector3.zero;
            currentHealth = maxHealth;
            facingRight = true;
            transform.rotation = Quaternion.Euler(0, 90, 0); // Reset rotation
            isGrounded = true;
            UpdateUI();
            Debug.Log("🔄 Respawned!");
        }
        else
        {
            Debug.Log("🎮 GAME OVER!");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
        }
    }
    
    void UpdateUI()
    {
        if (playerUI != null)
        {
            playerUI.UpdateHealth(currentHealth, maxHealth);
            playerUI.UpdateCoins(coins);
            playerUI.UpdateLives(lives);
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            Debug.Log($"✅ Landed on: {collision.gameObject.name}");
        }
    }
    
    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
    
    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
            Debug.Log($"⚠️ Left ground: {collision.gameObject.name}");
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Coin"))
        {
            AddCoins(1);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Crystal"))
        {
            AddCoins(5);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("HealthPickup"))
        {
            Heal(25);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("LevelExit"))
        {
            Debug.Log("🏆 Level Complete!");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CompleteLevel();
            }
        }
    }
}