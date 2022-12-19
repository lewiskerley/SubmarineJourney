using FishNet;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerMenu : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField nameInput;

    [SerializeField]
    private Button hostButton;

    [SerializeField]
    private Button connectButton;

    private void Start()
    {
        hostButton.onClick.AddListener(() =>
        {
            //if (!SetValidUsername()) { return; }
            //LocalPlayerData.isReady = false;

            InstanceFinder.ServerManager.StartConnection();
            InstanceFinder.ClientManager.StartConnection();
        });

        connectButton.onClick.AddListener(() =>
        {
            //if (!SetValidUsername()) { return; }
            //LocalPlayerData.isReady = false;

            InstanceFinder.ClientManager.StartConnection();
        });
    }

    /*private bool SetValidUsername()
    {
        if (string.IsNullOrEmpty(nameInput.text) || string.IsNullOrWhiteSpace(nameInput.text))
        {
            Debug.Log("Name Required!");
            return false;
        }
        LocalPlayerData.username = nameInput.text;
        return true;
    }*/
}
