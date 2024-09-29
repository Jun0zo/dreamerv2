using UnityEngine;
using System.Collections;

public class TracksControler : MonoBehaviour {

    private float offsetL = 0f;
    private float offsetR = 0f;
    public Renderer trackLeft;
    public Renderer trackRight;
    public Rigidbody Rig;
    private Vector3 vel;
    private float speed;
    private bool Front = false;
    private bool Back = false;
    private bool turn = true;

    // New variables to be controlled by agent
    public float moveInput = 0f; // Positive for forward, negative for backward
    public float turnInput = 0f; // Positive for right turn, negative for left turn

    void Start() {
        // Initialization if needed
    }

    // Updated to use moveInput and turnInput
    void HandleMovement() 
    {
        // Debug.Log("moveInput: " + moveInput);
        // Debug.Log("turnInput: " + turnInput);

        // Debug.Log("speed: " + speed);
        if (moveInput > 0.1f)
        {
            if (speed < 0.3f)
            {
                Front = true;
                Back = false;
            }
        }
        else if (moveInput < -0.1f)
        {
            if (speed < 0.3f)
            {
                Back = true;
                Front = false;
            }
        }

        // Tracks rotation
        if (turnInput != 0f && speed < 1.5f)
        {
            offsetL = offsetL + turnInput * 0.0002f;
            offsetR = offsetR - turnInput * 0.0002f;
            turn = true;
        }
        else
        {
            turn = false;
        }

        // Tracks move ,depends on current speed
        if (speed > 0 && !turn)
        {
            if (Front)
            {
                // Debug.Log("Front");
                offsetL = offsetL - speed / 10000;
                offsetR = offsetR - speed / 10000;
            }
            if (Back)
            {
                // Debug.Log("Back");
               
                offsetL = offsetL + speed / 10000;
                offsetR = offsetR + speed / 10000;
            }
        }

        // Speed 
        vel = Rig.velocity;
        speed = vel.magnitude;

        // scrolling
        trackLeft.material.SetTextureOffset("_MainTex", new Vector2(offsetL, 0));
        trackRight.material.SetTextureOffset("_MainTex", new Vector2(offsetR, 0));
    }

    void FixedUpdate()
    {
        HandleMovement(); // Always handle movement
    }
}
