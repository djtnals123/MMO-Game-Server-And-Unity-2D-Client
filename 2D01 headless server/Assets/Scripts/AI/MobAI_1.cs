using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MobAI_1 : RemoteObject
{
    public int NextMove { get; set; }
    private System.Random rand = new System.Random();

    void Start()
    {
        Rigid = GetComponent<Rigidbody2D>();
        Think();
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameObject.scene.GetPhysicsScene2D().Raycast(new Vector2(transform.position.x + NextMove * 0.2f, transform.position.y), Vector2.down, 1, 1 << LayerMask.NameToLayer("Ground")))
        {
            NextMove *= -1;
            CancelInvoke();
            Invoke("Think", 2);
        }
        Rigid.velocity = new Vector2(NextMove, Rigid.velocity.y);
    }


    void Think()
    {
        NextMove = rand.Next(-1,2);
        Invoke("Think", rand.Next(1, 6));
        PacketManager.instance.MoveMob(Map, ID, NextMove, transform.position.x, transform.position.y, Rigid.velocity.y);
    }
    public override void Hit()
    {
        HP -= 10;
        PacketManager.instance.MobHpSynchronization(ID, -10, Map);
    }
    public override void Die()
    {
        MapManager.Instance.Map[Map].DeadMobs.AddLast(this);
        MapManager.Instance.SpawnMobsIfPossible(Map);
    }
    public override void HpRecovery() 
    {
        HP = MaxHP;
    }
}
