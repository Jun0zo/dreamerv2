using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

    public Rigidbody Rigid;
    public float speedPower; // engine power
    public Transform centerOfmass;
    private float torque = 100f;
    private Vector3 vel;
    public float currentSpeed; // actual tank speed
    public float maxSpeed = 2.5f; // maximal tank speed

    public float turnInput = 0f;   // 좌우 회전
    public float moveInput = 0f;   // 전후 이동

    public void InitializeAgent()
    {
        // set position
        transform.localPosition = new Vector3(-223f, 4f, 5.5f);

        // set rotation
        transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
    
    }


	void Start () {

        InitializeAgent();

        // set centre of mass
        Rigid.centerOfMass = centerOfmass.localPosition;
        // max rotation speed
        Rigid.maxAngularVelocity = 0.6f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

    }

    void FixedUpdate()
    {

        // float turn = Input.GetAxis("Horizontal");
        // float moveVertical = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(0.0f, 0.0f, moveInput);

        if (currentSpeed < maxSpeed)
        {
            Rigid.AddRelativeForce(movement * speedPower); 
        }

        Rigid.AddTorque(transform.up * torque * turnInput);

    }


    void Update () {

        if (currentSpeed > 1.0f)
        {
            torque = 50f;
        }
        else
        {
            torque = 100f;
        }
        vel = Rigid.velocity;
        currentSpeed = vel.magnitude;

    }
}
