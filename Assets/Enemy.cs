using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Settings")]
    public int maxHealth = 75;
    public float moveSpeed = 2.5f;
    public float detectionRange = 8f;
    public float attackRange = 5f;
    public float attackCooldown = 2f;
    
    [Header("Combat")]
    public int damage = 20;
    public GameObject bulletPrefab;
    public Transform firePoint;
    
    [Header("Rewards")]
    public GameObject coinPrefab;
    public int coinReward = 3;
    
    [Header("AI Behavior")]
    public float patrolDistance = 4f;
    public LayerMask groundLayerMask = 1; // Default layer
    
    private int currentHealth;
    private Transform player;
    private Rigidbody rb;
    private float lastAttackTime;
    private bool facingRight = true;
    private Vector3 startPosition;
    private float patrolDirection = 1;
    private bool isGrounded = false;
    
    private enum EnemyState { Patrol, Chase, Attack, Dead }
    private EnemyState currentState = EnemyState.Patrol;
    
    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();
        
        // CRITICAL: Ensure Rigidbody exists and is configured
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            Debug.Log($"⚠️ Added Rigidbody to {gameObject.name}");
        }
        
        // FIXED: Proper Rigidbody configuration for ground-based movement
        rb.freezeRotation = true;
        rb.useGravity = true;
        rb.mass = 1f;
        rb.linearDamping = 2f;  // Reduced damping for better movement
        rb.angularDamping = 10f;
        rb.isKinematic = false;
        
        // IMPORTANT: Don't freeze any position constraints
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        
        startPosition = transform.position;
        
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log($"✅ {gameObject.name} found player: {player.name}");
        }
        else
        {
            Debug.LogError($"❌ {gameObject.name} cannot find Player! Check Player tag!");
        }
        
        // Set initial facing direction based on scale
        if (transform.localScale.x < 0)
        {
            facingRight = false;
        }
        
        Debug.Log($"🤖 {gameObject.name} initialized!");
        Debug.Log($"💖 Health: {maxHealth}");
        Debug.Log($"🎯 Detection Range: {detectionRange}");
        Debug.Log($"🔫 Has Bullet Prefab: {bulletPrefab != null}");
        Debug.Log($"🎯 Has Fire Point: {firePoint != null}");
        
        if (bulletPrefab == null)
            Debug.LogWarning($"⚠️ {gameObject.name} MISSING BULLET PREFAB! Assign EnemyBullet prefab!");
        if (firePoint == null)
            Debug.LogWarning($"⚠️ {gameObject.name} MISSING FIRE POINT! Create empty child object!");
    }
    
    void Update()
    {
        if (currentState == EnemyState.Dead || player == null) 
        {
            return;
        }
        
        // Ground check
        CheckGrounded();
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Debug distance every 2 seconds
        if (Time.time % 2f < 0.1f)
        {
            Debug.Log($"🎯 {gameObject.name} - Distance: {distanceToPlayer:F1} | State: {currentState} | Grounded: {isGrounded}");
        }
        
        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                if (distanceToPlayer <= detectionRange)
                {
                    currentState = EnemyState.Chase;
                    Debug.Log($"👁️ {gameObject.name} spotted player!");
                }
                break;
                
            case EnemyState.Chase:
                ChasePlayer();
                if (distanceToPlayer <= attackRange)
                {
                    currentState = EnemyState.Attack;
                    Debug.Log($"⚔️ {gameObject.name} entering attack mode!");
                }
                else if (distanceToPlayer > detectionRange * 1.5f)
                {
                    currentState = EnemyState.Patrol;
                    Debug.Log($"🔍 {gameObject.name} lost player");
                }
                break;
                
            case EnemyState.Attack:
                AttackPlayer();
                if (distanceToPlayer > attackRange)
                {
                    currentState = EnemyState.Chase;
                    Debug.Log($"🏃 {gameObject.name} player moved away");
                }
                break;
        }
    }
    
    // FIXED: Ground detection to prevent floating
    void CheckGrounded()
    {
        float groundCheckDistance = 1.1f;
        Vector3 rayStart = transform.position;
        
        // Cast ray downward to check for ground
        RaycastHit hit;
        if (Physics.Raycast(rayStart, Vector3.down, out hit, groundCheckDistance, groundLayerMask))
        {
            isGrounded = true;
            
            // If enemy is too high above ground, pull them down
            if (hit.distance > 0.6f)
            {
                Vector3 newPos = transform.position;
                newPos.y = hit.point.y + 0.5f; // Keep enemy 0.5 units above ground
                transform.position = newPos;
            }
        }
        else
        {
            isGrounded = false;
        }
        
        // Debug ground detection
        Debug.DrawRay(rayStart, Vector3.down * groundCheckDistance, isGrounded ? Color.green : Color.red);
    }
    
    void Patrol()
    {
        if (!isGrounded) return; // Don't move if not grounded
        
        float distanceFromStart = Vector3.Distance(transform.position, startPosition);
        
        if (distanceFromStart >= patrolDistance)
        {
            patrolDirection *= -1;
            Debug.Log($"🔄 {gameObject.name} changing patrol direction");
        }
        
        // Check if there's ground ahead before moving
        if (CanMoveInDirection(patrolDirection))
        {
            MoveEnemy(patrolDirection * moveSpeed * 0.5f);
        }
        else
        {
            // Turn around if no ground ahead
            patrolDirection *= -1;
            Debug.Log($"🚧 {gameObject.name} hit edge, turning around");
        }
        
        // Handle facing direction
        if (patrolDirection > 0 && !facingRight) Flip();
        if (patrolDirection < 0 && facingRight) Flip();
    }
    
    void ChasePlayer()
    {
        if (!isGrounded || player == null) return;
        
        float direction = (player.position.x - transform.position.x);
        direction = direction > 0 ? 1 : -1;
        
        // Handle facing direction
        if (direction > 0 && !facingRight) Flip();
        if (direction < 0 && facingRight) Flip();
        
        // Check if there's ground ahead before moving
        if (CanMoveInDirection(direction))
        {
            MoveEnemy(direction * moveSpeed);
        }
        
        Debug.Log($"🏃 {gameObject.name} chasing player! Moving {(direction > 0 ? "right" : "left")}");
    }
    
    // FIXED: Check if enemy can move without falling off platforms
    bool CanMoveInDirection(float direction)
    {
        float checkDistance = 1.5f;
        Vector3 rayStart = transform.position + Vector3.right * direction * 0.8f;
        
        // Check if there's ground ahead
        RaycastHit hit;
        bool hasGroundAhead = Physics.Raycast(rayStart, Vector3.down, out hit, 2f, groundLayerMask);
        
        // Debug the ground check
        Debug.DrawRay(rayStart, Vector3.down * 2f, hasGroundAhead ? Color.green : Color.red);
        
        return hasGroundAhead;
    }
    
    // Movement method that respects gravity and ground
    void MoveEnemy(float horizontalSpeed)
    {
        if (rb == null) return;
        
        // Only move horizontally, preserve Y velocity for gravity
        Vector3 newVelocity = rb.linearVelocity;
        newVelocity.x = horizontalSpeed;
        rb.linearVelocity = newVelocity;
        
        // Debug movement
        if (Mathf.Abs(horizontalSpeed) > 0.1f)
        {
            Debug.Log($"🚶 {gameObject.name} moving at speed: {horizontalSpeed}");
        }
    }
    
    void AttackPlayer()
    {
        // Stop horizontal movement when attacking
        Vector3 velocity = rb.linearVelocity;
        velocity.x = 0;
        rb.linearVelocity = velocity;
        
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            Debug.Log($"🔥 {gameObject.name} TRYING TO SHOOT!");
            ShootAtPlayer();
            lastAttackTime = Time.time;
        }
        else
        {
            float timeLeft = (lastAttackTime + attackCooldown) - Time.time;
            Debug.Log($"⏰ {gameObject.name} attack cooldown: {timeLeft:F1}s remaining");
        }
    }
    
    // FIXED: Improved shooting with better error checking
    void ShootAtPlayer()
    {
        Debug.Log($"🎯 {gameObject.name} ShootAtPlayer() called!");
        
        if (bulletPrefab == null)
        {
            Debug.LogError($"❌ {gameObject.name} CANNOT SHOOT - NO BULLET PREFAB ASSIGNED!");
            Debug.LogError($"❌ Go to {gameObject.name} inspector and assign EnemyBullet prefab to 'Bullet Prefab' field!");
            return;
        }
        
        Vector3 spawnPosition;
        if (firePoint != null)
        {
            spawnPosition = firePoint.position;
            Debug.Log($"🎯 Using FirePoint position: {spawnPosition}");
        }
        else
        {
            // Use enemy position as backup
            spawnPosition = transform.position + Vector3.right * (facingRight ? 1 : -1);
            Debug.LogWarning($"⚠️ {gameObject.name} NO FIRE POINT! Using backup position: {spawnPosition}");
        }
        
        try
        {
            Debug.Log($"🚀 {gameObject.name} CREATING BULLET!");
            
            GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
            
            if (bullet == null)
            {
                Debug.LogError($"❌ Failed to instantiate bullet from prefab!");
                return;
            }
            
            Debug.Log($"✅ Bullet created successfully: {bullet.name}");
            
            // Setup bullet Rigidbody
            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
            if (bulletRb == null)
            {
                bulletRb = bullet.AddComponent<Rigidbody>();
                Debug.Log($"📦 Added Rigidbody to bullet");
            }
                
            bulletRb.useGravity = false;
            bulletRb.isKinematic = false;
            
            // Set bullet direction and speed
            float direction = facingRight ? 1 : -1;
            Vector3 bulletVelocity = new Vector3(direction * 15f, 0, 0);
            bulletRb.linearVelocity = bulletVelocity;
            
            Debug.Log($"💨 Bullet velocity set to: {bulletVelocity}");
            
            // Configure EnemyBullet script if it exists
            EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
            if (bulletScript != null)
            {
                bulletScript.direction = (int)direction;
                Debug.Log($"🎯 EnemyBullet script configured with direction: {direction}");
            }
            else
            {
                Debug.LogWarning($"⚠️ No EnemyBullet script found on bullet!");
            }
            
            // Auto-destroy bullet after 5 seconds
            Destroy(bullet, 5f);
            
            Debug.Log($"🎉 {gameObject.name} SUCCESSFULLY FIRED BULLET! Direction: {(facingRight ? "right" : "left")}");
            
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ {gameObject.name} SHOOTING ERROR: {e.Message}");
            Debug.LogError($"❌ Stack trace: {e.StackTrace}");
        }
    }
    
    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
        
        Debug.Log($"🔄 {gameObject.name} flipped! Now facing: {(facingRight ? "right" : "left")}");
    }
    
    // CRITICAL: This method MUST be public for bullets to call it
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"🤖 {gameObject.name} took {damage} damage! Health: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            Debug.Log($"💀 {gameObject.name} health reached zero! Calling Die()...");
            Die();
        }
    }
    
    void Die()
    {
        currentState = EnemyState.Dead;
        Debug.Log($"☠️ {gameObject.name} is DYING!");
        
        // Stop all movement
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
        
        // Drop coins
        if (coinPrefab != null)
        {
            for (int i = 0; i < coinReward; i++)
            {
                Vector3 coinPos = transform.position + new Vector3(
                    Random.Range(-2f, 2f), 
                    1f, 
                    Random.Range(-0.5f, 0.5f)
                );
                GameObject coin = Instantiate(coinPrefab, coinPos, Quaternion.identity);
                Debug.Log($"🪙 {gameObject.name} dropped coin {i + 1}");
            }
        }
        
        // Notify game manager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EnemyKilled();
        }
        
        // Destroy this enemy
        Destroy(gameObject, 0.5f);
    }
    
    // Ground collision detection
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
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
        }
    }
    
    // Draw gizmos in editor to visualize ranges
    void OnDrawGizmosSelected()
    {
        // Detection range (yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Attack range (red)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    
    // For debugging
    [ContextMenu("Test Shoot")]
    void TestShoot()
    {
        ShootAtPlayer();
    }
}