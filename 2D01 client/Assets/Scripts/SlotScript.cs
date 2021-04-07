using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotScript : MonoBehaviour
{
    int m_item;
    int m_type;
    int m_count;

    public int Item
    {
        get
        {
            return m_item;
        }
        set
        {
            m_item = value;
        }
    } 

    void Start()
    {
    }

    public void SetItem(int item, int type)
    {
        m_item = item;
        m_type = type;

        if (item == 0)
            transform.GetChild(0).gameObject.SetActive(false);
        else
        {
            string strType = null;
            switch (type)
            {
                case 0:
                    strType = "Equipment";
                    break;
                case 1:
                    strType = "Useable";
                    break;
                case 2:
                    strType = "Etc";
                    break;
                case 3:
                    strType = "Enhancement";
                    break;
            }
            transform.GetChild(0).gameObject.SetActive(true);
            transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("Image/Item/" + strType + "/" + item);
        }

        //    Debug.Log("eeeeeeeeee");
    }

    private void Update()
    {
    }
}
