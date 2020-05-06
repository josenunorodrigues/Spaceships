using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heart : Cube
{
    public KeyCode rotateShipLeftKey = KeyCode.A;
    public KeyCode rotateShipRightKey = KeyCode.D;
    public KeyCode accelerateKey = KeyCode.W;
    public Rigidbody2D rb;

    private float vertical;
    private float horizontal;

    void Start()
    {
        heartParent = this;
    }

    void Update()
    {
    }

    private void FixedUpdate()
    {
        if (Input.GetKey(accelerateKey))
        {
            print(GetComponent<Rigidbody2D>().velocity + " " + transform.forward + " " + vertical + " " + speed);

            vertical = Input.GetAxis("Vertical");
            rb.AddForce((transform.up * vertical) * speed);
            //GetComponent<Rigidbody2D>().velocity = (transform.forward * vertical) * speed;
        }
        if (Input.GetKey(rotateShipRightKey))
        {
            transform.Rotate(new Vector3(0, 0, -5) * Time.fixedDeltaTime * 10, Space.World);
        }
        if (Input.GetKey(rotateShipLeftKey))
        {
            transform.Rotate(new Vector3(0, 0, 5) * Time.fixedDeltaTime * 10, Space.World);
        }
    }

    public override void OnMouseDown()
    {}

    public override void OnMouseDrag()
    {}

    public override void OnMouseUp()
    {}
}
