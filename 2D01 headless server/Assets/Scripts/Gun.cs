using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Gun : MonoBehaviour
{
    [SerializeField] private Transform m_FirePoint;

    static private GameObject m_bulletPrefab = null;
    private SpriteRenderer m_playerSR;
    private PlayerScript m_player;
    private bool m_curParentFlapX;

    private Vector2 m_vecFlipX = new Vector2(-1f, 1f);

    void Start()
    {
        if(m_bulletPrefab == null)
            m_bulletPrefab = Resources.Load("Prefabs/Bullet/Gun_Bullet") as GameObject;
        m_player = transform.parent.GetComponent<PlayerScript>();
        m_playerSR = m_player.SR;
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
            SceneManager.SetActiveScene(InitObject.SceneMap[m_player.Map]);
            GameObject BulletObject = MonoBehaviour.Instantiate(m_bulletPrefab, m_FirePoint.position, Quaternion.identity) as GameObject;
            var Bullet = BulletObject.GetComponent<BulletScript>();
            Bullet.SetDir(m_playerSR.flipX ? -1 : 1);
            Bullet.setOwner(m_player);
            m_player.KeyDownSpace = false;
        }
    }
}
