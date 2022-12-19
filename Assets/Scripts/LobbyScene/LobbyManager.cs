using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Steamworks;
using System.Collections.Generic;
using TMPro;

public class LobbyManager : NetworkBehaviour
{
    [SyncObject]
    private readonly SyncDictionary<NetworkConnection, string> playerNames = new SyncDictionary<NetworkConnection, string>();

    public TMP_Text playersText;

    private void Awake()
    {
        UpdatePlayersDisplay();
    }

    private void UpdatePlayersDisplay()
    {
        string newText = "Players [" + playerNames.Count + "/4]\n";
        foreach(KeyValuePair<NetworkConnection, string> playerName in playerNames)
        {
            newText += "- " + playerName.Value + "\n";
        }
        playersText.text = newText;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        base.NetworkManager.ServerManager.OnRemoteConnectionState += ServerManager_OnRemoteConnectionState;

        if (SteamManager.Initialized)
        {
            playerNames.Add(ClientManager.Connection, SteamFriends.GetPersonaName());
        }
        UpdatePlayersDisplay();
    }
    public override void OnStopServer()
    {
        base.OnStopServer();
        base.NetworkManager.ServerManager.OnRemoteConnectionState -= ServerManager_OnRemoteConnectionState;
    }


    private void ServerManager_OnRemoteConnectionState(NetworkConnection nc, FishNet.Transporting.RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState != FishNet.Transporting.RemoteConnectionState.Started) //Disconnect
        {
            playerNames.Remove(nc);
        }
        else //Connect
        {
            if (SteamManager.Initialized)
            {
                playerNames.Add(nc, SteamFriends.GetPersonaName());
            }
        }

        UpdatePlayersDisplay();
    }
}
