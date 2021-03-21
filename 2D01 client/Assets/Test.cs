using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Test : MonoBehaviourPunCallbacks, IPunObservable
{
    // Start is called before the first frame update
    int dir;
    public PhotonView pv;
    void Start()
    {
        Destroy(gameObject, 111f);

    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(pv.IsMine);
    }

    [PunRPC]
    void DirRPC(int dir) => this.dir = dir;

    [PunRPC]
    void DestroyRPC() => Destroy(gameObject);

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}
