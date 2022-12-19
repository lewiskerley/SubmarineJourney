using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateMachine : MonoBehaviour
{
    // state variables
    PlayerBaseState _currentState;
    PlayerStateFactory _states;

    // variables to store player input
    private PlayerControls _playerControls;
    private Vector2 _currentMovementInput;
    private bool _isMovementPressed;

    // magic numbers
    private int _zero = 0;

    // applied movement variables
    private Vector2 _moveVector;
    private float _moveSpeed = 5.5f;
    private Rigidbody rb;

    // animations
    private Animator _anim;
    private int _animIdleHash;
    private int _animWalkHash;

    // public getters / setters
    public PlayerBaseState CurrentState { get { return _currentState; } set { _currentState = value; } }
    public bool IsMovementPressed { get { return _isMovementPressed; } set { _isMovementPressed = value;} }
    public Animator Anim { get { return _anim; } }
    public int AnimIdleHash { get { return _animIdleHash; } }
    public int AnimWalkHash { get { return _animWalkHash; } }
    public Vector2 MoveVector { get { return _moveVector; } set { _moveVector = value; } }
    public Vector2 MoveInput { get { return _currentMovementInput; } }
    public float MoveSpeed { get { return _moveSpeed; } }


    private void Awake()
    {
        _states = new PlayerStateFactory(this);
        _currentState = _states.Idle();
        _currentState.EnterState();

        _playerControls = new PlayerControls();
        _anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();

        _animIdleHash = Animator.StringToHash("Player_Idle");
        _animWalkHash = Animator.StringToHash("Player_Swim");

        _playerControls.Map.Move.started += OnMovementInput;
        _playerControls.Map.Move.canceled += OnMovementInput;
        _playerControls.Map.Move.performed += OnMovementInput;
    }

    void OnMovementInput(InputAction.CallbackContext context)
    {
        _currentMovementInput = context.ReadValue<Vector2>();
        _isMovementPressed = _currentMovementInput.x != _zero || _currentMovementInput.y != _zero;
    }

    private void Move()
    {
        rb.velocity = _moveVector;
    }

    private void PlayerRotationUpdate()
    {
        float rot_z = Mathf.Atan2(_moveVector.y, _moveVector.x) * Mathf.Rad2Deg;
        Quaternion lookDirection = Quaternion.Euler(0f, 0f, rot_z - 90);

        if (Quaternion.Angle(transform.rotation, lookDirection) >= 120f)
        {
            //animator_TurnRotation = lookDirection;
            //PLAYER.ANIM_DIRECTION_SWITCH.Trigger();

            //TODO: SWITCH ROTATION AFTER ANIMATION
            //transform.rotation = animator_TurnRotation; //Called from animator now
            return;
        }

        transform.rotation = Quaternion.Lerp(transform.rotation, lookDirection, 0.5f);
    }

    private void Update()
    {
        _currentState.UpdateStates();

        Move();
        PlayerRotationUpdate();
    }

    private void OnEnable()
    {
        _playerControls.Enable();
    }
    private void OnDisable()
    {
        _playerControls.Disable();
    }
}
