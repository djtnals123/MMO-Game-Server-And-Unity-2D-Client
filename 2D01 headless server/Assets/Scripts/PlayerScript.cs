using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;
using System.Net;

public class PlayerScript : RemoteObject
{
    public SpriteRenderer SR;
    public string nicname;
    public Image HealthImage;

    private bool m_enableSpace = false;
    private bool m_keyDownSpace = false;
    public bool CheckMap { get; set; }
    public IPEndPoint RemoteEP { get; set; }

    public Vector3 CurPos { get; set; }

    public List<int> Equips { get; set; }

    void Awake()
    {
        Rigid = transform.GetComponent<Rigidbody2D>();
        SR = transform.GetComponent<SpriteRenderer>();

        CheckMap = false;
    }


    void Update()
    {
        if ((transform.position - CurPos).sqrMagnitude >= 100) transform.position = CurPos;
        else transform.position = Vector3.Lerp(transform.position, CurPos, Time.deltaTime * 10);
    }


    public override void Hit()
    {
        HP -= 10;
        PacketManager.instance.PlayerHpSynchronization(nicname, -10);
    }
    public override void Die()
    {

    }
    public override void HpRecovery()
    {
    }


    public void SyncPlayer(float positionX, float positionY, float velocityX, float velocityY, float rotation, float angularVelocity, bool flipX)
    {
        CurPos = new Vector3(positionX, positionY);
        Rigid.velocity = new Vector3(velocityX, velocityY);
        Rigid.rotation = rotation;
        Rigid.angularVelocity = angularVelocity;
        SR.flipX = flipX;
    }

    public void TakeOffEquipment(int subType)
    {
        string equipName = null;
        switch (subType)
        {
            case 0:
                equipName = "Weapon";
                break;
        }
        Destroy(transform.Find(equipName).gameObject);

        for (int i = 0; i < Equips.Count; i++)
        {
            if (Equips[i] / 10000 == subType)
            {
                Equips.RemoveAt(i);
                break;
            }
        }
    }

    public void PutOnEquipment(int equipCode)
    {
        string equipType = null;
        string equipName = null;
        switch(equipCode / 10000)
        {
            case 0:
                Transform wearingEquip = transform.Find("Weapon");
                if (wearingEquip)
                    Destroy(wearingEquip.gameObject);
                equipType = "Weapons";
                equipName = "Weapon";
                break;
        }
        GameObject equipmentResource = Resources.Load("Prefabs/" + equipType + "/" + equipCode) as GameObject;
        GameObject equipment = Instantiate(equipmentResource, transform);
        equipment.name = equipName;

        for(int i=0; i < Equips.Count; i++)
        {
            if(Equips[i] / 10000 == equipCode / 10000)
            {
                Equips.RemoveAt(i);
                break;
            }
        }
        Equips.Add(equipCode);

    }
    public void PutOnEquipment(List<int> equipCodes)
    {
        string equipType = null;
        string equipName = null;
        Equips = equipCodes;
        foreach (int i in equipCodes)
        {
            switch (i / 10000)
            {
                case 0:
                    equipType = "Weapons";
                    break;
            }
            switch (i / 10000)
            {
                case 0:
                    equipName = "Weapon";
                    break;
            }
            GameObject equipmentResource = Resources.Load("Prefabs/" + equipType + "/" + i) as GameObject;
            GameObject equipment = Instantiate(equipmentResource, transform);
            equipment.name = equipName;
        }
    }


    public bool KeyDownSpace
    {
        get
        {
            return m_keyDownSpace;
        }
        set
        {
            m_keyDownSpace = value;
        }
    }
    public bool EnableSpace
    {
        set
        {
            if(value != m_enableSpace)

                m_enableSpace = value;
        }
        get
        {
            return m_enableSpace;
        }
    }

    private void OnDestroy()
    {
        MapManager.Instance.Map[Map].Players.Remove(InitObject.UserListByName[nicname]);
        InitObject.UserList.Remove(RemoteEP.ToString());
        InitObject.UserListByName.Remove(nicname);
    }
}