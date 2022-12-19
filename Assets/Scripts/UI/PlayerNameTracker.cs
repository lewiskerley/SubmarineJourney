using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNameTracker : NetworkBehaviour
{
    public static event Action<NetworkConnection, string> OnNameChange;

    [SyncObject]
    private readonly SyncDictionary<NetworkConnection, string> playerNames = new SyncDictionary<NetworkConnection, string>();

    public static PlayerNameTracker instance { get; private set; }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Multiple PlayerNameTracker instances found!");
            return;
        }

        instance = this;
        playerNames.OnChange += playerNames_OnChange;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        base.NetworkManager.ServerManager.OnRemoteConnectionState += ServerManager_OnRemoteConnectionState;
    }
    public override void OnStopServer()
    {
        base.OnStopServer();
        base.NetworkManager.ServerManager.OnRemoteConnectionState -= ServerManager_OnRemoteConnectionState;
    }


    private void ServerManager_OnRemoteConnectionState(NetworkConnection nc, FishNet.Transporting.RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState != FishNet.Transporting.RemoteConnectionState.Started)
            playerNames.Remove(nc);
    }

    private void playerNames_OnChange(SyncDictionaryOperation op, NetworkConnection key, string value, bool asServer)
    {
        if (op == SyncDictionaryOperation.Add || op == SyncDictionaryOperation.Set)
            OnNameChange?.Invoke(key, value);
    }


    public static string GetPlayerName(NetworkConnection conn)
    {
        if (instance.playerNames.TryGetValue(conn, out string result))
            return result;
        else
            return string.Empty;
    }
    public static SyncDictionary<NetworkConnection, string> GetPlayerNames()
    {
        return instance.playerNames;
    }

    [Client]
    public static void SetName(string name)
    {
        instance.ServerSetName(name);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerSetName(string name, NetworkConnection sender = null)
    {
        playerNames[sender] = name;
    }
}
