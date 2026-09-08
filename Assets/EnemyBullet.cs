using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [Header("Enemy Bullet Settings")]
    public float speed = 12f;
    public float lifetime = 5f;
    public int damage = 34; // 34 damage per bullet (player has 100 health = 3 bullets needed)
    
    [HideInInspector]
    public float direction = 1;
    
    private Vector3 targetDirection;
    private bool hasTarget = false;
    
    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        rb.useGravity = false;
        
        // Find player and shoot directly at them
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            targetDirection = (player.transform.position - transform.position).normalized;
            rb.linearVelocity = targetDirection * speed;
            hasTarget = true;
            Debug.Log($"🔴 EnemyBullet created - shooting toward player at direction: {targetDirection}");
        }
        else
        {
            // Fallback: use the direction parameter
            rb.linearVelocity = new Vector3(direction * speed, 0, 0);
            Debug.Log($"🔴 EnemyBullet created - no player found, using direction: {direction}");
        }
        
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            SphereCollider sphereCol = gameObject.AddComponent<SphereCollider>();
            sphereCol.isTrigger = true;
            sphereCol.radius = 0.1f;
        }
        else
        {
            col.isTrigger = true;
        }
        
        Destroy(gameObject, lifetime);
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Debug.Log($"🔴 Enemy bullet hit player!");
            }
            Destroy(gameObject);
        }
        else if (other.CompareTag("Ground") || other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}