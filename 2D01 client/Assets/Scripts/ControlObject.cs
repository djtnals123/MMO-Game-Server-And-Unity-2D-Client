using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlObject : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void SetObject(string name, float positionX, float positionY, float rotationX, float rotationY, float velocityX, float velocityY, float angularVelocity)
    {
        GameObject player = GameObject.Find("Player " + name);
        if (player != null)
        {
            Rigidbody2D rd = player.GetComponent<Rigidbody2D>();
            rd.velocity = new Vector3(velocityX, velocityY);
            rd.transform.position = new Vector3(positionX, positionY);
            rd.transform.rotation = Quaternion.Euler(rotationX, rotationY, 0);
            rd.angularVelocity = angularVelocity;
        }


    }
}
