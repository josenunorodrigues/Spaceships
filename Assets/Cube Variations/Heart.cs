using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heart : Cube
{


    void Start()
    {
        
    }

    void Update()
    {
        if(Input.GetKey(KeyCode.W))
        {
            GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, 1f));
        }
        else if (Input.GetKey(KeyCode.D))
        {
            GetComponent<Rigidbody2D>().AddForce(new Vector2(1f, 0f));
        }
        else if (Input.GetKey(KeyCode.A))
        {
            GetComponent<Rigidbody2D>().AddForce(new Vector2(-1f, 0f));
        }
    }

    public override void OnMouseDown()
    {}

    public override void OnMouseDrag()
    {}

    public override void OnMouseUp()
    {}
}
