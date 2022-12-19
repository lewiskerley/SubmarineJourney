using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimatorHelper : MonoBehaviour
{
    PlayerMovement playerMovement;

    private void Awake()
    {
        playerMovement = GetComponentInParent<PlayerMovement>();
        if (playerMovement == null)
        {
            Debug.LogError("No PlayerMovement Found on Player!");
        }
    }

    private void Animator_SetTurnRotation()
    {
        playerMovement.Animator_SetTurnRotation();
    }
}
