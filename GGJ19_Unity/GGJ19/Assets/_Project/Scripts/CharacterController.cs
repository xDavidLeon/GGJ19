using UnityEngine;
using System.Collections.Generic;

public class CharacterController : MonoBehaviour {

    public float moveSpeed = 2;
    public float turnSpeed = 200;
    private Animator myAnimator;
    public Rigidbody myRigidbody;

    private float currentV = 0;
    private float currentH = 0;

    private readonly float interpolation = 10;
    private readonly float walkScale = 0.33f;
    private readonly float backwardsWalkScale = 0.16f;
    private readonly float backwardRunScale = 0.66f;

    private bool isGrounded;
    private bool wasGrounded;
    private Vector3 currentDirection = Vector3.zero;
    private List<Collider> collisions = new List<Collider>();

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

        Transform camera = Camera.main.transform;

        if(Input.GetKey(KeyCode.LeftShift))
        {
            v *= walkScale;
            h *= walkScale;
        }

        currentV = Mathf.Lerp(currentV, v, Time.deltaTime * interpolation);
        currentH = Mathf.Lerp(currentH, h, Time.deltaTime * interpolation);

        Vector3 direction = camera.forward * currentV + camera.right * currentH;

        float directionLength = direction.magnitude;
        direction.y = 0;
        direction = direction.normalized * directionLength;

        if(direction != Vector3.zero)
        {
            currentDirection = Vector3.Slerp(currentDirection, direction, Time.deltaTime * interpolation);

            transform.rotation = Quaternion.LookRotation(currentDirection);
            transform.position += currentDirection * moveSpeed * Time.deltaTime;

            myAnimator.SetFloat("MoveSpeed", direction.magnitude);
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
