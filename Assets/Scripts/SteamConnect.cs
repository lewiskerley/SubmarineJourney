using FishNet.Managing.Client;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteamConnect : MonoBehaviour
{
    bool connected = false;
    public void ConnectToMe()
    {
        GetComponent<ClientManager>().StartConnection("897578489647844838380");
        connected = true;
    }

    private void OnApplicationQuit()
    {
        if (connected)
        {
            GetComponent<ClientManager>().Connection.Disconnect(true);
        }
    }
}
