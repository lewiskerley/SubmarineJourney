using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Volcano : MonoBehaviour
{
    int x;
    int y;
    bool errupt = false;

    public void Initialise(int x, int y)
    {
        this.x = x;
        this.y = y;
        errupt = false;
    }

    public void StartActiveEvent()
    {
        StartCoroutine(ActiveLoop());
    }

    public void StartErruptEvent()
    {
        errupt = true;

        //TODO: Send death wave: (Fast speed, goes infinite)
        //WorldData.instance.GetMap();
        Debug.Log("Deadly Wave");
    }

    private IEnumerator ActiveLoop()
    {
        WaitForSeconds loopDelay = new WaitForSeconds(1.5f);

        while(!errupt)
        {
            yield return loopDelay;

            if (errupt) { break; } //Cancel Activation Wave Early

            //TODO: Send wave: (Slow speed, every 4th wave goes infinite)
            //WorldData.instance.GetMap();
            Debug.Log("Small Wave");
        }

        //Erruption has triggered!
        errupt = false;
    }
}
