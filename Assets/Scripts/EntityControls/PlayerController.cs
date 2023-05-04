using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    
    [SerializeField] private float speed = 8;
    private Vector3 rotation;
    
    private void Awake()
    {
        rotation = new(30, 180, 0);
    }

    void Update()
    {
        var dir = new Vector3(Input.GetAxis("Horizontal"),0,Input.GetAxis("Vertical"));
        transform.Translate(dir * speed * Time.deltaTime);

        if(Input.GetKey(KeyCode.Space))
        {
            transform.position += transform.up * speed * Time.deltaTime;
        }
        if(Input.GetKey(KeyCode.LeftControl))
        {
            transform.position -= transform.up * speed * Time.deltaTime;
        }

        rotation.x -= Input.GetAxis("Mouse Y");
        rotation.y += Input.GetAxis("Mouse X");
        rotation.x = Mathf.Clamp(rotation.x, -70, 70);
        transform.localEulerAngles = rotation;

    }
}
