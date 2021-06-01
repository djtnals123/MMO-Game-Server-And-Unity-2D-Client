using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MobAI_1 : RemoteObject
{
    public int NextMove { get; set; }

    // Update is called once per frame
    void Update()
    {
        if(!Physics2D.Raycast(new Vector2(transform.position.x + NextMove * 0.2f, transform.position.y), Vector2.down, 1, 1 << LayerMask.NameToLayer("Ground")))
            NextMove *= -1;
        Rigid.velocity = new Vector2(NextMove, Rigid.velocity.y);
    }

    public void MobSync(Vector2 position, float velY, int nextMove)
    {
        transform.position = position;
        this.NextMove = nextMove;
    }

    public override void Hit()
    {

    }
    public override void HpRecovery()
    {
        HP = MaxHP;
    }
}
