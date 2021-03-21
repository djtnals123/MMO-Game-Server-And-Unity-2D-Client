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
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        //  Debug.Log("pc");
        ReceivePacket();
    }

    public static string SendPacket(string message)
    {
        byte[] requestPacket = System.Text.Encoding.UTF8.GetBytes(message); // 형변환
        Client.Send(requestPacket, requestPacket.Length, "127.0.0.1", 7777); // 데이터 송신

        return null;
    }

    public static void ReceivePacket()
    {


        if(Client.Available != 0)
        {
            IPEndPoint epRemote = new IPEndPoint(IPAddress.Any, 0); // 데이터 수신
            byte[] responsePacket = Client.Receive(ref epRemote);
            string response = System.Text.Encoding.Default.GetString(responsePacket); // 형변환
            string[] responseSplit = response.Split('|');
            switch (responseSplit[0])
            {
                case "LOGIN_SUCCESS":
                    UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
                    break;
                case "INIT_NEWPLAYER":

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
