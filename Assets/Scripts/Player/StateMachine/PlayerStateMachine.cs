using FishNet;
using FishNet.Object;
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
    private EquipmentItems _equippedItemType;
    private Equipment _equippedItem;

    // magic numbers
    private int _zero = 0;

    // extras
    private bool _canSwitchEquipment = true;

    // applied movement variables
    private Vector2 _moveVector;
    private Vector2 _directionVector;
    private float _moveSpeed = 5.5f;
    private Rigidbody rb;

    // animations
    private Animator _anim;
    private int _animIdleHash;
    private int _animWalkHash;
    private int _animFlipHash;

    // public getters / setters
    public PlayerBaseState CurrentState { get { return _currentState; } set { _currentState = value; } }
    public bool IsMovementPressed { get { return _isMovementPressed; } set { _isMovementPressed = value;} }
    public bool EquippedEmpty { get { return _equippedItemType == EquipmentItems.Empty; } }
    public bool EquippedDrill { get { return _equippedItemType == EquipmentItems.Drill; } }
    public Animator Anim { get { return _anim; } }
    public int AnimIdleHash { get { return _animIdleHash; } }
    public int AnimWalkHash { get { return _animWalkHash; } }
    public int AnimFlipHash { get { return _animFlipHash; } }
    public Vector2 MoveVector { get { return _moveVector; } set { _moveVector = value; } }
    public Vector2 DirectionVector { get { return _directionVector; } set { _directionVector = value; } }
    public Vector2 MoveInput { get { return _currentMovementInput; } }
    public float MoveSpeed { get { return _moveSpeed; } }


    private void Awake()
    {
        _moveSpeed = 5.5f;
        _directionVector = Vector2.up;
        _moveVector = Vector2.zero;
        _playerControls = new PlayerControls();

        _anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();

        _animIdleHash = Animator.StringToHash("Player_Idle");
        _animWalkHash = Animator.StringToHash("Player_Swim");
        _animFlipHash = Animator.StringToHash("Player_Direction_Switch");

        _states = new PlayerStateFactory(this);
        _currentState = _states.EmptyHand();
        _equippedItemType = EquipmentItems.Empty;
        _equippedItem = null;
        _canSwitchEquipment = true;
        _currentState.EnterState();

        _playerControls.Map.Move.started += OnMovementInput;
        _playerControls.Map.Move.canceled += OnMovementInput;
        _playerControls.Map.Move.performed += OnMovementInput;
        _playerControls.Map.Equip.started += OnEquipKeyDown;

        //InstanceFinder.TimeManager.OnTick += Move;
        //InstanceFinder.TimeManager.OnPostTick += Move;
    }

    void OnMovementInput(InputAction.CallbackContext context)
    {
        _currentMovementInput = context.ReadValue<Vector2>();
        _isMovementPressed = _currentMovementInput.x != _zero || _currentMovementInput.y != _zero;
    }
    void OnEquipKeyDown(InputAction.CallbackContext context)
    {
        if (_equippedItemType != EquipmentItems.Empty) // Drop Item
        {
            TryDropEquipment();
        }
        else if (_equippedItemType == EquipmentItems.Empty) // Pickup Item
        {
            TrySetEquipment();
        }
    }

    private void Move()
    {
        rb.velocity = new Vector3(_moveVector.x, _moveVector.y, 0);
    }

    private void PlayerRotationUpdate()
    {
        float rot_z = Mathf.Atan2(_directionVector.y, _directionVector.x) * Mathf.Rad2Deg;
        Quaternion lookDirection = Quaternion.Euler(0f, 0f, rot_z - 90);

        transform.rotation = Quaternion.Lerp(transform.rotation, lookDirection, 0.05f);
    }

    private void Update()
    {
        _currentState.UpdateStates();

        Move();
        PlayerRotationUpdate();
    }

    public void UseEquipmentUpdate()
    {
        if (_equippedItem != null)
        {
            _equippedItem.UseUpdate();
        }
    }

    public bool TrySetEquipment()
    {
        if (!_canSwitchEquipment)
        {
            return false;
        }

        // TODO: Make this work when I redo world storage!
        Equipment equipment = null;
        foreach (Equipment e in WorldData.instance.GetEquipmentList())
        {
            if (e.IsPlayerInRange())
            {
                equipment = e;
                break;
            }
        }

        if (equipment == null)
        {
            return false;
        }

        _equippedItem = equipment;
        _equippedItem.Equip(transform);
        _equippedItemType = equipment.GetItemType();
        StartCoroutine(SwapEquipmentDelay());
        return true;
    }
    public bool TryDropEquipment()
    {
        if (!_canSwitchEquipment)
        {
            return false;
        }

        _equippedItemType = EquipmentItems.Empty;
        _equippedItem.Drop();
        _equippedItem = null;

        StartCoroutine(SwapEquipmentDelay());
        return true;
    }
    IEnumerator SwapEquipmentDelay()
    {
        _canSwitchEquipment = false;
        yield return new WaitForEndOfFrame();
        _canSwitchEquipment = true;
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
