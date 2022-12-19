using FishNet.Component.Animating;
using FishNet.Object;
using FishNet.Object.Prediction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [Header("Animations")]
    [SerializeField] private AnimationManager animationManager;
    [SerializeField] public AnimationState ANIM_IDLE;
    [SerializeField] public AnimationState ANIM_DIRECTION_SWITCH;
    [SerializeField] public AnimationState ANIM_SWIM;
    [SerializeField] public AnimationState ANIM_SWIM_SPECIAL;
    [SerializeField] private bool _debug_animate = true;

    [Header("In Hand")]
    [SerializeField] private Equipment inHand; //Can shoot when null (future: and with dual upgrades)
    [SerializeField] private bool canSwitchEquipment = true;

    private void Awake()
    {
        inHand = null;
        canSwitchEquipment = true;

        CreateAnimationStates();
    }

    private void CreateAnimationStates()
    {
        //Default State:
        ANIM_IDLE = new AnimationState("Player_Idle", null, 6f, 1);

        //Manager: (Links the Default Anim for you)
        animationManager = new AnimationManager(GetComponentInChildren<NetworkAnimator>(), ANIM_IDLE, GetAnimationState);

        //States:
        ANIM_DIRECTION_SWITCH = new AnimationState("Player_Direction_Switch", animationManager, 1, 1f);
        ANIM_SWIM = new AnimationState("Player_Swim", animationManager, 1 + (5/60f), 1.5f, 0.1f);
        ANIM_SWIM_SPECIAL = new AnimationState("Player_Swim_Special", animationManager, 1 + (5 / 6f), 1.3f);

        //Data:
        //randomMoveAnimationAfterTime = Random.Range(8, 15);
    }

    // Update is called once per frame
    private void Update()
    {
        if (!base.IsOwner)
            return;

        if (_debug_animate)
        {
            animationManager.PlayerAnimationStateUpdate();
        }

        PlayerInputUpdate();
        UseEquipment();
    }

    private void PlayerInputUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space)) //Equipment Bind
        {
            if (inHand != null)
            {
                TryDropEquipment(); //Drop
            }
            else
            {
                TrySetEquipment(); //Grab
            }
        }
    }

    /*
    private void PlayerInputUpdate()
    {
        //KEYBINDS HERE
        //xInput = Input.GetAxis("Horizontal");
        //yInput = Input.GetAxis("Vertical");

        if (Input.GetKeyDown(KeyCode.Space)) //Equipment Bind
        {
            if (inHand != null)
            {
                TryDropEquipment(); //Drop
            }
            else
            {
                TrySetEquipment(); //Grab
            }
        }

        UseEquipment();
    }
    private void PlayerMovementInputUpdate(out MoveData data)
    {
        data = default;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        if (h == 0 && v == 0)
            return;

        float rot_z = Mathf.Atan2(h, v) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.Euler(0f, 0f, rot_z - 90);

        data = new MoveData(h, v, rot);
    }
    */
    private void UseEquipment()
    {
        if (inHand == null)
        {
            //Head mining: (DONT IMPLEMENT - SHOOT ROCK TO MINE INSTEAD)
            return;
        }

        inHand.UseUpdate();
    }

    public bool TrySetEquipment()
    {
        if (!canSwitchEquipment)
        {
            return false;
        }

        Equipment equipment = null;
        foreach (Equipment e in WorldData.instance.GetEquipmentList())
        {
            if (e.IsPlayerInRange())
            {
                equipment = e;
            }
        }

        if (equipment == null)
        {
            return false;
        }

        inHand = equipment;
        inHand.Equip(transform);
        StartCoroutine(SwapEquipmentDelay());
        return true;
    }
    public bool TryDropEquipment()
    {
        if (!canSwitchEquipment)
        {
            return false;
        }

        inHand.Drop();
        inHand = null;
        StartCoroutine(SwapEquipmentDelay());
        return true;
    }
    IEnumerator SwapEquipmentDelay()
    {
        canSwitchEquipment = false;
        yield return new WaitForEndOfFrame();
        canSwitchEquipment = true;
    }



    /*float consecutiveMoveTime;
    float randomMoveAnimationAfterTime;
    [Replicate]
    private void PlayerMovementUpdate(MoveData moveData, bool asServer, bool replaying = false)
    {
        Vector3 moveVector = Vector3.ClampMagnitude(new Vector3(moveData.Horizontal, moveData.Vertical, 0), 1) * MOVE_SPEED;
        rb.velocity = moveVector;

        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
        {
            if (!ANIM_SWIM.HasTriggered())
            {
                ANIM_SWIM.Trigger();
            }

            consecutiveMoveTime += Time.deltaTime;
            if (consecutiveMoveTime > randomMoveAnimationAfterTime)
            {
                consecutiveMoveTime = 0;
                randomMoveAnimationAfterTime = Random.Range(5, 10);

                ANIM_SWIM_SPECIAL.Trigger();
            }

            //float rot_z = Mathf.Atan2(moveData.Horizontal, moveData.Vertical) * Mathf.Rad2Deg;
            //transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 90);
            //lookDirection = Quaternion.Euler(0f, 0f, rot_z - 90);

            //lastXInput = Input.GetAxis("Horizontal");
            //lastYInput = Input.GetAxis("Vertical");
        }
        else
        {
            consecutiveMoveTime = 0;
            if (ANIM_SWIM.HasTriggered())
            {
                ANIM_SWIM.Detrigger();
            }
        }
    }

    private void PlayerRotationUpdate(MoveData moveData, bool asServer, bool replaying = false)
    {
        if (Quaternion.Angle(transform.rotation, moveData.LookDirection) >= 120f)
        {
            ANIM_DIRECTION_SWITCH.Trigger();
            animationManager.PlayerAnimationStateUpdate(); //Force state update for better animation
            transform.rotation = moveData.LookDirection;
            return;
        }

        transform.rotation = Quaternion.Lerp(transform.rotation, moveData.LookDirection, 0.5f);
    }*/

    bool[] detriggerDelayBlockers = new bool[1]; //Each delayed detrigger must have a blocker ID to reach into this array
    private AnimationState GetAnimationState()
    {
        //Priorities:
        if (ANIM_DIRECTION_SWITCH.HasTriggered())
        {
            ANIM_DIRECTION_SWITCH.Detrigger();
            return LockState(ANIM_DIRECTION_SWITCH);
        }
        if (ANIM_SWIM_SPECIAL.HasTriggered())
        {
            AnimationStateDelayedDeTrigger(ref ANIM_SWIM_SPECIAL, blockerID: 0);
            return ANIM_SWIM_SPECIAL;
        }
        if (ANIM_SWIM.HasTriggered())
        {
            return ANIM_SWIM;
        }

        return ANIM_IDLE;


        AnimationState LockState(AnimationState pas)
        {
            animationManager.LockState(pas.GetPlayTime());
            return pas;
        }
    }

    private void AnimationStateDelayedDeTrigger(ref AnimationState a, int blockerID)
    {
        if (!detriggerDelayBlockers[blockerID])
        {
            detriggerDelayBlockers[blockerID] = true;
            StartCoroutine(a.DeTriggerDelayed());
            //a.Detrigger();
            StartCoroutine(DeTriggerManyCallDelay(a, blockerID));
        }
    }
    IEnumerator DeTriggerManyCallDelay(AnimationState a, int blockerID)
    {
        yield return new WaitForSeconds(a.GetPlayTime());

        detriggerDelayBlockers[blockerID] = false;
    }
}