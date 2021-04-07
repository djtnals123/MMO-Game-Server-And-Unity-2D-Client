using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using System;

public class PlayerScript : MonoBehaviour
{
    public Rigidbody2D RB;
    public Animator AN;
    public SpriteRenderer SR;
    public Text NicNameText;
    public Image HealthImage;
    public bool m_isMine = false;

    private bool m_enableSpace = false;
    private bool m_keyDownSpace = false;

    private bool m_isGround;
    private Vector3 m_curPos;
    private float m_curRot;

    GameObject bulletPrefab;

    // Start is called before the first frame update
    void Awake()
    {
        bulletPrefab = Resources.Load("Bullet") as GameObject;
        //      NicNameText.text = PV.IsMine ? PhotonNetwork.NickName : PV.Owner.NickName;
        //      NicNameText.color = PV.IsMine ? Color.green : Color.red;


    }

    public void ItsMe()
    {
        m_isMine = true;
        //      NicNameText.text = PV.IsMine ? PhotonNetwork.NickName : PV.Owner.NickName;
        //      NicNameText.color = PV.IsMine ? Color.green : Color.red;
        var CM = GameObject.Find("CM Camera").GetComponent<CinemachineVirtualCamera>();
        CM.Follow = transform;
        CM.LookAt = transform;

        InvokeRepeating("Synchronization", 0f, 0.1f);
        PacketManager.PlayerSetting(InitObject.playerNicname);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_isMine)
        {
            float axis = Input.GetAxisRaw("Horizontal");
            RB.velocity = new Vector2(4 * axis, RB.velocity.y);


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
                RB.velocity = Vector2.zero;
                RB.AddForce(Vector2.up * 700);
            }
            if (Input.GetKeyDown(KeyCode.I))
            {
                GameObject Inventory = GameObject.Find("Canvas").transform.GetChild(0).gameObject;
                Inventory.SetActive(!Inventory.activeSelf);
            }
        }
        else if ((transform.position - m_curPos).sqrMagnitude >= 100) transform.position = m_curPos;
        else transform.position = Vector3.Lerp(transform.position, m_curPos, Time.deltaTime * 10);

        Debug.Log(transform.name + " ff");
    }

    public void Hit() //오류 수정예정
    {
        if(m_isMine)
            PacketManager.HpSynchronization(-10);
    }


    public void SyncPlayer(string name, float positionX, float positionY, float velocityX, float velocityY, float rotation, float angularVelocity, bool flipX)
    {
        Debug.Log(positionX+ " "  +positionY);
        m_curPos = new Vector3(positionX, positionY);
        RB.velocity = new Vector3(velocityX, velocityY);
        RB.rotation = rotation;
        RB.angularVelocity = angularVelocity;
        SR.flipX = flipX;
    }

    private void Synchronization()
    {
        if (transform.position.x != m_curPos.x || transform.position.y != m_curPos.y || RB.rotation != m_curRot)
        {
            m_curPos = new Vector3(transform.position.x, transform.position.y);
            m_curRot = RB.rotation;
            PacketManager.ObjectSynchronization(InitObject.playerNicname, RB.transform.position.x, RB.transform.position.y, RB.velocity.x, RB.velocity.y, RB.rotation, RB.angularVelocity, SR.flipX);
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
        Debug.Log("gd " + equipCodes.Count);
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

    ~PlayerScript()
    {
        if (m_isMine)
            PacketManager.Disconnected();
    }
}