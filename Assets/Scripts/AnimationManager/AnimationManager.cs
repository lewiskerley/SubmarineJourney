using FishNet.Component.Animating;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager
{
    private NetworkAnimator networkAnimator;
    private Animator animator;
    public AnimationState currentAnimationState { get; private set; }
    private float animationLockedTill;
    public delegate AnimationState GetAnimationState();
    public GetAnimationState getAnimationState;

    public AnimationManager(NetworkAnimator networkAnimator, AnimationState defaultState, GetAnimationState getAnimationState)
    {
        this.networkAnimator = networkAnimator;
        if (networkAnimator == null)
        {
            Debug.LogError("No NetworkAnimator supplied!");
        }

        this.animator = networkAnimator.Animator;
        if (animator == null)
        {
            Debug.LogError("No Animator in NetworkAnimator!");
        }

        this.getAnimationState = getAnimationState;
        if (getAnimationState == null)
        {
            Debug.LogError("No GetAnimationState delegate supplied!");
        }

        currentAnimationState = new AnimationState("", null);
        defaultState.SetAnimationManager(this);
        defaultState.Trigger();
    }

    //Call each frame
    public void PlayerAnimationStateUpdate()
    {
        if (Time.time < animationLockedTill) return;

        AnimationState state = getAnimationState();
        //Debug.Log("PLAYING: " + state.GetPlayTime());

        if (state.Equals(currentAnimationState)) return;
        animator.speed = state.animSpeedMultiplier;
        networkAnimator.CrossFade(state.animHash, currentAnimationState.animTransitionToSmoothing, 0);
        currentAnimationState = state;
    }

    public void LockState(float lockTime)
    {
        animationLockedTill = Time.time + lockTime;
    }
}
