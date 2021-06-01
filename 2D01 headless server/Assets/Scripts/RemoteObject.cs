using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RemoteObject : MonoBehaviour
{
    private int hp;
    public int ID { get; set; }
    public int Map { get; set; }
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
            if (hp <= 0)
            {
                gameObject.SetActive(false);
                Die();
            }
        }
    }
    public abstract void Hit();
    public abstract void Die();
    public abstract void HpRecovery();
    public void Initialise(int id, int map, int maxHP)
    {
        ID = id;
        Map = map;
        MaxHP = maxHP;
        HP = maxHP;
        gameObject.SetActive(false);
    }
}
