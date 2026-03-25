using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f; // movement speed
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // get Rigidbody2D
    }

    void Update()
    {
        // get input (-1 to 1)
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        // apply movement
        rb.linearVelocity = new Vector2(moveX * speed, moveY * speed);
    }
}