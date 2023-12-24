using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System;
using Unity.Netcode.Relay;
using Unity.Services.Core;

public class GameJoinCanvas : MonoBehaviour
{
    public TMP_InputField ipField;
    public TMP_InputField portField;
    public TMP_Text hostDataText;
    public GameObject failText;
    public TMP_Text joinCodeText;
    public string joinCode;
    private async void Start()
    {
        #if UNITY_EDITOR
        if (ParrelSync.ClonesManager.IsClone())
        {
            Debug.Log("This is a clone project.");
            string customArgument = "MTest Duplicate";
            UnityEditor.PlayerSettings.productName = customArgument;
        }
        #endif
        await UnityServices.InitializeAsync();
        print("Services Initialized");
    }
    void Update()
    {
        joinCode = ipField.text.Substring(0, 6);
        //NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ipField.text,ushort.TryParse(portField.text,out ushort result) ? result : (ushort)2417);
    }
    public async void StartHost()
    {
        //NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(GetInternalIPAddress(), ushort.TryParse(portField.text, out ushort result) ? result : (ushort)2417);
        string joinCode = await RelayConnections.HostGame();
        if (joinCode != null)
            print(joinCode);
        	joinCodeText.text = joinCode;
            gameObject.SetActive(false);
    }
    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
    }
    public async void StartClient()
    {
        bool result = await RelayConnections.JoinGame(joinCode/*ipField.text.Substring(0,6)*/);
        print(result);
        if (result)
            gameObject.SetActive(false);
        else
            FailText();
    }
    public async void FailText()
    {
        failText.SetActive(true);
        await Task.Delay(3000);
        failText.SetActive(false);
    }
    public static string GetExternalIPAddress()
    {
        string externalIP = "N/A";
        try
        {
            using (WebClient client = new WebClient())
            {
                externalIP = client.DownloadString("http://api.ipify.org");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error getting external IP address: " + e.Message);
        }

        return externalIP;
    }

    public static string GetInternalIPAddress()
    {
        string internalIP = "N/A";
        try
        {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                internalIP = endPoint.Address.ToString();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error getting internal IP address: " + e.Message);
        }

        return internalIP;
    }
}
