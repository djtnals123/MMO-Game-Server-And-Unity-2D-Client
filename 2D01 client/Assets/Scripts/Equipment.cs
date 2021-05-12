using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Equipment : MonoBehaviour, IPointerClickHandler
{
    private static EquipmentSlot weapon;
    [SerializeField]
    private SelectedItem selectedItem;

    private void Awake()
    {
        weapon = GameObject.Find("WeaponSlot").GetComponent<EquipmentSlot>();
    }

    private void Start()
    {
    }

    public static void PutOn(int item, int subType, int inventorySlot)
    {
        if (subType == 0)
        {
            Inventory.EquipmentList[inventorySlot].GetComponent<SlotScript>().SetItem(weapon.Item, 0);
            GameObject.Find("Player(Clone)").GetComponent<PlayerScript>().PutOnEquipment(item);
            weapon.Item = item;
        }
    }
    public static void TakeOff(int subType, int index)
    {
        if (subType == 0)
        {
            Inventory.AddItem(0, weapon.Item, 1, index);
            weapon.TakeOff();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        GameObject clickedObject = eventData.pointerCurrentRaycast.gameObject;
        string stringSubType;
        int subType;
        if (clickedObject.name == "Image")
            stringSubType = clickedObject.transform.parent.name;
        else if (clickedObject.transform.name.Contains("Slot"))
            stringSubType = clickedObject.transform.name;
        else stringSubType = "";

        switch (stringSubType)
        {
            case "WeaponSlot":
                subType = 0;
                break;
            default:
                subType = -1;
                break;
        }

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (selectedItem.IsSelected)
            {
                if (selectedItem.Owner == SelectedItem.OwnerType.inventory && Inventory.Tab == 0)
                {
                    int ivItem = Inventory.EquipmentList[selectedItem.SelectedItemIndex].GetComponent<SlotScript>().Item;
                    Debug.Log(clickedObject.name);
                    if (ivItem / 10000 == subType && ivItem != 0)
                        PacketManager.Instance.UseItem(0, selectedItem.SelectedItemIndex);
                }
                else if (selectedItem.Owner == SelectedItem.OwnerType.equipment)
                {
                    if (selectedItem.SelectedItemIndex == subType && eventData.clickCount == 2)
                        PacketManager.Instance.TakeOffEquipment(subType, -1);
                }
                selectedItem.IsSelected = false;
            }
            else
            {
                if (clickedObject.name == "Image")
                {
                    selectedItem.SelectedItemImage.sprite = clickedObject.GetComponent<Image>().sprite;
                    selectedItem.SelectedItemIndex = subType;
                    selectedItem.IsSelected = true;
                    selectedItem.Owner = SelectedItem.OwnerType.equipment;
                }
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (clickedObject.name == "Image")
            {
                PacketManager.Instance.TakeOffEquipment(subType, -1);
            }
        }


        //    throw new System.NotImplementedException();
    }

    public static void SetEquipment(List<int> item)
    {
        foreach (int i in item)
        {
            if (i / 10000 == 0)
            {
                weapon.Item = i;
            }
        }
    }
}