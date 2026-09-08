using UnityEngine;

public class CollectableItem : MonoBehaviour
{
    [Header("Item Settings")]
    public ItemType itemType = ItemType.Coin;
    public int value = 1;
    public bool autoRotate = true;
    public float rotationSpeed = 90f;
    
    [Header("Effects")]
    public bool enableGlow = true;
    public bool enableFloat = true;
    public float floatHeight = 0.5f;
    public float floatSpeed = 2f;
    
    public enum ItemType
    {
        Coin,
        Crystal,
        HealthPickup
    }
    
    private Vector3 startPosition;
    
    void Start()
    {
        startPosition = transform.position;
        Debug.Log($"✨ {itemType} spawned! Value: {value}");
    }
    
    void Update()
    {
        // Rotate the item
        if (autoRotate)
        {
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        }
        
        // Float up and down
        if (enableFloat)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                switch (itemType)
                {
                    case ItemType.Coin:
                        Debug.Log($"🪙 Coin collected! +{value}");
                        player.AddCoins(value);
                        break;
                        
                    case ItemType.Crystal:
                        Debug.Log($"💎 Crystal collected! +{value} coins!");
                        player.AddCoins(value);
                        break;
                        
                    case ItemType.HealthPickup:
                        Debug.Log($"❤️ Health pickup! +{value} HP!");
                        player.Heal(value);
                        break;
                }
            }
            
            // Destroy the item
            Destroy(gameObject);
        }
    }
}