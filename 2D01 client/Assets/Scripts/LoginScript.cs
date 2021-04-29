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

    private void Start()
    {

        DontDestroyOnLoad(LoginObject);
    }

    public void Login()
    {
        InitObject.playerNicname = IDInput.text;
        PacketManager.Instance.Login(IDInput.text, PassInput.text);
    }

    private void OnDestroy()
    {
    }
}
