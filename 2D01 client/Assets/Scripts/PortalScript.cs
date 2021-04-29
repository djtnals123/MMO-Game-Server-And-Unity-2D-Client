using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalScript : MonoBehaviour
{
 //   [SerializeField]
 //   private int map;
    [SerializeField]
    private int portal;
    /*
    public int Map
    {
        get
        {
            return map;
        }
        set
        {
            map = value;
        }
    }
    */
    public int Portal
    {
        get
        {
            return portal;
        }
        set
        {
            portal = value;
        }
    }
}
