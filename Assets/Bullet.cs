using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 20f;
    public float lifetime = 4f;
    public int damage = 20; // 20 damage per bullet (enemy has 100 health = 5 bullets needed)
    
    [HideInInspector]
    public float direction = 1;
    
    void Start()
    {
        // Set the tag so enemy can detect it
        gameObject.tag = "PlayerBullet";
        
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        rb.useGravity = false;
        rb.linearVelocity = new Vector3(direction * speed, 0, 0);
        
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
        
        Debug.Log("🔵 Player bullet created and tagged!");
    }
    
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"💥 Bullet hit: {other.gameObject.name} (Tag: {other.tag})");
        
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            AdvancedRobotEnemy advancedEnemy = other.GetComponent<AdvancedRobotEnemy>();
            
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log($"🎯 Bullet damaged enemy for {damage} damage!");
            }
            else if (advancedEnemy != null)
            {
                advancedEnemy.TakeDamage(damage);
                Debug.Log($"🎯 Bullet damaged advanced enemy for {damage} damage!");
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
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            AdvancedRobotEnemy advancedEnemy = collision.gameObject.GetComponent<AdvancedRobotEnemy>();
            
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            else if (advancedEnemy != null)
            {
                advancedEnemy.TakeDamage(damage);
            }
            
            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}