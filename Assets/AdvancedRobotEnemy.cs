using UnityEngine;

public class AdvancedRobotEnemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public int maxHealth = 100; // Enemy health - dies after 5 bullets (20 damage each)
    public float moveSpeed = 3f;
    public float jumpForce = 8f;
    public float detectionRange = 10f;
    public float attackRange = 6f;
    public float attackCooldown = 1.5f;
    
    [Header("Advanced Movement")]
    public float dodgeSpeed = 6f;
    public float dodgeDistance = 3f;
    public float groundCheckDistance = 1.5f;
    public float platformEdgeBuffer = 1f;
    
    [Header("Combat")]
    public int damage = 20;
    public GameObject bulletPrefab;
    public Transform firePoint;
    public GameObject explosionEffect; // Optional explosion prefab
    
    [Header("Rewards")]
    public GameObject coinPrefab;
    public int coinReward = 5;
    
    // Private variables
    private int currentHealth;
    private Transform player;
    private Rigidbody rb;
    private Animator animator;
    private float lastAttackTime;
    private float lastDodgeTime;
    private bool facingRight = true;
    private bool isGrounded = true;
    private bool isDodging = false;
    private Vector3 startPosition;
    private float patrolDirection = 1;
    private float dodgeDirection = 0;
    
    // AI States
    private enum EnemyState { Patrol, Chase, Attack, Dodge, Dead }
    private EnemyState currentState = EnemyState.Patrol;
    private EnemyState previousState = EnemyState.Patrol;
    
    // Ground detection
    private LayerMask groundLayerMask = -1;
    
    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        startPosition = transform.position;
        
        // Setup Rigidbody
        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.mass = 2f;
        }
        
        // DON'T override manual rotation - keep whatever you set in Inspector!
        // Just figure out which direction robot is facing based on current Y rotation
        float currentY = transform.eulerAngles.y;
        
        // Determine facing direction from current rotation
        if (currentY > 270 || currentY < 90)
        {
            facingRight = true;  // Robot is facing right-ish
        }
        else
        {
            facingRight = false; // Robot is facing left-ish
        }
        
        Debug.Log($"🤖 Robot keeping manual rotation Y: {currentY}, facing: {(facingRight ? "RIGHT" : "LEFT")}");
        
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log($"🤖 Advanced Robot {gameObject.name} found player!");
        }
        
        // Auto-create fire point if missing
        if (firePoint == null)
        {
            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(transform);
            fp.transform.localPosition = new Vector3(0f, 1f, 1f); // Forward position for -90 rotated robot
            firePoint = fp.transform;
            Debug.Log("🎯 Created FirePoint at forward position for robot");
        }
        
        // Position robot properly on ground
        PositionOnGround();
        
        Debug.Log($"🚀 Advanced Robot Enemy initialized with {maxHealth} health!");
        Debug.Log($"⚔️ Robot will die after 5 player bullets!");
        Debug.Log($"🔴 Robot bullets do 34 damage - player dies after 3 hits!");
    }
    
    void Update()
    {
        if (currentState == EnemyState.Dead || player == null) return;
        
        CheckGrounded();
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Check for incoming bullets (dodge behavior)
        CheckForIncomingBullets();
        
        // Update animations
        UpdateAnimations();
        
        // AI State Machine
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
                }
                else if (distanceToPlayer > detectionRange * 1.5f)
                {
                    currentState = EnemyState.Patrol;
                }
                break;
                
            case EnemyState.Attack:
                AttackPlayer();
                if (distanceToPlayer > attackRange)
                {
                    currentState = EnemyState.Chase;
                }
                break;
                
            case EnemyState.Dodge:
                PerformDodge();
                break;
        }
        
        // Prevent falling off platforms
        PreventFalling();
    }
    
    void PositionOnGround()
    {
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * 2f;
        
        if (Physics.Raycast(rayStart, Vector3.down, out hit, 10f))
        {
            if (hit.collider.CompareTag("Ground"))
            {
                // Position robot on the ground
                Vector3 newPos = hit.point;
                newPos.y += GetComponent<Collider>().bounds.size.y * 0.5f;
                transform.position = newPos;
                Debug.Log($"🏃 Robot positioned on ground at {newPos}");
            }
        }
    }
    
    void CheckGrounded()
    {
        RaycastHit hit;
        Vector3 rayStart = transform.position;
        
        isGrounded = Physics.Raycast(rayStart, Vector3.down, out hit, groundCheckDistance);
        
        if (isGrounded && hit.collider != null)
        {
            // Keep robot ON the ground - don't let it fly up!
            float targetY = hit.point.y + GetComponent<Collider>().bounds.size.y * 0.5f;
            Vector3 pos = transform.position;
            
            // Force robot to stay on ground level
            if (pos.y > targetY + 0.2f) // If robot is flying up
            {
                pos.y = targetY; // Snap back to ground
                transform.position = pos;
                
                // Stop upward movement
                if (rb != null)
                {
                    Vector3 vel = rb.linearVelocity;
                    vel.y = Mathf.Min(vel.y, 0); // No upward velocity
                    rb.linearVelocity = vel;
                }
            }
        }
        else
        {
            // If not grounded, push robot down
            if (rb != null)
            {
                Vector3 vel = rb.linearVelocity;
                vel.y = -5f; // Force downward
                rb.linearVelocity = vel;
            }
        }
        
        // Debug ray
        Debug.DrawRay(rayStart, Vector3.down * groundCheckDistance, isGrounded ? Color.green : Color.red);
    }
    
    void CheckForIncomingBullets()
    {
        GameObject[] bullets = GameObject.FindGameObjectsWithTag("PlayerBullet");
        
        foreach (GameObject bullet in bullets)
        {
            float distanceToBullet = Vector3.Distance(transform.position, bullet.transform.position);
            
            if (distanceToBullet < 4f && Time.time > lastDodgeTime + 1f)
            {
                // Check if bullet is coming towards us
                Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
                if (bulletRb != null)
                {
                    Vector3 bulletDirection = bulletRb.linearVelocity.normalized;
                    Vector3 toEnemy = (transform.position - bullet.transform.position).normalized;
                    
                    if (Vector3.Dot(bulletDirection, toEnemy) > 0.7f) // Bullet coming towards us
                    {
                        StartDodge();
                        break;
                    }
                }
            }
        }
    }
    
    void StartDodge()
    {
        if (isDodging) return;
        
        previousState = currentState;
        currentState = EnemyState.Dodge;
        isDodging = true;
        lastDodgeTime = Time.time;
        
        // ONLY horizontal dodging - NO JUMPING to prevent flying up
        dodgeDirection = Random.Range(0, 2) == 0 ? -1 : 1; // Left or right only
        
        Debug.Log($"🏃 {gameObject.name} dodging horizontally only!");
    }
    
    void PerformDodge()
    {
        if (dodgeDirection != 0)
        {
            // Horizontal dodge movement
            Vector3 dodgeMove = Vector3.right * dodgeDirection * dodgeSpeed * Time.deltaTime;
            transform.position += dodgeMove;
        }
        
        // End dodge after short time
        if (Time.time > lastDodgeTime + 0.8f)
        {
            isDodging = false;
            dodgeDirection = 0;
            currentState = previousState;
        }
    }
    
    void Patrol()
    {
        // Check if we're at platform edge
        if (IsAtPlatformEdge())
        {
            patrolDirection *= -1;
        }
        
        FaceDirection(patrolDirection > 0);
        MoveHorizontal(patrolDirection * moveSpeed * 0.6f);
        
        // Occasional random direction change
        if (Random.Range(0, 200) == 1)
        {
            patrolDirection *= -1;
        }
    }
    
    void ChasePlayer()
    {
        if (player == null) return;
        
        float direction = player.position.x - transform.position.x;
        FaceDirection(direction > 0);
        
        // Smart chasing - move up/down and left/right
        Vector3 moveDirection = Vector3.zero;
        
        // Horizontal movement towards player
        moveDirection.x = (direction > 0 ? 1 : -1) * moveSpeed;
        
        // Vertical movement if player is significantly higher/lower
        float heightDiff = player.position.y - transform.position.y;
        if (Mathf.Abs(heightDiff) > 2f && isGrounded && Random.Range(0, 30) == 1)
        {
            // Try to jump to reach player level
            if (rb != null)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
            }
        }
        
        MoveHorizontal(moveDirection.x);
        
        Debug.Log($"🏃 {gameObject.name} chasing player!");
    }
    
    void AttackPlayer()
    {
        if (player == null) return;
        
        FaceDirection(player.position.x > transform.position.x);
        
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            FireWeapon();
            lastAttackTime = Time.time;
        }
        
        // Move slightly during attack for dynamic combat
        float direction = player.position.x - transform.position.x;
        MoveHorizontal((direction > 0 ? 1 : -1) * moveSpeed * 0.3f);
    }
    
    bool IsAtPlatformEdge()
    {
        Vector3 edgeCheckPos = transform.position + Vector3.right * (facingRight ? platformEdgeBuffer : -platformEdgeBuffer);
        RaycastHit hit;
        
        bool groundAhead = Physics.Raycast(edgeCheckPos, Vector3.down, out hit, groundCheckDistance + 1f);
        
        // Debug rays
        Debug.DrawRay(edgeCheckPos, Vector3.down * (groundCheckDistance + 1f), groundAhead ? Color.blue : Color.yellow);
        
        return !groundAhead;
    }
    
    void PreventFalling()
    {
        if (IsAtPlatformEdge())
        {
            // Stop movement if at edge
            if (rb != null)
            {
                Vector3 vel = rb.linearVelocity;
                vel.x = 0;
                rb.linearVelocity = vel;
            }
        }
    }
    
    void FaceDirection(bool faceRight)
    {
        // DO NOTHING - Don't change rotation at all
        facingRight = faceRight;
        Debug.Log($"Robot should face {(faceRight ? "RIGHT" : "LEFT")} but keeping current rotation");
    }
    
    void MoveHorizontal(float speed)
    {
        if (rb != null)
        {
            Vector3 velocity = rb.linearVelocity;
            velocity.x = speed;
            rb.linearVelocity = velocity;
        }
        else
        {
            Vector3 movement = Vector3.right * speed * Time.deltaTime;
            transform.position += movement;
        }
    }
    
    void FireWeapon()
    {
        if (bulletPrefab == null) return;
        
        // Spawn bullet in front of robot
        Vector3 spawnPos = transform.position + Vector3.up * 1f;
        if (firePoint != null)
        {
            spawnPos = firePoint.position;
        }
        
        try
        {
            GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
            
            // The EnemyBullet script will handle the direction automatically
            // Just make sure it has the EnemyBullet script
            EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
            if (bulletScript == null)
            {
                bulletScript = bullet.AddComponent<EnemyBullet>();
            }
            
            Debug.Log($"💥 {gameObject.name} fired red bullet toward player!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Weapon fire error: {e.Message}");
        }
    }
    
    void UpdateAnimations()
    {
        if (animator == null) return;
        
        try
        {
            bool isMoving = Mathf.Abs(rb != null ? rb.linearVelocity.x : 0) > 0.1f;
            
            // Set various animation parameters
            SetAnimatorBool("isMoving", isMoving);
            SetAnimatorBool("isWalking", isMoving);
            SetAnimatorBool("Walk", isMoving);
            SetAnimatorBool("Moving", isMoving);
            SetAnimatorFloat("Speed", isMoving ? 1f : 0f);
            SetAnimatorFloat("Velocity", isMoving ? 1f : 0f);
            
            // Special states
            SetAnimatorBool("isGrounded", isGrounded);
            SetAnimatorBool("isDodging", isDodging);
            
            // Attack animation
            if (currentState == EnemyState.Attack && Time.time >= lastAttackTime + attackCooldown - 0.1f)
            {
                SetAnimatorTrigger("Attack");
                SetAnimatorTrigger("Shoot");
                SetAnimatorTrigger("Fire");
            }
        }
        catch { }
    }
    
    void SetAnimatorBool(string param, bool value)
    {
        try
        {
            if (HasParameter(param))
                animator.SetBool(param, value);
        }
        catch { }
    }
    
    void SetAnimatorFloat(string param, float value)
    {
        try
        {
            if (HasParameter(param))
                animator.SetFloat(param, value);
        }
        catch { }
    }
    
    void SetAnimatorTrigger(string param)
    {
        try
        {
            if (HasParameter(param))
                animator.SetTrigger(param);
        }
        catch { }
    }
    
    bool HasParameter(string param)
    {
        if (animator == null) return false;
        foreach (AnimatorControllerParameter p in animator.parameters)
        {
            if (p.name == param) return true;
        }
        return false;
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"🤖 {gameObject.name} took {damage} damage! Health: {currentHealth}/{maxHealth}");
        
        // Show damage visually
        if (currentHealth <= 80)
            Debug.Log("🟡 Robot is damaged!");
        if (currentHealth <= 60)
            Debug.Log("🟠 Robot is badly damaged!");
        if (currentHealth <= 40)
            Debug.Log("🔴 Robot is critically damaged!");
        if (currentHealth <= 20)
            Debug.Log("💀 Robot is almost dead!");
        
        // Trigger dodge behavior when hit (but don't jump too much)
        if (currentHealth > 0 && Random.Range(0, 4) == 0)
        {
            // Only horizontal dodging - no jumping to prevent flying
            previousState = currentState;
            currentState = EnemyState.Dodge;
            isDodging = true;
            lastDodgeTime = Time.time;
            dodgeDirection = Random.Range(0, 2) == 0 ? -1 : 1; // Left or right only
            Debug.Log($"🏃 Robot dodging horizontally!");
        }
        
        if (currentHealth <= 0)
        {
            Debug.Log($"💀 ROBOT HEALTH REACHED ZERO! Calling Die()...");
            Die();
        }
    }
    
    void Die()
    {
        currentState = EnemyState.Dead;
        Debug.Log($"💀 {gameObject.name} destroyed after 5 player bullets!");
        
        // Create explosion effect
        CreateExplosion();
        
        // Drop coins
        if (coinPrefab != null)
        {
            for (int i = 0; i < coinReward; i++)
            {
                Vector3 coinPos = transform.position + new Vector3(
                    Random.Range(-2f, 2f), 1f, Random.Range(-0.5f, 0.5f));
                GameObject coin = Instantiate(coinPrefab, coinPos, Quaternion.identity);
                
                // Add some physics to dropped coins
                Rigidbody coinRb = coin.GetComponent<Rigidbody>();
                if (coinRb != null)
                {
                    coinRb.AddForce(new Vector3(Random.Range(-5f, 5f), Random.Range(2f, 5f), 0), ForceMode.Impulse);
                }
            }
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EnemyKilled();
        }
        
        Destroy(gameObject, 0.5f);
    }
    
    void CreateExplosion()
    {
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }
        else
        {
            // Create simple explosion effect with particles
            GameObject explosion = new GameObject("Explosion");
            explosion.transform.position = transform.position;
            
            // Add a simple particle system
            ParticleSystem particles = explosion.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startLifetime = 1f;
            main.startSpeed = 10f;
            main.startSize = 0.5f;
            main.startColor = Color.red;
            main.maxParticles = 50;
            
            var emission = particles.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(0.0f, 50)
            });
            
            Destroy(explosion, 2f);
        }
        
        Debug.Log($"💥 {gameObject.name} exploded!");
    }
    
    void OnDrawGizmos()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Draw ground check
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawRay(transform.position, Vector3.down * groundCheckDistance);
    }
}