using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Runtime.CompilerServices;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using Cinemachine;

public class PlayerBehavior : SerializedMonoBehaviour
{
    static private PlayerBehavior instance;
    static public PlayerBehavior Instance { get { return instance; } }

    [SerializeField] private float moveSpeed;
    [SerializeField, Range(0f, 1f)] private float accLerp;
    [SerializeField] private float jumpPower;
    [SerializeField] private LayerMask groundLayermask;

    public enum PlayerState
    {
        NormalControl,
        Slingshot,
        CameraCapture
    }

    private bool isControlEnabled = false;
    public bool IsControlEnabled { get { return isControlEnabled; } set { IsControlEnabled = value; } }

    [SerializeField] private PlayerState currentControlState = PlayerState.NormalControl;
    [SerializeField] private SphereCollider groundCheckCollider;
    [SerializeField] private Transform armatureTransform;
    [SerializeField] private Animator armatureAnimator;
    [SerializeField] private CinemachineFreeLook NormalActionVCam;
    [SerializeField] private CinemachineFreeLook CaptureActionVCam;
    [SerializeField] private CinemachineFreeLook SlingshotActionVCam;

    public delegate void ONPlayerStateChanged();
    [HideInInspector] public ONPlayerStateChanged OnPlayerStateChanged;

    Rigidbody rbody;

    PlayerInputActions input;
    RaycastHit[] GroundCache;
    private bool IsGrounded { get { return GroundCache.Length > 0; } }


    public void ChangePlayerState(PlayerState _playerState)
    {
        currentControlState = _playerState;
        if (OnPlayerStateChanged != null)
        {
            OnPlayerStateChanged.Invoke();
        }
    }

    private void Awake()
    {
        if (instance == null) instance = this;

        rbody = GetComponent<Rigidbody>();

        input = new PlayerInputActions();
        input.Enable();
    }

    private void OnEnable()
    {
        input.Player.Jump.performed += Jump;
        input.Player.Aim.started += AimDown;
        input.Player.Aim.canceled += AimUp;
    }

    private void OnDisable()
    {
        input.Player.Jump.performed -= Jump;
        input.Player.Aim.started -= AimDown;
        input.Player.Aim.canceled -= AimUp;
    }

    private void FixedUpdate()
    {
        GroundCache = GetGroundHit();

        if (input.Player.Move.IsPressed())
        {
            Vector2 Inputdirection = input.Player.Move.ReadValue<Vector2>();
            Vector3 direction = Camera.main.transform.TransformDirection(new Vector3(Inputdirection.x, 0f, Inputdirection.y));

            direction = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;

            rbody.velocity =
                Vector3.Lerp(rbody.velocity,
                new Vector3(direction.x * moveSpeed, rbody.velocity.y, direction.z * moveSpeed),
                accLerp);

            Vector3 planedDirection = new Vector3(direction.x, 0, direction.z);
            armatureTransform.localRotation =
                Quaternion.RotateTowards(
                    armatureTransform.localRotation,
                    Quaternion.LookRotation(planedDirection, Vector3.up),
                    10f);

            armatureAnimator.SetBool("RunKey", true);
        }
        else
        {
            armatureAnimator.SetBool("RunKey", false);
        }


    }

    private void Jump(InputAction.CallbackContext context)
    {
        Debug.Log("Jump Ready");

        if (currentControlState == PlayerState.NormalControl)
        {
            if (IsGrounded)
            {
                rbody.velocity = new Vector3(rbody.velocity.x, jumpPower, rbody.velocity.z);
                Debug.Log("Jump Performed");
            }
        }
    }

    private void AimDown(InputAction.CallbackContext context)
    {
        if (currentControlState == PlayerState.NormalControl)
        {
            ChangePlayerState(PlayerState.CameraCapture);
            CaptureActionVCam.gameObject.SetActive(true);
            CaptureActionVCam.m_XAxis = NormalActionVCam.m_XAxis;
            CaptureActionVCam.m_YAxis = NormalActionVCam.m_YAxis;
            NormalActionVCam.gameObject.SetActive(false);
            HUDManager.Instance.SetCapturemodHUD(true);
        }

    }

    private void AimUp(InputAction.CallbackContext context)
    {
        if (currentControlState == PlayerState.CameraCapture)
        {
            ChangePlayerState(PlayerState.NormalControl);
            NormalActionVCam.gameObject.SetActive(true);
            NormalActionVCam.m_XAxis = CaptureActionVCam.m_XAxis;
            NormalActionVCam.m_YAxis = CaptureActionVCam.m_YAxis;
            CaptureActionVCam.gameObject.SetActive(false);
            HUDManager.Instance.SetCapturemodHUD(false);
        }
    }

    private RaycastHit[] GetGroundHit()
    {
        return Physics.SphereCastAll(groundCheckCollider.center, groundCheckCollider.radius, Vector3.down, groundCheckCollider.radius,groundLayermask);
    }


}
