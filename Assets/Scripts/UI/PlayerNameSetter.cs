using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerNameSetter : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField nameInput;

    private void Awake()
    {
        nameInput.onSubmit.AddListener(nameInput_OnSubmit);
    }
    private void nameInput_OnSubmit(string name)
    {
        Debug.Log("Set Name: " + name);
        PlayerNameTracker.SetName(name);
    }
}
