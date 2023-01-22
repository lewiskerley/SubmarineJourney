using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Equipment : MonoBehaviour
{
    protected Transform followTransform;
    protected PlayerStateMachine curPlayerHolding;
    public float cooldownTime;
    private float timeLeft;
    private bool playerInRange;

    public void Awake()
    {
        playerInRange = false;
        timeLeft = cooldownTime;

        followTransform = null;
        curPlayerHolding = null;
    }

    public bool IsPlayerInRange()
    {
        return playerInRange;
    }

    //Single Player atm
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            playerInRange = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            playerInRange = false;
        }
    }

    public void Equip(Transform player)
    {
        Transform mesh = player.transform.Find("PlayerMesh");
        if (mesh == null)
        {
            Debug.LogError("[Developer Mistake]: PLAYER MESH (1 with the animator) MUST HAVE THE NAME: PlayerMesh");
        }

        //Debug.Log("Picked up");
        curPlayerHolding = player.GetComponent<PlayerStateMachine>();
        followTransform = mesh;
        transform.SetParent(followTransform);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
    public void Drop()
    {
        curPlayerHolding = null;
        followTransform = null;
        transform.SetParent(followTransform);
        transform.localRotation = Quaternion.identity;
    }

    public abstract void UseWithCooldown();

    public void UseUpdate()
    {
        if (timeLeft <= 0)
        {
            UseWithCooldown();
            timeLeft = cooldownTime;
            return;
        }

        timeLeft -= Time.deltaTime;
    }

    public abstract EquipmentItems GetItemType();
}


public enum EquipmentItems
{
    Empty,
    Drill
}
