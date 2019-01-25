using UnityEngine;
using System.Collections.Generic;

public class CharacterController : MonoBehaviour {

    public float moveSpeed = 2;
    public float moveAcceleration = 10;
    public float turnSpeed = 200;
    private Animator myAnimator;
    public Rigidbody myRigidbody;

    private float currentV = 0;
    private float currentH = 0;

    private bool isGrounded;
    private bool wasGrounded;
    private Vector3 currentDirection = Vector3.zero;
    private List<Collider> collisions = new List<Collider>();
    private Camera cam;

    public Camera CharacterCamera
    {
        get
        {
            if(cam == null) cam = Camera.main;
            return cam;
        }
    }

    private void Awake()
    {
        myAnimator = GetComponent<Animator>();
        myRigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        myAnimator.SetBool("Grounded", isGrounded);

        UpdateInput();
        wasGrounded = isGrounded;
    }

    private void UpdateInput()
    {
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");
        currentV = Mathf.Lerp(currentV, v, Time.deltaTime * moveAcceleration);
        currentH = Mathf.Lerp(currentH, h, Time.deltaTime * moveAcceleration);

        Vector3 moveDirection = Vector3.zero;
        if(CharacterCamera != null)
        {
            Transform camera = CharacterCamera.transform;
            moveDirection = camera.forward * currentV + camera.right * currentH;
        }

        float directionLength = moveDirection.magnitude;
        moveDirection.y = 0;
        moveDirection = moveDirection.normalized * directionLength;

        if(moveDirection != Vector3.zero)
        {
            currentDirection = Vector3.Slerp(currentDirection, moveDirection, Time.deltaTime * moveAcceleration);

            transform.rotation = Quaternion.LookRotation(currentDirection);
            transform.position += currentDirection * moveSpeed * Time.deltaTime;

            myAnimator.SetFloat("MoveSpeed", moveDirection.magnitude);
        }

        JumpingAndLanding();
    }

    private void JumpingAndLanding()
    {
        if(!wasGrounded && isGrounded)
        {
            myAnimator.SetTrigger("Land");
        }

        if(!isGrounded && wasGrounded)
        {
            myAnimator.SetTrigger("Jump");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        ContactPoint[] contactPoints = collision.contacts;
        for(int i = 0; i < contactPoints.Length; i++)
        {
            if (Vector3.Dot(contactPoints[i].normal, Vector3.up) > 0.5f)
            {
                if (!collisions.Contains(collision.collider)) {
                    collisions.Add(collision.collider);
                }
                isGrounded = true;
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        ContactPoint[] contactPoints = collision.contacts;
        bool validSurfaceNormal = false;
        for (int i = 0; i < contactPoints.Length; i++)
        {
            if (Vector3.Dot(contactPoints[i].normal, Vector3.up) > 0.5f)
            {
                validSurfaceNormal = true; break;
            }
        }

        if(validSurfaceNormal)
        {
            isGrounded = true;
            if (!collisions.Contains(collision.collider))
            {
                collisions.Add(collision.collider);
            }
        } else
        {
            if (collisions.Contains(collision.collider))
            {
                collisions.Remove(collision.collider);
            }
            if (collisions.Count == 0) { isGrounded = false; }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if(collisions.Contains(collision.collider))
        {
            collisions.Remove(collision.collider);
        }
        if (collisions.Count == 0) { isGrounded = false; }
    }

}
