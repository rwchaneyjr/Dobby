using UnityEngine;

public class SimplePlayerMove : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float turnSpeed = 120f;

    void Update()
    {
        float move = 0f;
        float turn = 0f;

        if (Input.GetKey(KeyCode.W)) move = 1f;
        if (Input.GetKey(KeyCode.S)) move = -1f;

        if (Input.GetKey(KeyCode.A)) turn = -1f;
        if (Input.GetKey(KeyCode.D)) turn = 1f;

        // Move forward/back
        transform.Translate(Vector3.forward * move * moveSpeed * Time.deltaTime);

        // Turn left/right
        transform.Rotate(Vector3.up * turn * turnSpeed * Time.deltaTime);
    }
}