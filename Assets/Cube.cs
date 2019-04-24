using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    public int tier = 1;
    public int health = 20;
    private int id;
    private LineRenderer line;

    // Start is called before the first frame update
    void Start()
    {
        line = gameObject.GetComponent<LineRenderer>();
        line.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    Vector3 offsetValue;
    public Vector2 gameObjectSreenPoint;
    public Vector3 mousePreviousLocation;
    public Vector3 mouseCurLocation;
    Collider2D[] hitColliders;
    Collider2D nearestCollider = null;

    public virtual void OnMouseDown()
    {
        if (transform.parent != null && GetComponent<Rigidbody2D>()!= null)
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
            hitColliders = Physics2D.OverlapCircleAll(GetComponent<Rigidbody2D>().transform.position, 1.5f);

            if (hitColliders.Length != 1)
            {

                float minSqrDistance = Mathf.Infinity;

                for (int i = 0; i < hitColliders.Length; i++)
                {
                    if(hitColliders[i].name != this.name)
                    {

                        float sqrDistanceToCenter = (GetComponent<Rigidbody2D>().transform.position - hitColliders[i].transform.position).sqrMagnitude;
                        if (sqrDistanceToCenter < minSqrDistance)
                        {
                            minSqrDistance = sqrDistanceToCenter;
                            nearestCollider = hitColliders[i];
                        }
                    }
                }
                if ((nearestCollider && nearestCollider.name == "heart") || nearestCollider.transform.parent)
                {
                    line.enabled = true;
                    line.SetPosition(0, transform.position);
                    line.SetPosition(1, nearestCollider.transform.position);
                }
                else
                {
                    line.enabled = false;
                }
            }
            else
            {
                nearestCollider = null;
                line.enabled = false;
            }
        }
    }

    public virtual void OnMouseUp()
    {
        if (!transform.parent)
        {
            line.enabled = false;

            //Makes sure there isn't a ludicrous speed
            if (GetComponent<Rigidbody2D>().velocity.magnitude > topSpeed)
                force = GetComponent<Rigidbody2D>().velocity.normalized * topSpeed;

            if ((nearestCollider && nearestCollider.name == "heart") || nearestCollider.transform.parent )
            {
                transform.SetParent(nearestCollider.transform);
                Destroy(transform.GetComponent<Rigidbody2D>());
            }
        
            hitColliders = null;
            nearestCollider = null;
        }
    }
}
