using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentSlot : MonoBehaviour
{
    int m_item;

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void TakeOff()
    {
        m_item = 0;
        transform.GetChild(0).gameObject.SetActive(false);
    }

    public int Item
    {
        set
        {
            m_item = value;
            transform.GetChild(0).gameObject.SetActive(true);
            transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("Image/Item/Equipment/" + value);
        }
        get
        {
            return m_item;
        }
    }
}
