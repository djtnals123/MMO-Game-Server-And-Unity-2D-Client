using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HpBar : MonoBehaviour
{
    private Image healthImage;
    void Awake()
    {
        SpriteRenderer sr = transform.parent.parent.GetComponent<SpriteRenderer>();
        healthImage = transform.Find("Health").GetComponent<Image>();

        transform.position = new Vector2(sr.bounds.center.x, sr.bounds.max.y + 0.2f);
    }

    public void SetHpBar(int maxHp, int hp) =>
        healthImage.fillAmount = (float)hp / maxHp;


}
