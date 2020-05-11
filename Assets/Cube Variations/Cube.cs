using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Cube : MonoBehaviour
{
    public KeyCode rotateShipLeftKey = KeyCode.A;
    public KeyCode rotateShipRightKey = KeyCode.D;
    public KeyCode accelerateKey = KeyCode.W;
    public KeyCode rotateKey = KeyCode.R;
    public float speed = 1.5f;
    public int tier = 1;
    public int health = 20;
    private int id;
    private LineRenderer line;
    public int length = 1;
    public Heart heartParent;
    public float colliderRadius = 3f;
    Cube cubeNorth, cubeEast, cubeSouth, cubeWest;
    bool socketNorth = true, socketEast = true, socketSouth = true, socketWest = true;
    public CircleCollider2D colliderNorth, colliderEast, colliderSouth, colliderWest;
    public Vector2 force;
    public float topSpeed = 10;
    Direction ?dirToNearestCollider;
    enum Direction { North, East, South, West }
    Vector3 offsetValue;
    public Vector3 mousePreviousLocation;
    Collider2D[] hitColliders;
    public GameObject nearestCube = null;
    bool rotating = false;
    float rotationSpeed = 10f;
    Quaternion targetRotation;
    public float rotationTime;
    // Start is called before the first frame update

    // Snapping e Rotação
    // quando o bloco entra no alcance para dar snap, o bloco fica com a mesma rotação do nearestCube e sofre dos inputs de rotação que a nave também recebe

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
        if (rotating)
        {
            rotationTime += Time.deltaTime * rotationSpeed;
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationTime);
        }
        if (rotationTime > 1)
        {
            rotating = false;
        }
    }
    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, colliderRadius);
    }
    bool canAddBlock(float angle)
    {
        if(angle < 315 && angle > 225)
        {
            if (socketNorth && cubeNorth == null)
            {
                dirToNearestCollider = Direction.North;
                return true;
            }
            return false;
        }
        else if (angle < 225 && angle > 135)
        {
            if (socketEast && cubeEast == null)
            {
                dirToNearestCollider = Direction.East;
                return true;
            }
            return false;
        }
        else if (angle < 135 && angle > 45)
        {
            if (socketSouth && cubeSouth == null)
            {
                dirToNearestCollider = Direction.South;
                return true;
            }
            return false;
        }
        else if (angle < 45 && angle > 0 || angle < 360 && angle > 315)
        {
            if (socketWest && cubeWest == null)
            {
                dirToNearestCollider = Direction.West;
                return true;
            }
            return false;
        }
        return false;
    }
    public virtual void OnMouseDown()
    {
        if (transform.parent != null && GetComponent<Rigidbody2D>() != null)
        {
            transform.SetParent(null);
            transform.gameObject.AddComponent<Rigidbody2D>();
            GetComponent<Rigidbody2D>().angularDrag = 1;
        }
        offsetValue = transform.position - Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
        GetComponent<Rigidbody2D>().isKinematic = true;
        GetComponent<BoxCollider2D>().isTrigger = true;
        GetComponent<Rigidbody2D>().freezeRotation = true;
        GetComponent<Rigidbody2D>().freezeRotation = false;
    }
    public virtual void OnMouseDrag()
    {
        if (!transform.parent)
        {
            if (Input.GetKey(rotateShipRightKey))
            {
                transform.Rotate(new Vector3(0, 0, -5) * Time.deltaTime * 10, Space.World);
            }
            if (Input.GetKey(rotateShipLeftKey))
            {
                transform.Rotate(new Vector3(0, 0, 5) * Time.deltaTime * 10, Space.World);
            }
            if (Input.GetKeyDown(rotateKey))
            {
                if(!rotating)
                {
                    Quaternion targetRotation = transform.rotation;
                    targetRotation *= Quaternion.AngleAxis(90, Vector3.forward);
                    this.targetRotation = targetRotation;
                    rotationTime = 0;
                    rotating = true;
                }
            }
            Vector3 mouseCurLocation = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
            force = mouseCurLocation - mousePreviousLocation;//Changes the force to be applied
            mousePreviousLocation = mouseCurLocation;

            transform.position = new Vector2((Camera.main.ScreenToWorldPoint(mouseCurLocation) + offsetValue).x, (Camera.main.ScreenToWorldPoint(mouseCurLocation) + offsetValue).y);
            GetComponent<Rigidbody2D>().velocity = force;
            hitColliders = Physics2D.OverlapCircleAll(transform.position, colliderRadius, 1 << 8);

            if (hitColliders.Length > 1)
            {
                float minSqrDistance = Mathf.Infinity;

                GameObject cycleNearestCube = null;
                for (int i = 0; i < hitColliders.Length; i++)
                {
                    GameObject cube = hitColliders[i].transform.parent.gameObject;
                    if (cube.name != name)
                    {
                        float sqrDistanceToCenter = (transform.position - cube.transform.position).sqrMagnitude;
                        if ((sqrDistanceToCenter < minSqrDistance) && (cube.name == "Heart" || getHeartParent(cube.GetComponent<Cube>())))
                        {
                            minSqrDistance = sqrDistanceToCenter;
                            cycleNearestCube = cube;
                        }
                    }
                }
                if(cycleNearestCube)
                {
                    bool addedDifferentCube = false;
                    if (nearestCube == null)
                    {
                        addedDifferentCube = true;
                        nearestCube = cycleNearestCube;
                        Vector3 eulerAngles = nearestCube.transform.localEulerAngles;
                        List<float> angles = new List<float>();
                        angles.Add(eulerAngles.z);
                        for (int j = 0; j < 3; j++)
                        {
                            eulerAngles.z = (eulerAngles.z + 90) % 360;
                            angles.Add(eulerAngles.z);
                        }
                        float closest = angles.Aggregate((x, y) => Math.Abs(x - transform.localEulerAngles.z) < Math.Abs(y - transform.localEulerAngles.z) ? x : y);
                        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, closest);
                    }
                    else if (nearestCube.name != cycleNearestCube.name)
                    {
                        addedDifferentCube = true;
                        nearestCube = cycleNearestCube;
                    }
                    if(addedDifferentCube)
                    {
                        Gradient gradient = new Gradient();
                        GradientColorKey[] colorKey = new GradientColorKey[2];
                        colorKey[0].color = GetComponent<Renderer>().material.GetColor("_Color");
                        colorKey[0].time = 0.0f;
                        colorKey[1].color = nearestCube.GetComponent<Renderer>().material.GetColor("_Color");
                        colorKey[1].time = 1.0f;

                        // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
                        GradientAlphaKey[] alphaKey = new GradientAlphaKey[2];
                        alphaKey[0].alpha = 1.0f;
                        alphaKey[0].time = 0.0f;
                        alphaKey[1].alpha = 0.3f;
                        alphaKey[1].time = 1.0f;

                        gradient.SetKeys(colorKey, alphaKey);

                        //line.GetComponent<Renderer>().material.color = gradient.Evaluate(1);
                        line.colorGradient = gradient;

                        //line.SetColors(GetComponent<Renderer>().material.GetColor("_Color"), nearestCube.GetComponent<Renderer>().material.GetColor("_Color"));
                    }
                }
                else
                {
                    nearestCube = null;
                }
                if (nearestCube)
                {
                    Cube heartParent = getHeartParent(nearestCube.GetComponent(typeof(Cube)) as Cube);
                    if (heartParent)
                    {
                        Vector3 dir = transform.position - nearestCube.transform.position;
                        dir = nearestCube.transform.InverseTransformDirection(dir);
                        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 180;
                        if(canAddBlock(angle))
                        {
                            this.heartParent = (Heart)heartParent;
                            line.enabled = true;
                            line.SetPosition(0, transform.position);
                            line.SetPosition(1, nearestCube.transform.position);
                        }
                    }
                    else
                    {
                        nearestCube = null;
                        line.enabled = false;
                    }
                }
                else
                {
                    nearestCube = null;
                    line.enabled = false;
                }
            }
            else
            {
                nearestCube = null;
                line.enabled = false;
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
            GetComponent<Rigidbody2D>().isKinematic = false;
            GetComponent<BoxCollider2D>().isTrigger = false;
            if (GetComponent<Rigidbody2D>().velocity.magnitude > topSpeed)
                force = GetComponent<Rigidbody2D>().velocity.normalized * topSpeed;

            if (nearestCube && (nearestCube.name == "Heart" || nearestCube.transform.parent))
            {
                transform.SetParent(nearestCube.transform);
                Destroy(transform.GetComponent<Rigidbody2D>());

                if (dirToNearestCollider == Direction.North)
                {
                    transform.localPosition = new Vector3(0, nearestCube.GetComponent<Cube>().length, 0);
                }
                else if(dirToNearestCollider == Direction.South)
                {
                    transform.localPosition = new Vector3(0, -1 * nearestCube.GetComponent<Cube>().length, 0);
                }
                else if (dirToNearestCollider == Direction.West)
                {
                    transform.localPosition = new Vector3(-1 * nearestCube.GetComponent<Cube>().length, 0, 0);
                }
                else if (dirToNearestCollider == Direction.East)
                {
                    transform.localPosition = new Vector3(nearestCube.GetComponent<Cube>().length, 0, 0);
                } 
            }
            dirToNearestCollider = null;
            hitColliders = null;
            nearestCube = null;
        }
    }
}
