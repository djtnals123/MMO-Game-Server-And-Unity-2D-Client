using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectedItem : MonoBehaviour
{
    public enum OwnerType { inventory, equipment }

    private int m_selectedItemIndex;
    private OwnerType m_owner;
    public Image selectedItemImage;

   private void Awake()
    {
        selectedItemImage = transform.GetComponent<Image>();
        selectedItemImage.gameObject.SetActive(false);
    }

    void Update()
    {
        transform.GetComponent<Image>().transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y);
    }

    public Image SelectedItemImage
    {
        get
        {
            return selectedItemImage;
        }
    }
    public bool IsSelected
    {
        get
        {
            return selectedItemImage.gameObject.activeSelf;
        }
        set
        {
            if(value) transform.GetComponent<Image>().transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y);
            selectedItemImage.gameObject.SetActive(value);
        }
    }
    public int SelectedItemIndex
    {
        get
        {
            return m_selectedItemIndex;
        }
        set => m_selectedItemIndex = value;
    }
    public OwnerType Owner
    {
        get
        {
            return m_owner;
        }
        set => m_owner = value;
    }


}
