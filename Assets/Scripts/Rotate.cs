using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public  bool DoRotate;
    [SerializeField] float _speed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (DoRotate)
        {
            float x = transform.rotation.eulerAngles.x;
            float y = transform.rotation.eulerAngles.y + _speed * Time.deltaTime;
            float z = transform.rotation.eulerAngles.z;

            transform.rotation = Quaternion.Euler(x,y,z);
        }
    }
}
