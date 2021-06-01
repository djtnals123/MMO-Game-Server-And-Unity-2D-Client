using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using System;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;

public class PlayerScript : RemoteObject
{
    public Animator AN;
    public SpriteRenderer SR;
    public Text NicNameText;
    public bool m_isMine = false;

    private bool m_enableSpace = false;
    private bool m_keyDownSpace = false;

    private bool m_isGround;
    public Vector3 CurPos { get; set; }
    private float m_curRot;
    private bool canWarp = true;


    public GameObject UI { get; set; }

    public void ItsMe()
    {
        m_isMine = true;
        //      NicNameText.text = PV.IsMine ? PhotonNetwork.NickName : PV.Owner.NickName;
        //      NicNameText.color = PV.IsMine ? Color.green : Color.red;


        GameObject uiPrefab = Resources.Load("Prefabs/UI/UI") as GameObject;
        GameObject ui = Instantiate(uiPrefab);
        DontDestroyOnLoad(ui);
        ui.name = "UI";


        var CM = GameObject.Find("CM Camera").GetComponent<CinemachineVirtualCamera>();
        CM.Follow = transform;
        CM.LookAt = transform;

        InvokeRepeating("Synchronization", 0f, 0.1f);
        DontDestroyOnLoad(this);

        UI = ui;
    }

    public bool IsLoading
    {
        set
        {
            gameObject.SetActive(!value);
            UI.SetActive(!value);
        }
    }

    void Update()
    {
        if (m_isMine)
        {
            float axis = Input.GetAxisRaw("Horizontal");
            Rigid.velocity = new Vector2(4 * axis, Rigid.velocity.y);


            if (axis != 0)
            {
                AN.SetBool("walk", true);
                SR.flipX = axis == -1;
            }
            else AN.SetBool("walk", false);

            m_isGround = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(0, -0.5f), 0.07f, 1 << LayerMask.NameToLayer("Ground"));
            AN.SetBool("jump", !m_isGround);

            if (Input.GetKeyDown(KeyCode.Space))
                m_keyDownSpace = true;
            else m_keyDownSpace = false;
            if (Input.GetKey(KeyCode.Space))
                m_enableSpace = true;
            else m_enableSpace = false;
            if (Input.GetKeyDown(KeyCode.UpArrow) && m_isGround)
            {
                GameObject obj = Physics2D.OverlapPoint(transform.position).gameObject;
                if (obj.name.Contains("Portal "))
                {
                    if(canWarp)
                    {
                        canWarp = false;
                        Debug.Log("hihihi");
                        WarpMap(int.Parse(Regex.Replace(obj.name, @"\D", "")));
                        Invoke("warpDelay", 1f);
                    }
                }
                else
                {
                    Rigid.velocity = Vector2.zero;
                    Rigid.AddForce(Vector2.up * 700);
                }
            }
            if (Input.GetKeyDown(KeyCode.I))
            {
                GameObject Inventory = GameObject.Find("UI").transform.GetChild(0).gameObject;
                Inventory.SetActive(!Inventory.activeSelf);
            }
        }
        else if ((transform.position - CurPos).sqrMagnitude >= 100) transform.position = CurPos;
        else transform.position = Vector3.Lerp(transform.position, CurPos, Time.deltaTime * 10);

    }

    private void warpDelay()
    {
        canWarp = true;
    }

    private void WarpMap(int portal)
    {
        GameObject[] objs = SceneManager.GetSceneByName("Remote").GetRootGameObjects();
        for (int i=1; i < objs.Length; i++ )
            Destroy(objs[i]);
        LoadingSceneManager.IsLoading = true;
        PacketManager.Instance.WarpMap(portal);
    }

    public override void Hit() //오류 수정예정
    {
    }
    public override void HpRecovery()
    {
    }


    public void SyncPlayer(float positionX, float positionY, float velocityX, float velocityY, float rotation, float angularVelocity, bool flipX)
    {
        Debug.Log(positionX + " " + positionY);
        CurPos = new Vector3(positionX, positionY);
        Rigid.velocity = new Vector3(velocityX, velocityY);
        Rigid.rotation = rotation;
        Rigid.angularVelocity = angularVelocity;
        SR.flipX = flipX;
    }

    private void Synchronization()
    {
        if (canWarp && transform.position.x != CurPos.x || transform.position.y != CurPos.y || Rigid.rotation != m_curRot)
        {
            CurPos = new Vector3(transform.position.x, transform.position.y);
            PacketManager.Instance.ObjectSynchronization(InitObject.playerNicname, Rigid.transform.position.x, Rigid.transform.position.y, Rigid.velocity.x, Rigid.velocity.y, Rigid.rotation, Rigid.angularVelocity, SR.flipX, InitObject.MapCheck);

            m_curRot = Rigid.rotation;
            try
            {
//                int map = int.Parse(Regex.Replace(SceneManager.GetActiveScene().name, @"\D", ""));

            }
            catch (FormatException e)
            {
                Debug.Log("숫자없음 " + e.ToString());
            }

         }
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

    }
    public void PutOnEquipment(List<int> equipCodes)
    {
        string equipType = null;
        string equipName = null;
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

    public Animator Animator
    {
        get
        {
            return AN; 
        }
    }
    public bool IsMine
    {
        get
        {
            return m_isMine;
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

    private void OnApplicationQuit()
    {
        if (m_isMine)
            PacketManager.Instance.Disconnected();
    }

}