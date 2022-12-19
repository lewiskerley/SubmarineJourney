using System.Collections;
using UnityEngine;

[System.Serializable]
public class AnimationState
{
    public int animHash { get; private set; }
    [SerializeField] private float animLengthSeconds;
    public float animSpeedMultiplier { get; private set; }
    public float animTransitionToSmoothing { get; private set; }
    [SerializeField] private float animPlayTime;
    [SerializeField] private bool queuePlay;
    private AnimationManager animManager;

    public AnimationState(string animName, AnimationManager manager, float lengthSeconds = 1, float speedMult = 1, float transitionToSmoothing = 0)
    {
        animHash = Animator.StringToHash(animName);
        animManager = manager;
        animLengthSeconds = lengthSeconds;
        animSpeedMultiplier = speedMult;
        animTransitionToSmoothing = transitionToSmoothing;
        animPlayTime = animLengthSeconds / animSpeedMultiplier;
        queuePlay = false;
    }

    public void SetAnimationManager(AnimationManager manager)
    {
        animManager = manager;
    }

    public void Trigger()
    {
        if (animManager.currentAnimationState.Equals(this))
        {
            return;
        }
        //Debug.Log(animPlayTime + " TRIGGERED");
        queuePlay = true;
    }

    public bool HasTriggered()
    {
        return queuePlay;
    }

    public void Detrigger()
    {
        //Debug.Log(animPlayTime + " DETRIGGERED");
        queuePlay = false;
    }

    public IEnumerator DeTriggerDelayed()
    {
        yield return new WaitForSeconds(GetPlayTime());

        Detrigger();
    }

    public float GetPlayTime()
    {
        return animPlayTime;
    }

    public override bool Equals(object obj)
    {
        if (obj.GetType() != typeof(AnimationState))
        {
            return false;
        }
        return animHash == ((AnimationState)obj).animHash;
    }

    public override int GetHashCode()
    {
        return animHash;
    }
}