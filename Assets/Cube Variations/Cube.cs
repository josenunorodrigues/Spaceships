using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    public KeyCode rotateKey = KeyCode.R;
    public float speed = 1.5f;
    public int tier = 1;
    public int health = 20;
    private int id;
    private LineRenderer line;
    public float snapOffset = 0;
    public int blockWidth = 1;
    public int blockHeight = 1;
    public Heart heartParent;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Rigidbody2D>().transform.localScale = new Vector3(blockWidth, blockHeight, 1);
        line = gameObject.GetComponent<LineRenderer>();
        line.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 3f);
    }

    Vector3 offsetValue;
    public Vector2 gameObjectSreenPoint;
    public Vector3 mousePreviousLocation;
    public Vector3 mouseCurLocation;
    Collider2D[] hitColliders;
    Collider2D nearestCollider = null;

    public virtual void OnMouseDown()
    {
        if (transform.parent != null && GetComponent<Rigidbody2D>() != null)
        {
            transform.SetParent(null);
            transform.gameObject.AddComponent<Rigidbody2D>();
            GetComponent<Rigidbody2D>().angularDrag = 1;
        }
        //This grabs the position of the object in the world and turns it into the position on the screen
        gameObjectSreenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        //Sets the mouse pointers vector3
        mousePreviousLocation = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        offsetValue = GetComponent<Rigidbody2D>().transform.position - Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));

    }

    public Vector2 force;
    public float topSpeed = 10;
    public virtual void OnMouseDrag()
    {
        if (!transform.parent)
        {
            mouseCurLocation = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
            force = mouseCurLocation - mousePreviousLocation;//Changes the force to be applied
            mousePreviousLocation = mouseCurLocation;

            GetComponent<Rigidbody2D>().transform.position = new Vector2((Camera.main.ScreenToWorldPoint(mouseCurLocation) + offsetValue).x, (Camera.main.ScreenToWorldPoint(mouseCurLocation) + offsetValue).y);
            GetComponent<Rigidbody2D>().velocity = force;
            hitColliders = Physics2D.OverlapCircleAll(GetComponent<Rigidbody2D>().transform.position, 3f);
            Debug.Log(hitColliders);
            if (hitColliders.Length != 1)
            {

                float minSqrDistance = Mathf.Infinity;

                for (int i = 0; i < hitColliders.Length; i++)
                {
                    if (hitColliders[i].name != name)
                    {
                        Debug.Log(GetComponent<Rigidbody2D>().transform.position + " " + hitColliders[i].transform.position);
                        float sqrDistanceToCenter = (GetComponent<Rigidbody2D>().transform.position - hitColliders[i].transform.position).sqrMagnitude;
                        if (sqrDistanceToCenter < minSqrDistance)
                        {
                            minSqrDistance = sqrDistanceToCenter;
                            nearestCollider = hitColliders[i];
                        }
                    }
                }
                //Debug.Log(nearestCollider);
                try
                {
                    Cube heartParent = getHeartParent(nearestCollider.GetComponent<Cube>());
                    if (heartParent)
                    {
                        this.heartParent = (Heart)heartParent;
                        line.enabled = true;
                        line.SetPosition(0, transform.position);
                        line.SetPosition(1, nearestCollider.transform.position);
                    }
                    else
                    {
                        line.enabled = false;
                    }
                }
                catch (Exception e)
                {
                    Cube heartParent = getHeartParent(nearestCollider.GetComponent<Heart>());
                    if (heartParent)
                    {
                        this.heartParent = (Heart)heartParent;
                        line.enabled = true;
                        line.SetPosition(0, transform.position);
                        line.SetPosition(1, nearestCollider.transform.position);
                    }
                    else
                    {
                        line.enabled = false;
                    }
                }
               
            }
            else
            {
                nearestCollider = null;
                line.enabled = false;
            }

            if (Input.GetKey(rotateKey))
            {
                Debug.Log(GetComponent<Rigidbody2D>().transform.rotation);
            }
        }
    }

    private Cube getHeartParent(Cube nearestCube)
    {
        if (nearestCube.name == "Heart") return nearestCube;
        if (nearestCube.transform.parent) return getHeartParent(nearestCube.transform.parent.GetComponent<Cube>());
        return null;
    }

    public virtual void OnMouseUp()
    {
        if (!transform.parent)
        {
            line.enabled = false;

            //Makes sure there isn't a ludicrous speed
            if (GetComponent<Rigidbody2D>().velocity.magnitude > topSpeed)
                force = GetComponent<Rigidbody2D>().velocity.normalized * topSpeed;

            // fix piece to the ship
            if ((nearestCollider && nearestCollider.name == "Heart") || nearestCollider.transform.parent)
            {
                transform.SetParent(nearestCollider.transform);
                Destroy(transform.GetComponent<Rigidbody2D>());

                float xDiff = nearestCollider.transform.position.x - transform.position.x;
                float yDiff = nearestCollider.transform.position.y - transform.position.y;
                Debug.Log("x:" + xDiff + " y: " + yDiff + " " + (xDiff > yDiff));
                //Cube nearCube = GameObject.FindGameObjectWithTag(nearestCollider.name);
                
                if (Math.Abs(xDiff) < Math.Abs(yDiff) && yDiff > 0)
                {
                    //if Down
                    //heartParent.GetComponent<Rigidbody2D>().transform.right
                    Debug.Log("Down");
                    GetComponent<Rigidbody2D>().transform.position = new Vector3(nearestCollider.transform.position.x, nearestCollider.transform.position.y - nearestCollider.GetComponent<Cube>().blockHeight, nearestCollider.transform.position.z);
                    //Debug.Log(nearestCollider.transform.position.x + " " + (nearestCollider.transform.position.y - nearestCollider.GetComponent<Cube>().blockHeight));
                }
                else if (Math.Abs(xDiff) > Math.Abs(yDiff) && xDiff > 0)
                {
                    //if Left
                    Debug.Log("Up");
                    GetComponent<Rigidbody2D>().transform.position = new Vector3(nearestCollider.transform.position.x - nearestCollider.GetComponent<Cube>().blockWidth, nearestCollider.transform.position.y, nearestCollider.transform.position.z);
                    //Debug.Log(nearestCollider.transform.position.x - nearestCollider.GetComponent<Cube>().blockWidth + " " + nearestCollider.transform.position.y);
                }
                else if (Math.Abs(xDiff) > Math.Abs(yDiff) && xDiff < 0)
                {
                    //if Right
                    Debug.Log("Right");
                    GetComponent<Rigidbody2D>().transform.position = new Vector3(nearestCollider.transform.position.x + nearestCollider.GetComponent<Cube>().blockWidth, nearestCollider.transform.position.y, nearestCollider.transform.position.z);
                    //Debug.Log((nearestCollider.transform.position.x + nearestCollider.GetComponent<Cube>().blockWidth) + " " + nearestCollider.transform.position.y);

                }
                else if (Math.Abs(xDiff) < Math.Abs(yDiff) && yDiff < 0)
                {
                    //if Left
                    Debug.Log("Left");

                    GetComponent<Rigidbody2D>().transform.position = new Vector3(nearestCollider.transform.position.x, nearestCollider.transform.position.y + nearestCollider.GetComponent<Cube>().blockHeight, nearestCollider.transform.position.z);
                    //Debug.Log(nearestCollider.transform.position.x + " " + (nearestCollider.transform.position.y + nearestCollider.GetComponent<Cube>().blockHeight));
                }
            }

            hitColliders = null;
            nearestCollider = null;
        }
    }
}
