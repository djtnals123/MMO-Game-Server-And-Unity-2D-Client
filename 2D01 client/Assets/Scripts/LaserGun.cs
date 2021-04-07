using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserGun : MonoBehaviour
{
    [SerializeField] private float m_distanceRay = 100;
    [SerializeField] private Transform m_FirePoint;
    [SerializeField] private LineRenderer m_lineRenderer;

    private Vector2 m_vecFlipX = new Vector2(-1f, 1f);
    private SpriteRenderer m_playerSR;
    private PlayerScript m_player;

    private bool m_curParentFlapX;

    // Start is called before the first frame update
    void Start()
    {
        m_playerSR = transform.parent.GetComponent<SpriteRenderer>();
        m_player = transform.parent.GetComponent<PlayerScript>();
        m_curParentFlapX = m_playerSR.flipX;
        m_lineRenderer.enabled = false;

        if (m_playerSR.flipX)
        {
            m_FirePoint.localPosition *= m_vecFlipX;
            transform.localPosition *= m_vecFlipX;
        }

    }

    void ShootLaser()
    {
        if(m_curParentFlapX)
        {
            RaycastHit2D hit = Physics2D.Raycast(m_FirePoint.position, -(m_FirePoint.transform.right), m_distanceRay,
                1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Ground"));
            if (hit.collider != null)
            {
                DrawRay(m_FirePoint.position, hit.point);
            }
            else
            {
                DrawRay(m_FirePoint.position, m_FirePoint.position + Vector3.left * m_distanceRay);
            }
        }
        else
        {
            RaycastHit2D hit = Physics2D.Raycast(m_FirePoint.position, m_FirePoint.transform.right, m_distanceRay,
                1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Ground"));
            if (hit.collider != null)
            {
                DrawRay(m_FirePoint.position, hit.point);
            }
            else
            {
                DrawRay(m_FirePoint.position, m_FirePoint.position + Vector3.right * m_distanceRay);
            }
        }
    }

    void DrawRay(Vector2 startPos, Vector2 endPos)
    {
        m_lineRenderer.SetPosition(0, startPos);
        m_lineRenderer.SetPosition(1, endPos);
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

        if(m_player.IsMine)
        {
            if (m_player.EnableSpace)
            {
                if(!(m_lineRenderer.enabled))
                {
                    PacketManager.EnableSpace(true);
                    m_lineRenderer.enabled = true;
                }
                ShootLaser();
            }
            else
            {
                if(m_lineRenderer.enabled)
                {
                    PacketManager.EnableSpace(false);
                    m_lineRenderer.enabled = false;
                }
            }
        }
        else
        {
            if (m_player.EnableSpace)
            {
                m_lineRenderer.enabled = true;
                ShootLaser();
            }
            else m_lineRenderer.enabled = false; 
        }

    }
}
