using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heart : Cube
{
    public Rigidbody2D rb;

    private float vertical;
    private float horizontal;

    void Start()
    {
        heartParent = this;
    }

    void Update()
    {
        if (Input.GetKey(rotateShipRightKey))
        {
            transform.Rotate(new Vector3(0, 0, -5) * Time.deltaTime * 10, Space.World);
        }
        if (Input.GetKey(rotateShipLeftKey))
        {
            transform.Rotate(new Vector3(0, 0, 5) * Time.deltaTime * 10, Space.World);
        }
    }

    private void FixedUpdate()
    {
        if (Input.GetKey(accelerateKey))
        {
            vertical = Input.GetAxis("Vertical");
            rb.AddForce((transform.up * vertical) * speed);
        }
        
    }

    public override void OnMouseDown()
    {}

    public override void OnMouseDrag()
    {}

    public override void OnMouseUp()
    {}
}
