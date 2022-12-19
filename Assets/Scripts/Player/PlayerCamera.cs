using Cinemachine;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : NetworkBehaviour
{
    public override void OnStartClient()
    {
        base.OnStartClient();
        return;

        if (base.IsOwner)
        {
            GameObject camObj = GameObject.FindGameObjectWithTag("PlayerCameras").transform
                .GetChild(0).gameObject; //Single cam atm

            camObj.GetComponent<CinemachineVirtualCamera>().Follow = transform;
            camObj.SetActive(true);
        }
    }
}
