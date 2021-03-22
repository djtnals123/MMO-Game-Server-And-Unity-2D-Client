using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    int dir;
    void Start()
    {
        Destroy(gameObject, 111f);

    }

    // Update is called once per frame
    void Update()
    {
    }
    
    void DirRPC(int dir) => this.dir = dir;

    void DestroyRPC() => Destroy(gameObject);

}
