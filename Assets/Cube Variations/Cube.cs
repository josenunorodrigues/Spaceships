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
    public int length = 1;
    public Heart heartParent;
    public float colliderRadius = 3f;
    Cube north, south, east, west;
    // Start is called before the first frame update

    // Snapping e Rotação
    // quando o bloco entra no alcance para dar snap, o bloco fica com a mesma rotação do nearestCollider e sofre dos inputs de rotação que a nave também recebe
    
    // Detecção de Blocos em redor
    // collider em cada face para saber qual é o bloco nessa face
    
    // Adicionar Blocos
    // quando se adiciona um bloco à nave, vai-se buscar todos os blocos em redor e ver qual é o bloco que tem o menor caminho ao heart e adicionar mais 1 ao caminho deste bloco. E actualizar todos os blocos em redor cujos caminhos sejam maiores que o seu número mais ou menos 1.
    
    // Remover Blocos
    // ao remover, verificar os blocos em redor e verificar se o caminho é maior. Se for, continuar a verificar os blocos em cadeia. Se houver um bloco cujo caminho seja menor que o próprio, não é removido mais nenhum bloco da cadeia. Se não houver nenhum bloco com caminho menor, todos os blocos da cadeia são removidos.

    void Start()
    {
        transform.localScale = new Vector3(length, length, 1);

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
        Gizmos.DrawWireSphere(transform.position, colliderRadius);
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
        offsetValue = transform.position - Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));

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

            transform.position = new Vector2((Camera.main.ScreenToWorldPoint(mouseCurLocation) + offsetValue).x, (Camera.main.ScreenToWorldPoint(mouseCurLocation) + offsetValue).y);
            GetComponent<Rigidbody2D>().velocity = force;
            hitColliders = Physics2D.OverlapCircleAll(transform.position, colliderRadius);
 
            nearestCollider = null;
            if (hitColliders.Length != 1)
            {
                float minSqrDistance = Mathf.Infinity;

                for (int i = 0; i < hitColliders.Length; i++)
                {
                    if (hitColliders[i].name != name)
                    {
                        float sqrDistanceToCenter = (transform.position - hitColliders[i].transform.position).sqrMagnitude;
                        if ((sqrDistanceToCenter < minSqrDistance) && (hitColliders[i].name == "Heart" || getHeartParent(hitColliders[i].GetComponent<Cube>())))
                        {
                            minSqrDistance = sqrDistanceToCenter;
                            nearestCollider = hitColliders[i];
                        }
                    }
                }
                if (nearestCollider)
                {
                    Cube heartParent = getHeartParent(nearestCollider.GetComponent(typeof(Cube)) as Cube);
                    if (heartParent)
                    {
                        this.heartParent = (Heart)heartParent;
                        line.enabled = true;
                        line.SetPosition(0, transform.position);
                        line.SetPosition(1, nearestCollider.transform.position);
                    }
                    else
                    {
                        nearestCollider = null;
                        line.enabled = false;
                    }
                }
                else
                {
                    nearestCollider = null;
                    line.enabled = false;
                }
            }
            else
            {
                nearestCollider = null;
                line.enabled = false;
            }

            if (Input.GetKey(rotateKey))
            {
                transform.Rotate(0, 90, 0);
                Debug.Log(transform.rotation);
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
            if (nearestCollider && (nearestCollider.name == "Heart" || nearestCollider.transform.parent))
            {
                Debug.Log(nearestCollider.transform.position);
                transform.SetParent(nearestCollider.transform);
                Destroy(transform.GetComponent<Rigidbody2D>());

                float xDiff = nearestCollider.transform.position.x - transform.position.x;
                float yDiff = nearestCollider.transform.position.y - transform.position.y;
                Debug.Log("x:" + xDiff + " y: " + yDiff + " " + (xDiff > yDiff));
                
                if (Math.Abs(xDiff) < Math.Abs(yDiff) && yDiff > 0)
                {
                    Debug.Log(Mathf.Round(nearestCollider.transform.position.x));
                    //if Down
                    Debug.Log("Down");
                    transform.position = new Vector3(
                        nearestCollider.transform.position.x,
                        nearestCollider.transform.position.y - nearestCollider.GetComponent<Cube>().length,
                        nearestCollider.transform.position.z
                    );
                }
                else if (Math.Abs(xDiff) > Math.Abs(yDiff) && xDiff > 0)
                {
                    //if Left
                    Debug.Log("Left");
                    transform.position = new Vector3(
                        nearestCollider.transform.position.x - nearestCollider.GetComponent<Cube>().length,
                        nearestCollider.transform.position.y,
                        nearestCollider.transform.position.z
                    );
                }
                else if (Math.Abs(xDiff) > Math.Abs(yDiff) && xDiff < 0)
                {
                    //if Right
                    Debug.Log("Right");
                    transform.position = new Vector3(
                        nearestCollider.transform.position.x + nearestCollider.GetComponent<Cube>().length,
                        nearestCollider.transform.position.y,
                        nearestCollider.transform.position.z
                    );
                }
                else if (Math.Abs(xDiff) < Math.Abs(yDiff) && yDiff < 0)
                {
                    //if Up
                    Debug.Log("Up");
                    transform.position = new Vector3(
                        nearestCollider.transform.position.x,
                        nearestCollider.transform.position.y + nearestCollider.GetComponent<Cube>().length,
                        nearestCollider.transform.position.z
                    );
                }
            }

            hitColliders = null;
            nearestCollider = null;
        }
    }
}
