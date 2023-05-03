using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private float speed = 5;
    [SerializeField] private float rotation_speed = 3;
    private Vector3 rotation = Vector3.zero;

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
        transform.eulerAngles = rotation * rotation_speed;

    }
}
