using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerNameDisplayer : NetworkBehaviour
{
    [SerializeField]
    private TMP_Text textField;

    public override void OnStartClient()
    {
        base.OnStartClient();
        ShowNames(false); //Hide names on START
        SetName(); //SetName on START
        PlayerNameTracker.OnNameChange += PlayerNameTracker_OnNameChange;
    }
    public override void OnStopClient()
    {
        base.OnStopClient();
        PlayerNameTracker.OnNameChange -= PlayerNameTracker_OnNameChange;
    }

    private void PlayerNameTracker_OnNameChange(NetworkConnection nc, string arg)
    {
        if (nc != base.Owner)
            return;

        SetName(); //SetName on NAME CHANGE
    }

    public override void OnOwnershipClient(NetworkConnection prevOwner)
    {
        base.OnOwnershipClient(prevOwner);
        SetName();//SetName on PLAYER CHANGE
    }

    private void SetName()
    {
        string result = null;

        if (base.Owner.IsValid)
            result = PlayerNameTracker.GetPlayerName(base.Owner);

        if (string.IsNullOrEmpty(result))
            result = "Invalid Name";

        textField.text = result;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ShowNames(true);
        }
        else if (Input.GetKeyUp(KeyCode.Tab))
        {
            ShowNames(false);
        }
    }

    private void ShowNames(bool show)
    {
        textField.enabled = show;
    }
}
