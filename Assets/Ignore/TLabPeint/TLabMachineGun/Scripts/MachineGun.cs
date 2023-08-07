using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineGun : MonoBehaviour
{
    public GameObject bullet;

    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            Shot();
        }
    }

    public void Shot()
    {
        GameObject ball =
            (GameObject)Instantiate(bullet, transform.position + new Vector3(0f, 0f, 1.25f), Quaternion.identity);
        Rigidbody ballRigidbody = ball.GetComponent<Rigidbody>();
        ballRigidbody.AddForce(transform.forward * 500);
    }
}
