using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotScript : MonoBehaviour
{
    public int item;
    int count;

    public void SetItem(int item)
    {
        this.item = item;
        transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = item.ToString();
    }
}
