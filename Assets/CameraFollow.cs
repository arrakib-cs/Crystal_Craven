using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform player;
    public Vector3 offset = new Vector3(0, 5, -12);
    public float smoothSpeed = 2f;
    public float lookAheadDistance = 3f;
    
    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }
    
    void LateUpdate()
    {
        if (player != null)
        {
            Vector3 lookAhead = Vector3.zero;
            if (player.localScale.x > 0)
                lookAhead = Vector3.right * lookAheadDistance;
            else
                lookAhead = Vector3.left * lookAheadDistance;
            
            Vector3 desiredPosition = player.position + offset + lookAhead;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;
        }
    }
}