using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Inventory : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
{
    private static GameObject m_equipmentSlots;
    private static GameObject m_useableSlots;
    private static GameObject m_etcSlots;
    private static GameObject m_enhancementSlots;

    private static List<RectTransform> m_equipmentList = null;
    private static List<RectTransform> m_useableList = null;
    private static List<RectTransform> m_etcList = null;
    private static List<RectTransform> m_enhancementList = null;

    private Image m_imgEquipmentTab;
    private Image m_imgUseableTab;
    private Image m_imgEtcTab;
    private Image m_imgEnhancementTab;

    private Color m_enableColor = new Color(1f, 1f, 1f, 1f);
    private Color m_disableColor = new Color(1f, 1f, 1f, 100 / 255f);

    [SerializeField]
    private SelectedItem selectedItem;

    private static int tab = -1;

    // Start is called before the first frame update
    void Start()
    {
        m_equipmentSlots = GameObject.Find("EquipmentSlots");
        m_useableSlots = GameObject.Find("UseableSlots");
        m_etcSlots = GameObject.Find("EtcSlots");
        m_enhancementSlots = GameObject.Find("EnhancementSlots");

        var equipmentTab = GameObject.Find("EquipmentTab");
        var useableTab = GameObject.Find("UseableTab");
        var etcTab = GameObject.Find("EtcTab");
        var enhancementTab = GameObject.Find("EnhancementTab");

        equipmentTab.GetComponent<Button>().onClick.AddListener(EquipmentTabClick);
        useableTab.GetComponent<Button>().onClick.AddListener(UseableTabClick);
        etcTab.GetComponent<Button>().onClick.AddListener(EtcTabClick);
        enhancementTab.GetComponent<Button>().onClick.AddListener(EnhancementTabClick);

        m_imgEquipmentTab = equipmentTab.GetComponent<Image>();
        m_imgUseableTab = useableTab.GetComponent<Image>();
        m_imgEtcTab = etcTab.GetComponent<Image>();
        m_imgEnhancementTab = enhancementTab.GetComponent<Image>();

        PacketManager.Instance.PlayerSetting(InitObject.playerNicname);
        EquipmentTabClick();
    }

    public static void SetInventories(List<int> item, List<sbyte> type, List<short> count, List<short> slot, int maxEquipmentSlot, int maxUseableSlot, int maxEtcSlot, int maxEnhancementSlot)
    {
        m_equipmentList = new List<RectTransform>();
        m_useableList = new List<RectTransform>();
        m_etcList = new List<RectTransform>();
        m_enhancementList = new List<RectTransform>();

        GameObject Slot = Resources.Load("Slot") as GameObject;
        RectTransform r = Slot.GetComponent<RectTransform>();

        m_equipmentSlots.SetActive(true);
        m_useableSlots.SetActive(false);
        m_etcSlots.SetActive(false);
        m_enhancementSlots.SetActive(false);

        for (int i = 0; maxEquipmentSlot > i; i++)
        {
            m_equipmentList.Add(MonoBehaviour.Instantiate(r));
            m_equipmentList[i].name = "EquipmentSlot " + i;
            m_equipmentList[i].transform.SetParent(m_equipmentSlots.transform);
            m_equipmentList[i].transform.localScale = new Vector2(1f, 1f);
            m_equipmentList[i].transform.GetChild(0).gameObject.SetActive(false);
        }
        for (int i = 0; maxUseableSlot > i; i++)
        {
            m_useableList.Add(MonoBehaviour.Instantiate(r));
            m_useableList[i].name = "UseableSlot " + i;
            m_useableList[i].transform.SetParent(m_useableSlots.transform);
            m_useableList[i].transform.localScale = new Vector2(1f, 1f);
            m_useableList[i].transform.GetChild(0).gameObject.SetActive(false);
        }
        for (int i = 0; maxEtcSlot > i; i++)
        {
            m_etcList.Add(MonoBehaviour.Instantiate(r));
            m_etcList[i].name = "EtcSlot " + i;
            m_etcList[i].transform.SetParent(m_etcSlots.transform);
            m_etcList[i].transform.localScale = new Vector2(1f, 1f);
            m_etcList[i].transform.GetChild(0).gameObject.SetActive(false);
        }
        for (int i = 0; maxEnhancementSlot > i; i++)
        {
            m_enhancementList.Add(MonoBehaviour.Instantiate(r));
            m_enhancementList[i].name = "EnhancementSlot " + i;
            m_enhancementList[i].transform.SetParent(m_enhancementSlots.transform);
            m_enhancementList[i].transform.localScale = new Vector2(1f, 1f);
            m_enhancementList[i].transform.GetChild(0).gameObject.SetActive(false);
        }
        for (int i = 0; i < item.Count; i++)
        {
            switch (type[i])
            {
                case 0:
                    m_equipmentList[slot[i]].GetComponent<SlotScript>().SetItem(item[i], 0);
                    break;
                case 1:
                    m_useableList[slot[i]].GetComponent<SlotScript>().SetItem(item[i], 1);
                    break;
                case 2:
                    m_etcList[slot[i]].GetComponent<SlotScript>().SetItem(item[i], 2);
                    break;
                case 3:
                    m_enhancementList[slot[i]].GetComponent<SlotScript>().SetItem(item[i], 3);
                    break;
                default:
                    break;
            }
        }
    }

    public static void AddItem(int type, int item, int count, int index)
    {

        if (type == 0)
        {
            if (index == -1)
            {
                for (int i = 0; i < m_equipmentList.Count; i++)
                {
                    if (m_equipmentList[i].GetComponent<SlotScript>().Item == 0)
                    {
                        m_equipmentList[i].GetComponent<SlotScript>().SetItem(item, 0);
                        break;
                    }
                }
            }
            else m_equipmentList[index].GetComponent<SlotScript>().SetItem(item, 0);
        }
    }

    public static List<RectTransform> EquipmentList
    {
        get
        {
            return m_equipmentList;
        }
    }

    private void EquipmentTabClick()
    {
        if (tab != 0)
        {
            m_imgEquipmentTab.color = m_enableColor;
            m_imgUseableTab.color = m_disableColor;
            m_imgEtcTab.color = m_disableColor;
            m_imgEnhancementTab.color = m_disableColor;

            m_equipmentSlots.SetActive(true);
            m_useableSlots.SetActive(false);
            m_etcSlots.SetActive(false);
            m_enhancementSlots.SetActive(false);

            selectedItem.IsSelected = false;

            tab = 0;
        }
    }

    private void UseableTabClick()
    {
        if (tab != 1)
        {
            m_imgEquipmentTab.color = m_disableColor;
            m_imgUseableTab.color = m_enableColor;
            m_imgEtcTab.color = m_disableColor;
            m_imgEnhancementTab.color = m_disableColor;

            m_equipmentSlots.SetActive(false);
            m_useableSlots.SetActive(true);
            m_etcSlots.SetActive(false);
            m_enhancementSlots.SetActive(false);

            selectedItem.IsSelected = false;

            tab = 1;
        }
    }

    private void EtcTabClick()
    {
        if (tab != 2)
        {
            m_imgEquipmentTab.color = m_disableColor;
            m_imgUseableTab.color = m_disableColor;
            m_imgEtcTab.color = m_enableColor;
            m_imgEnhancementTab.color = m_disableColor;

            m_equipmentSlots.SetActive(false);
            m_useableSlots.SetActive(false);
            m_etcSlots.SetActive(true);
            m_enhancementSlots.SetActive(false);

            selectedItem.IsSelected = false;

            tab = 2;
        }
    }

    private void EnhancementTabClick()
    {
        if (tab != 3)
        {
            m_imgEquipmentTab.color = m_disableColor;
            m_imgUseableTab.color = m_disableColor;
            m_imgEtcTab.color = m_disableColor;
            m_imgEnhancementTab.color = m_enableColor;

            m_equipmentSlots.SetActive(false);
            m_useableSlots.SetActive(false);
            m_etcSlots.SetActive(false);
            m_enhancementSlots.SetActive(true);

            selectedItem.IsSelected = false;

            tab = 3;
        }
    }

    public static void SwapSlot(int tab, int slot1, int slot2)
    {
        int tmp;
        string strSlots = null;
        switch (tab)
        {
            case 0:
                strSlots = "EquipmentSlots";
                break;
            case 1:
                strSlots = "UseableSlots";
                break;
            case 2:
                strSlots = "EtcSlots";
                break;
            case 3:
                strSlots = "EnhancementSlots";
                break;
        }
        SlotScript slt1 = GameObject.Find(strSlots).transform.GetChild(slot1).GetComponent<SlotScript>();
        SlotScript slt2 = GameObject.Find(strSlots).transform.GetChild(slot2).GetComponent<SlotScript>();
        tmp = slt1.Item;
        slt1.SetItem(slt2.Item, tab);
        slt2.SetItem(tmp, tab);
    }

    public static void UseItem(int tab, int slot, int item)
    {
        int subType;
        switch (tab)
        {
            case 0: // Equipment
                subType = item / 10000;
                switch (subType)
                {
                    case 0: // Weapon
                        Equipment.PutOn(item, 0, slot);
                        break;
                }

                break;
        }
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        GameObject clickedObject = eventData.pointerCurrentRaycast.gameObject;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (selectedItem.IsSelected)
            {
                int selectIvIndex;

                if (clickedObject.name == "Image")
                    selectIvIndex = int.Parse(Regex.Replace(clickedObject.transform.parent.name, @"\D", ""));
                else if (clickedObject.transform.parent.name.Contains("Slots"))
                    selectIvIndex = int.Parse(Regex.Replace(clickedObject.transform.name, @"\D", ""));
                else selectIvIndex = -1;

                if (selectedItem.Owner == SelectedItem.OwnerType.inventory)
                {
                    if (selectIvIndex != -1)
                    {
                        Debug.Log(selectedItem.SelectedItemIndex + " " + selectIvIndex);
                        if (selectedItem.SelectedItemIndex == selectIvIndex)
                        {
                            if (eventData.clickCount == 2)
                                PacketManager.Instance.UseItem(0, selectIvIndex);
                        }
                        else PacketManager.Instance.ChangeItemSlot((sbyte)tab, (short)selectedItem.SelectedItemIndex, (short)selectIvIndex);
                    }
                }
                else if ((selectedItem.Owner == SelectedItem.OwnerType.equipment) && tab == 0 && selectIvIndex != -1)
                {
                    int selectItem = EquipmentList[selectIvIndex].GetComponent<SlotScript>().Item;
                    if (selectItem == 0)
                        PacketManager.Instance.TakeOffEquipment(selectedItem.SelectedItemIndex, selectIvIndex);
                    else if ((selectItem / 10000) == selectedItem.SelectedItemIndex)
                        PacketManager.Instance.UseItem(0, selectIvIndex);
                }

                selectedItem.IsSelected = false;
            }
            else
            {
                if (clickedObject.name == "Image")
                {
                    selectedItem.SelectedItemImage.sprite = clickedObject.GetComponent<Image>().sprite;
                    selectedItem.SelectedItemIndex = int.Parse(Regex.Replace(clickedObject.transform.parent.name, @"\D", ""));
                    selectedItem.IsSelected = true;
                    selectedItem.Owner = SelectedItem.OwnerType.inventory;
                }
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (clickedObject.name == "Image")
            {
                string slotName = clickedObject.transform.parent.name;
                if (slotName.Contains("EquipmentSlot"))
                {
                    PacketManager.Instance.UseItem(0, int.Parse(Regex.Replace(slotName, @"\D", "")));
                }
            }
        }


        //    throw new System.NotImplementedException();
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        if (selectedItem.IsSelected)
        {
        }
        //   throw new System.NotImplementedException();
    }

    public static int Tab
    {
        get
        {
            return tab;
        }
    }
}