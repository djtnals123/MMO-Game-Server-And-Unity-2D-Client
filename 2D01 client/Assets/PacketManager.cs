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
    }

    public static string SendPacket(string message)
    {
        byte[] requestPacket = System.Text.Encoding.UTF8.GetBytes(message); // 형변환
        Client.Send(requestPacket, requestPacket.Length, "127.0.0.1", 7777); // 데이터 송신
        IPEndPoint epRemote = new IPEndPoint(IPAddress.Any, 0); // 데이터 수신
        byte[] responsePacket = Client.Receive(ref epRemote);
        string response = System.Text.Encoding.Default.GetString(responsePacket); // 형변환

        return response;
    }

    public static void Login(string id, string pass)
    {
        string response = PacketManager.SendPacket("LOGIN|" + id + "|" + pass);
        Debug.Log(response);
        if (response == "LOGIN_SUCCESS") 
            UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
    }

    public static List<RectTransform> setInventory(string id)
    {
        List<RectTransform> list = new List<RectTransform>();
        string[] messageSplit = PacketManager.SendPacket("GET_INVENTORY|"+id).Split('|');
        if (messageSplit[0] == "GET_INVENTORY_SUCCESS")
        {
            GameObject Slot = Resources.Load("Slot") as GameObject;
            RectTransform r = Slot.GetComponent<RectTransform>();
            Transform parent = GameObject.Find("Slots").transform;
            for (int i = 0; Convert.ToInt32(messageSplit[1]) > i; i++)
            {
                list.Add(MonoBehaviour.Instantiate(r));
                list[i].transform.SetParent(parent);
                list[i].transform.localScale = new Vector2(1f, 1f);
            }
            for (int i = 0; i < (messageSplit.Length - 2) / 3; i++)
            {
                int slotNumber = Convert.ToInt32(messageSplit[i * 3 + 1]);
                list[i].GetComponent<SlotScript>().SetItem(Convert.ToInt32(messageSplit[i * 3 + 2]));
            }
        }
        else return null;
        return list;


    }
}
