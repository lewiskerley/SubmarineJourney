using FishNet;
using FishNet.Object;
using FishNet.Object.Prediction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerMovement : NetworkBehaviour
{
    private Player PLAYER;

    [Header("Movement")]
    private Rigidbody rb;
    [SerializeField] private float MOVE_SPEED = 5.5f;
    //[SerializeField] private float xInput;
    //[SerializeField] private float yInput;
    //[SerializeField] private Quaternion lookDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        PLAYER = GetComponent<Player>();
        //lookDirection = Quaternion.Euler(0, 0, 0);
        transform.rotation = Quaternion.Euler(0, 0, 0);

        InstanceFinder.TimeManager.OnTick += TimeManager_OnTick;
        InstanceFinder.TimeManager.OnPostTick += TimeManager_OnPostTick;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        SetupClient();
    }
    private void SetupClient()
    {
        //Makes all other clients that arent the server a kinematic. + Server only
        rb.isKinematic = (!base.IsOwner && !base.IsServer) || base.IsServerOnly; //Not needed

        //Removes collider from all "other" players
        GetComponent<Collider>().isTrigger = !base.IsOwner;
    }

    private void OnDestroy()
    {
        if (InstanceFinder.TimeManager != null)
        {
            InstanceFinder.TimeManager.OnTick -= TimeManager_OnTick;
            InstanceFinder.TimeManager.OnPostTick -= TimeManager_OnPostTick;
        }
    }

    private struct MoveData
    {
        public float Horizontal;
        public float Vertical;

        public MoveData(float h, float v)
        {
            Horizontal = h;
            Vertical = v;
        }
    }
    private struct ReconcileMovementData
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Velocity;
        public Vector3 AngularVelocity;

        public ReconcileMovementData(Vector3 pos, Quaternion rot, Vector3 vel, Vector3 angVel)
        {
            Position = pos;
            Rotation = rot;
            Velocity = vel;
            AngularVelocity = angVel;
        }
    }

    private void TimeManager_OnTick()
    {
        if (base.IsOwner)
        {
            Reconcililation(default, false);

            GatherInputs(out MoveData data);
            Move(data, false);
        }

        if (base.IsServer)
        {
            Move(default, true);
        }
    }
    private void TimeManager_OnPostTick()
    {
        if (base.IsServer)
        {
            ReconcileMovementData data = new ReconcileMovementData(transform.position, transform.rotation, rb.velocity, rb.angularVelocity);
            Reconcililation(data, true);
        }
    }

    private void GatherInputs(out MoveData data)
    {
        data = default;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        if (h == 0 && v == 0)
            return;

        data = new MoveData(h, v);
    }

    [Replicate]
    private void Move(MoveData moveData, bool asServer, bool replaying = false)
    {
        Vector3 moveVector = Vector3.ClampMagnitude(new Vector3(moveData.Horizontal, moveData.Vertical, 0), 1) * MOVE_SPEED;
        rb.velocity = moveVector;
        //rb.AddForce(moveVector, ForceMode.VelocityChange);

        if (moveData.Horizontal != 0 || moveData.Vertical != 0)
        {
            //Swim Animation:
            SwimAnimationPlayChecker(isMoving: true);

            //Rotation
            PlayerRotationUpdate(moveData);
        }
        else
        {
            //Swim Animation:
            SwimAnimationPlayChecker(isMoving: false);
        }
    }

    Quaternion animator_TurnRotation;
    private void PlayerRotationUpdate(MoveData moveData)
    {
        float rot_z = Mathf.Atan2(moveData.Vertical, moveData.Horizontal) * Mathf.Rad2Deg;
        Quaternion lookDirection = Quaternion.Euler(0f, 0f, rot_z - 90);

        if (Quaternion.Angle(transform.rotation, lookDirection) >= 120f)
        {
            animator_TurnRotation = lookDirection;
            PLAYER.ANIM_DIRECTION_SWITCH.Trigger();
            //animationManager.PlayerAnimationStateUpdate(); //Force state update for better animation
            //transform.rotation = lookDirection; //Called from animator now
            return;
        }

        transform.rotation = Quaternion.Lerp(transform.rotation, lookDirection, 0.5f);
    }

    public void Animator_SetTurnRotation()
    {
        transform.rotation = animator_TurnRotation;
    }

    float consecutiveMoveTime = 0;
    float randomMoveAnimationAfterTime = 5;
    private void SwimAnimationPlayChecker(bool isMoving)
    {
        if (isMoving)
        {
            if (!PLAYER.ANIM_SWIM.HasTriggered())
            {
                PLAYER.ANIM_SWIM.Trigger();
            }
            consecutiveMoveTime += Time.fixedDeltaTime;
            if (consecutiveMoveTime > randomMoveAnimationAfterTime)
            {
                consecutiveMoveTime = 0;
                randomMoveAnimationAfterTime = Random.Range(5, 10);

                PLAYER.ANIM_SWIM_SPECIAL.Trigger();
            }
        }
        else
        {
            consecutiveMoveTime = 0;
            if (PLAYER.ANIM_SWIM.HasTriggered())
            {
                PLAYER.ANIM_SWIM.Detrigger();
            }
        }
    }

    [Reconcile]
    private void Reconcililation(ReconcileMovementData data, bool asServer)
    {
        transform.position = data.Position;
        transform.rotation = data.Rotation;
        rb.velocity = data.Velocity;
        rb.angularVelocity = data.AngularVelocity;
    }
}
