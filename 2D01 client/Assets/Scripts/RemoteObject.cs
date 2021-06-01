using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RemoteObject : MonoBehaviour
{
    private HpBar hpBar;
    private int hp;
    public int ID { get; set; }
    public int MaxHP { get; set; }
    public Rigidbody2D Rigid { get; set; }
    public int HP
    {
        get
        {
            return hp;
        }
        set
        {
            hp = value;
            hpBar.SetHpBar(MaxHP, value);
            if (hp <= 0)
                gameObject.SetActive(false);
        }
    }

    public abstract void HpRecovery();

    public abstract void Hit();

    public void Awake()
    {
        hpBar = Instantiate(InitObject.HpBarPrefab, transform.Find("Canvas")).GetComponent<HpBar>();
        Rigid = GetComponent<Rigidbody2D>();
    }
}
