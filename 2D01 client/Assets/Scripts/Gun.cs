using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] private Transform m_FirePoint;

    private GameObject m_bulletPrefab;
    private SpriteRenderer m_playerSR;
    private PlayerScript m_player;
    private bool m_curParentFlapX;

    private Vector2 m_vecFlipX = new Vector2(-1f, 1f);

    void Start()
    {
        m_bulletPrefab = Resources.Load("Prefabs/Bullet/Gun_Bullet") as GameObject;
        m_player = transform.parent.GetComponent<PlayerScript>();
        m_playerSR = transform.parent.GetComponent<SpriteRenderer>();
        m_curParentFlapX = m_playerSR.flipX;

        if (m_playerSR.flipX)
        {
            m_FirePoint.localPosition *= m_vecFlipX;
            transform.localPosition *= m_vecFlipX;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (m_curParentFlapX != m_playerSR.flipX)
        {
            transform.localPosition *= m_vecFlipX;
            m_FirePoint.transform.localPosition *= m_vecFlipX;

            m_curParentFlapX = m_playerSR.flipX;
        }

        if (m_player.KeyDownSpace)
        {
            GameObject BulletObject = MonoBehaviour.Instantiate(m_bulletPrefab, m_FirePoint.position, Quaternion.identity) as GameObject;
            var Bullet = BulletObject.GetComponent<BulletScript>();
            Bullet.SetDir(m_playerSR.flipX ? -1 : 1);
            Bullet.setOwner(m_player.NicNameText.text);
            if (m_player.IsMine)
            {
                Bullet.IsMine = true;
                PacketManager.Instance.AttackPlayer(InitObject.playerNicname);
            }
            else m_player.KeyDownSpace = false;
            m_player.AN.SetTrigger("attack");
        }
    }
}
