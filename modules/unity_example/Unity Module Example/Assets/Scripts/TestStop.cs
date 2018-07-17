using UnityEngine;

public class TestStop : MonoBehaviour
{

    [HideInInspector] public bool isMoving = false;
    public static TestStop instance;

    void Awake()
    {
        if(instance==null) instance = this;
    }
    
    void Start()
    {
        GameServer.Instance.OnMovement(MovementData);
    }

    private void MovementData(Vector3 acceleration, Vector3 velocity)
    {
        velocity.y = 0f;
        
        if (velocity.magnitude == 0f)
        {
            Debug.Log("Not Moving");
            isMoving = false;
        }
        else
        {
            Debug.Log("Is Moving!");
            isMoving = true;
        }
    }
}