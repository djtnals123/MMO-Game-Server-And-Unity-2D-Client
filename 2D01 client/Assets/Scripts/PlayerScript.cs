using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Cinemachine;
using System;

public class PlayerScript : MonoBehaviourPunCallbacks, IPunObservable
{
    public Rigidbody2D RB;
    public Animator AN;
    public SpriteRenderer SR;
    public PhotonView PV;
    public Text NicNameText;
    public Image HealthImage;

    public RectTransform r;

    bool isGround;
    Vector3 curPos;


    // Start is called before the first frame update
    void Awake()
    {
        NicNameText.text = PV.IsMine ? PhotonNetwork.NickName : PV.Owner.NickName;
        NicNameText.color = PV.IsMine ? Color.green : Color.red;
        if (PV.IsMine)
        {
            var CM = GameObject.Find("CM Camera").GetComponent<CinemachineVirtualCamera>();
            CM.Follow = transform;
            CM.LookAt = transform;

            PacketManager.setInventory(PhotonNetwork.NickName);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (PV.IsMine)
        {
            float axis = Input.GetAxisRaw("Horizontal");
            RB.velocity = new Vector2(4 * axis, RB.velocity.y);

            if (axis != 0)
            {
                AN.SetBool("walk", true);
                PV.RPC("FlipXRPC", RpcTarget.AllBuffered, axis);
            }
            else AN.SetBool("walk", false);

            isGround = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(0, -0.5f), 0.07f, 1 << LayerMask.NameToLayer("Ground"));
            AN.SetBool("jump", !isGround);
            if (Input.GetKeyDown(KeyCode.UpArrow) && isGround) PV.RPC("JumpRPC", RpcTarget.All);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                PhotonNetwork.Instantiate("Bullet", transform.position + new Vector3(SR.flipX ? -0.4f : 0.4f, -0.11f, 0), Quaternion.identity).GetComponent<PhotonView>().RPC("DirRPC", RpcTarget.All, SR.flipX ? -1 : 1);
                AN.SetTrigger("attack");
            }
            if(Input.GetKeyDown(KeyCode.I))
            {
                GameObject Inventory = GameObject.Find("Canvas").transform.GetChild(0).gameObject;
                Inventory.SetActive(!Inventory.activeSelf);
            }
        }
        else if ((transform.position - curPos).sqrMagnitude >= 100) transform.position = curPos;
        else transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
    }
    [PunRPC]
    void FlipXRPC(float axis) => SR.flipX = axis == -1;
    [PunRPC]
    void JumpRPC()
    {
        RB.velocity = Vector2.zero;
        RB.AddForce(Vector2.up * 700);
    }


    [PunRPC]
    void DestroyRPC() => Destroy(gameObject);

    public void Hit()
    {
        /*
        string[] messageSplit = PacketManager.SendPacket("HPCHANGE|" + NicNameText.text + "|-10").Split('|');
        int maxhp = Convert.ToInt32(messageSplit[1]);
        int hp = Convert.ToInt32(messageSplit[2]);
        HealthImage.fillAmount = (float)hp / maxhp;
        if (HealthImage.fillAmount <= 0)
        {
            PV.RPC("DestroyRPC", RpcTarget.AllBuffered);
        }*/

    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(HealthImage.fillAmount);
        }
        else
        {
            curPos = (Vector3)stream.ReceiveNext();
            HealthImage.fillAmount = (float)stream.ReceiveNext();
        }
    }

}  
