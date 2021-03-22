using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class PacketManager : MonoBehaviour
{
    private static UdpClient Client = new UdpClient();
    

    void Start()
    {
        InvokeRepeating("ReceivePacket", 0f, 0.1f);
        InvokeRepeating("ObjectSynchronization", 0f, 0.1f);
        DontDestroyOnLoad(gameObject);
    }

    private void ObjectSynchronization()
    {
        GameObject player = GameObject.Find("Player(Clone)");
        Debug.Log(string.Join("|", "ObjectSynchronization|s|0|-0.0003528051|-0.4914245|-0.4425813|0|0|0".Split('|')).Split('|')[1]);
        if (player != null)
        {
            Rigidbody2D rd = player.GetComponent<Rigidbody2D>();
            string message = "ObjectSynchronization|" + InitObject.playerNicname + '|' + rd.velocity.x + '|' + rd.velocity.y + '|' +
                rd.transform.position.x + '|' + rd.transform.position.y + '|' + rd.transform.rotation.x + '|' + rd.transform.rotation.y + '|' + rd.angularVelocity;
     //       Debug.Log(string.Join("|", message.Split('|')));
            SendPacket(message);
        }
    }

    private void Update()
    {
        //  Debug.Log("pc");
    }

    public static string SendPacket(string message)
    {
        byte[] requestPacket = System.Text.Encoding.UTF8.GetBytes(message); // 형변환
        Client.Send(requestPacket, requestPacket.Length, "127.0.0.1", 7777); // 데이터 송신

        return null;
    }

    public void ReceivePacket()
    {
        if(Client.Available != 0)
        {
            IPEndPoint epRemote = new IPEndPoint(IPAddress.Any, 0); // 데이터 수신
            byte[] responsePacket = Client.Receive(ref epRemote);
            string response = System.Text.Encoding.Default.GetString(responsePacket); // 형변환
            string[] responseSplit = response.Split('|');
            switch (responseSplit[0])
            {
                case "ObjectSynchronization":
                    GameObject player = GameObject.Find("Player " + response[1]);
           //         Debug.Log(string.Join("|", responseSplit));
                    Debug.Log(response[1] + "11111111111" + string.Join("|", responseSplit));
                    if (player != null)
                    {
                        Debug.Log("check");
                        Rigidbody2D rd = GameObject.Find("Player " + response[1]).GetComponent<Rigidbody2D>();
                        rd.velocity = new Vector3(Convert.ToSingle(response[2]), Convert.ToSingle(response[3]));
                        rd.transform.position = new Vector3(Convert.ToSingle(response[4]), Convert.ToSingle(response[5]));
                        rd.transform.rotation = Quaternion.Euler(Convert.ToSingle(response[6]), Convert.ToSingle(response[7]), 0f);
                        rd.angularVelocity = Convert.ToSingle(response[8]);
                    }
                    break;
                case "LOGIN_SUCCESS":
                    UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
                    break;
                case "INIT_PLAYERS":
                    InitObject.InitPlayer(responseSplit);
                    break;
                case "GET_INVENTORY_SUCCESS":
                    List<RectTransform> list = new List<RectTransform>();
                    GameObject Slot = Resources.Load("Slot") as GameObject;
                    RectTransform r = Slot.GetComponent<RectTransform>();
                    Transform parent = GameObject.Find("Slots").transform;

                    for (int i = 0; Convert.ToInt32(responseSplit[1]) > i; i++)
                    {
                        list.Add(MonoBehaviour.Instantiate(r));
                        list[i].transform.SetParent(parent);
                        list[i].transform.localScale = new Vector2(1f, 1f);
                    }
                    for (int i = 0; i < (responseSplit.Length - 2) / 3; i++)
                    {
                        int slotNumber = Convert.ToInt32(responseSplit[i * 3 + 1]);
                        list[i].GetComponent<SlotScript>().SetItem(Convert.ToInt32(responseSplit[i * 3 + 2]));
                    }
                    break;
                case "HPCHANGE_SUCCESS":
                    break;
                default:
                    break;
            }
        }
    }

    public static void InitPlayers()
    {
        PacketManager.SendPacket("INIT_PLAYERS|" + InitObject.playerNicname);
    }

    public static void Login(string id, string pass)
    {
        PacketManager.SendPacket("LOGIN|" + id + "|" + pass);
    }

    public static void PlayerMovement(string id, Rigidbody2D rb)
    {
        string response = PacketManager.SendPacket("PLAYER_MOVEMENT|" + id + "|" + rb.position.x + "|" + rb.position.y + "|" + rb.rotation + "|" + rb.velocity + "|" + rb.angularVelocity);
        
        if (response == "LOGIN_SUCCESS")
            UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
    }

    public static void setInventory(string id)
    {
        PacketManager.SendPacket("GET_INVENTORY|"+id);
    }
}
