using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Runtime.CompilerServices;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class PlayerBehavior : SerializedMonoBehaviour
{
    private PlayerBehavior instance;
    public PlayerBehavior Instance { get { return instance; } }

    [SerializeField] private float moveSpeed;
    [SerializeField, Range(0f, 1f)] private float accLerp;
    [SerializeField] private float jumpPower;

    [SerializeField] private SphereCollider groundCheckCollider;
    Rigidbody rbody;
    Animator animator;

    PlayerInputActions input;

    RaycastHit[] GroundCache;
    private bool IsGrounded { get { return GroundCache.Length > 0; }}


    private void Awake()
    {
        if(instance == null) instance = this;

        rbody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        input = new PlayerInputActions();
        input.Enable();

    }

    private void OnEnable()
    {
        input.Player.Jump.performed += Jump;
    }

    private void FixedUpdate()
    {
        GroundCache = GetGroundHit();

        if (input.Player.Move.IsPressed())
        {
            Vector2 Inputdirection = input.Player.Move.ReadValue<Vector2>();
            Vector3 direction = Camera.main.transform.TransformDirection(new Vector3(Inputdirection.x,0f,Inputdirection.y));

            direction = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;

            rbody.velocity = Vector3.Lerp(rbody.velocity, new Vector3(direction.x * moveSpeed, rbody.velocity.y, direction.z * moveSpeed),accLerp);

            Vector3 planedDirection = new Vector3(direction.x, 0, direction.z);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(planedDirection, Vector3.up), 10f);

            animator.SetBool("RunKey", true);
        }
        else
        {
            animator.SetBool("RunKey", false);
        }
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if(IsGrounded)
        {
            rbody.velocity = new Vector3(rbody.velocity.x, jumpPower, rbody.velocity.z);
        }
    }

    private RaycastHit[] GetGroundHit()
    {
        return Physics.SphereCastAll(groundCheckCollider.center, groundCheckCollider.radius, Vector3.down, groundCheckCollider.radius);
    }


}
