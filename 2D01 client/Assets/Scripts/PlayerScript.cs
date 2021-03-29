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
    public bool isMine = false;

    public RectTransform r;

    bool isGround;
    Vector3 curPos;
    float curRot;

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
        isMine = true;
        //      NicNameText.text = PV.IsMine ? PhotonNetwork.NickName : PV.Owner.NickName;
        //      NicNameText.color = PV.IsMine ? Color.green : Color.red;
        var CM = GameObject.Find("CM Camera").GetComponent<CinemachineVirtualCamera>();
        CM.Follow = transform;
        CM.LookAt = transform;

        InvokeRepeating("Synchronization", 0f, 0.1f);
        PacketManager.getInventory(InitObject.playerNicname);
    }

    // Update is called once per frame
    void Update()
    {
        if (isMine)
        {
            float axis = Input.GetAxisRaw("Horizontal");
            RB.velocity = new Vector2(4 * axis, RB.velocity.y);

            if (axis != 0)
            {
                AN.SetBool("walk", true);
                SR.flipX = axis == -1;
            }
            else AN.SetBool("walk", false);

            isGround = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(0, -0.5f), 0.07f, 1 << LayerMask.NameToLayer("Ground"));
            AN.SetBool("jump", !isGround);
            if (Input.GetKeyDown(KeyCode.UpArrow) && isGround)
            {
                RB.velocity = Vector2.zero;
                RB.AddForce(Vector2.up * 700);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Attack();
            }                  
            if (Input.GetKeyDown(KeyCode.I))
            {
                GameObject Inventory = GameObject.Find("Canvas").transform.GetChild(0).gameObject;
                Inventory.SetActive(!Inventory.activeSelf);
            }
        }
               else if ((transform.position - curPos).sqrMagnitude >= 100) transform.position = curPos;
               else transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
    }

    public void Attack()
    {
        GameObject BulletObject = MonoBehaviour.Instantiate(bulletPrefab, transform.position + new Vector3(SR.flipX ? -0.4f : 0.4f, -0.11f, 0), Quaternion.identity) as GameObject;
        var Bullet = BulletObject.GetComponent<BulletScript>();
        Bullet.SetDir(SR.flipX ? -1 : 1);
        Bullet.setOwner(this.NicNameText.text);
        if (isMine)
        {
            Bullet.IsMine(true);
            PacketManager.AttackPlayer(InitObject.playerNicname);
        }

        AN.SetTrigger("attack");
    }

    public void Hit() //오류 수정예정
    {
        if(isMine)
            PacketManager.HpSynchronization(-10);
    }


    public void SyncPlayer(string name, float positionX, float positionY, float velocityX, float velocityY, float rotation, float angularVelocity, bool flipX)
    {
        curPos = new Vector3(positionX, positionY);
        RB.velocity = new Vector3(velocityX, velocityY);
        RB.rotation = rotation;
        RB.angularVelocity = angularVelocity;
        SR.flipX = flipX;
    }

    private void Synchronization()
    {

        if (transform.position.x != curPos.x || transform.position.y != curPos.y || RB.rotation != curRot)
        {
            curPos = new Vector3(transform.position.x, transform.position.y);
            curRot = RB.rotation;
            PacketManager.ObjectSynchronization(InitObject.playerNicname, RB.transform.position.x, RB.transform.position.y, RB.velocity.x, RB.velocity.y, RB.rotation, RB.angularVelocity, SR.flipX);
        }
    }
}