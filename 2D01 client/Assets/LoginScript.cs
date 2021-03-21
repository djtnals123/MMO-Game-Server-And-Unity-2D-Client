using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginScript : MonoBehaviour
{
    public InputField IDInput;
    public InputField PassInput;
    public GameObject LoginObject;
    private UdpClient Client;

    private void Start()
    {
        Client = new UdpClient();
        DontDestroyOnLoad(LoginObject);
    }

    public void Login() => PacketManager.Login(IDInput.text, PassInput.text);

    private void OnDestroy()
    {
        Client.Close();
    }
}
