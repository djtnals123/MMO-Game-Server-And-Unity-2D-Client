 using System.Collections;
using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks
{
	void Awake()
	{
		Screen.SetResolution(960, 540, false);
		PhotonNetwork.SendRate = 60;
		PhotonNetwork.SerializationRate = 30;
		Connect();
	}

	public void Connect() => PhotonNetwork.ConnectUsingSettings();

    public override void OnConnectedToMaster()
    {
		PhotonNetwork.LocalPlayer.NickName = GameObject.Find("Login").GetComponent<LoginScript>().IDInput.text;
		PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions { MaxPlayers = 6 }, null );
	}

    public override void OnJoinedRoom()
    {
		Debug.Log("dd");
		Spawn();
	}
	public void Spawn()
	{
		PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity);
	}

	void Update() { if (Input.GetKeyDown(KeyCode.Escape) && PhotonNetwork.IsConnected) PhotonNetwork.Disconnect(); 	}


    public override void OnDisconnected(DisconnectCause cause)
    {
    }

}
