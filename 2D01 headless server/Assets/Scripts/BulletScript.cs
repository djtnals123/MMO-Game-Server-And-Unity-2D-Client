using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    int dir;
    int speed = 21;
    bool isMine = false;
    PlayerScript owner = null;

    void Start()
    {
        Destroy(gameObject, 3.5f);
    }

    private void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime * dir);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Ground") Destroy(gameObject);
        else if (collision.tag == "Player" && collision.gameObject != owner.gameObject)
        {
            collision.GetComponent<PlayerScript>().Hit();
            Destroy(gameObject);
        }
    }

    public void SetDir(int dir) => this.dir = dir;

    public bool IsMine
    {
        get
        {
            return isMine;
        }
        set
        {
            isMine = value;
        }
    }

    public void setOwner(PlayerScript owner) => this.owner = owner;
}
