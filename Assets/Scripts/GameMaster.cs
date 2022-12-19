using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    WorldData worldData;

    private void Start()
    {
        StartCoroutine(ConnectAndCreateGameEnvironment());
    }

    private IEnumerator ConnectAndCreateGameEnvironment()
    {
        Debug.Log("Waiting for all Players to connect...");

        yield return new WaitForSeconds(0.5f);
    }
}
