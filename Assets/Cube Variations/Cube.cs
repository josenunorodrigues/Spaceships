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
    public KeyCode rotateCubeLeftKey = KeyCode.Q;
    public KeyCode rotateCubeRightKey = KeyCode.E;
    public float speed = 1.5f;
    public int tier = 1;
    public int health = 20;
    private int id;
    private LineRenderer line;
    public int length = 1;
    public Heart heartParent;
    public float colliderRadius = 3f;
    public Cube cubeNorth, cubeEast, cubeSouth, cubeWest;
    public bool socketNorth = true, socketEast = true, socketSouth = true, socketWest = true;
    public Vector2 force;
    public float topSpeed = 10;
    public Direction? dirToNearestCollider;
    public enum Direction { North, East, South, West }
    Vector3 offsetValue;
    public Vector3 mousePreviousLocation;
    Collider2D[] hitColliders;
    public GameObject nearestCube = null;
    float placementOffset = 0.05f;
    // Start is called before the first frame update

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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, colliderRadius);
    }
    bool canAddBlock(float angle, Cube nearestCube)
    {
        if (angle < 315 && angle >= 225)
        {
            if (socketSouth && cubeSouth == null && nearestCube.cubeNorth == null && nearestCube.socketNorth)
            {
                dirToNearestCollider = Direction.North;
                return true;
            }
        }
        else if (angle < 225 && angle >= 135)
        {
            if (socketWest && cubeWest == null && nearestCube.cubeEast == null && nearestCube.socketEast)
            {
                dirToNearestCollider = Direction.East;
                return true;
            }
        }
        else if (angle < 135 && angle >= 45)
        {
            if (socketNorth && cubeNorth == null && nearestCube.cubeSouth == null && nearestCube.socketSouth)
            {
                dirToNearestCollider = Direction.South;
                return true;
            }
        }
        else if (angle < 45 && angle >= 0 || angle < 360 && angle >= 315)
        {
            if (socketEast && cubeEast == null && nearestCube.cubeWest == null && nearestCube.socketWest)
            {
                dirToNearestCollider = Direction.West;
                return true;
            }
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
            if (Input.GetKeyDown(rotateCubeLeftKey))
            {
                Quaternion targetRotation = transform.rotation;
                targetRotation *= Quaternion.AngleAxis(90, Vector3.forward);
                transform.rotation = targetRotation;
            }
            if (Input.GetKeyDown(rotateCubeRightKey))
            {
                Quaternion targetRotation = transform.rotation;
                targetRotation *= Quaternion.AngleAxis(-90, Vector3.forward);
                transform.rotation = targetRotation;
            }
            Vector3 mouseCurLocation = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
            force = mouseCurLocation - mousePreviousLocation;//Changes the force to be applied
            mousePreviousLocation = mouseCurLocation;
            bool canAdd = false;
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
                if (cycleNearestCube)
                {
                    bool addedDifferentCube = false;
                    if (nearestCube == null)
                    {
                        addedDifferentCube = true;
                        nearestCube = cycleNearestCube;
                        applyHeartRotation(nearestCube.GetComponent(typeof(Cube)) as Cube);
                    }
                    else if (nearestCube.name != cycleNearestCube.name)
                    {
                        addedDifferentCube = true;
                        nearestCube = cycleNearestCube;
                    }
                    if (addedDifferentCube)
                    {
                        Gradient gradient = new Gradient();
                        GradientColorKey[] colorKey = new GradientColorKey[2];
                        colorKey[0].color = GetComponent<Renderer>().material.GetColor("_Color");
                        colorKey[0].time = 0.0f;
                        colorKey[1].color = nearestCube.GetComponent<Renderer>().material.GetColor("_Color");
                        colorKey[1].time = 1.0f;
                        GradientAlphaKey[] alphaKey = new GradientAlphaKey[2];
                        alphaKey[0].alpha = 1.0f;
                        alphaKey[0].time = 0.0f;
                        alphaKey[1].alpha = 0.3f;
                        alphaKey[1].time = 1.0f;
                        gradient.SetKeys(colorKey, alphaKey);
                        line.colorGradient = gradient;
                    }
                }
                else nearestCube = null;
                if (nearestCube)
                {
                    Cube heartParent = getHeartParent(nearestCube.GetComponent(typeof(Cube)) as Cube);
                    if (heartParent)
                    {
                        Cube cube = nearestCube.GetComponent(typeof(Cube)) as Cube;
                        Vector3 dir = transform.position - nearestCube.transform.position;
                        dir = cube.heartParent.transform.InverseTransformDirection(dir);
                        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 180;
                        if (canAddBlock(angle, nearestCube.GetComponent(typeof(Cube)) as Cube))
                        {
                            canAdd = true;
                            line.enabled = true;
                            line.SetPosition(0, transform.position);
                            line.SetPosition(1, nearestCube.transform.position);
                        }
                    }
                }
            }
            if(!canAdd)
            {
                nearestCube = null;
                line.enabled = false;
            }
        }
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
                Cube cube = nearestCube.GetComponent(typeof(Cube)) as Cube;
                Vector3 dir = transform.position - nearestCube.transform.position;
                dir = cube.heartParent.transform.InverseTransformDirection(dir);
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 180;
                if (canAddBlock(angle, nearestCube.GetComponent(typeof(Cube)) as Cube))
                {
                    cube = nearestCube.GetComponent(typeof(Cube)) as Cube;
                    applyHeartRotation(cube);
                    heartParent = cube.heartParent;
                    transform.SetParent(nearestCube.transform);
                    GetComponent<Rigidbody2D>().isKinematic = true;

                    if (dirToNearestCollider == Direction.North)
                    {
                        transform.position = new Vector3(
                            nearestCube.transform.position.x + Mathf.Cos(heartParent.transform.eulerAngles.z * Mathf.Deg2Rad + Mathf.PI / 2) * (1 + placementOffset), 
                            nearestCube.transform.position.y + Mathf.Sin(heartParent.transform.eulerAngles.z * Mathf.Deg2Rad + Mathf.PI / 2) * (1 + placementOffset), 
                            0
                        );
                        nearestCube.GetComponent<Cube>().cubeNorth = this;
                        cubeSouth = nearestCube.GetComponent<Cube>();
                    }
                    if (dirToNearestCollider == Direction.East)
                    {
                        transform.position = new Vector3(
                            nearestCube.transform.position.x + Mathf.Cos(heartParent.transform.eulerAngles.z * Mathf.Deg2Rad) * (1 + placementOffset), 
                            nearestCube.transform.position.y + Mathf.Sin(heartParent.transform.eulerAngles.z * Mathf.Deg2Rad) * (1 + placementOffset), 
                            0
                        );
                        nearestCube.GetComponent<Cube>().cubeEast = this;
                        cubeWest = nearestCube.GetComponent<Cube>();
                    }
                    if (dirToNearestCollider == Direction.South)
                    {
                        transform.position = new Vector3(
                            nearestCube.transform.position.x - Mathf.Cos(heartParent.transform.eulerAngles.z * Mathf.Deg2Rad + Mathf.PI / 2) * (1 + placementOffset), 
                            nearestCube.transform.position.y - Mathf.Sin(heartParent.transform.eulerAngles.z * Mathf.Deg2Rad + Mathf.PI / 2) * (1 + placementOffset),
                            0
                        );
                        nearestCube.GetComponent<Cube>().cubeSouth = this;
                        cubeNorth = nearestCube.GetComponent<Cube>();
                    }
                    if (dirToNearestCollider == Direction.West)
                    {
                        transform.position = new Vector3(
                            nearestCube.transform.position.x - Mathf.Cos(heartParent.transform.eulerAngles.z * Mathf.Deg2Rad) * (1 + placementOffset), 
                            nearestCube.transform.position.y - Mathf.Sin(heartParent.transform.eulerAngles.z * Mathf.Deg2Rad) * (1 + placementOffset), 
                            0
                        );
                        nearestCube.GetComponent<Cube>().cubeWest = this;
                        cubeEast = nearestCube.GetComponent<Cube>();
                    }
                    updateNearCubes();
                }
            }

            dirToNearestCollider = null;
            hitColliders = null;
            nearestCube = null;
        }
    }
    private void updateNearCubes()
    {
        Vector3 northPosition = new Vector3(transform.position.x + Mathf.Cos(heartParent.transform.eulerAngles.z * Mathf.Deg2Rad + Mathf.PI / 2), transform.position.y + Mathf.Sin(heartParent.transform.eulerAngles.z * Mathf.Deg2Rad + Mathf.PI / 2), 0);
        Collider2D[] northColliders = Physics2D.OverlapCircleAll(northPosition, length/4, 1 << 8);
        Vector3 eastPosition = new Vector3(transform.position.x + Mathf.Cos(heartParent.transform.eulerAngles.z * Mathf.Deg2Rad), transform.position.y + Mathf.Sin(heartParent.transform.eulerAngles.z * Mathf.Deg2Rad), 0);
        Collider2D[] eastColliders = Physics2D.OverlapCircleAll(eastPosition, length / 4, 1 << 8);
        Vector3 southPosition = new Vector3(transform.position.x - Mathf.Cos(heartParent.transform.eulerAngles.z * Mathf.Deg2Rad + Mathf.PI / 2), transform.position.y - Mathf.Sin(heartParent.transform.eulerAngles.z * Mathf.Deg2Rad + Mathf.PI / 2), 0);
        Collider2D[] southColliders = Physics2D.OverlapCircleAll(southPosition, length / 4, 1 << 8);
        Vector3 westPosition = new Vector3(transform.position.x - Mathf.Cos(heartParent.transform.eulerAngles.z * Mathf.Deg2Rad), transform.position.y - Mathf.Sin(heartParent.transform.eulerAngles.z * Mathf.Deg2Rad), 0);
        Collider2D[] westColliders = Physics2D.OverlapCircleAll(westPosition, length / 4, 1 << 8);

    }
    private Cube getHeartParent(Cube nearestCube)
    {
        if (nearestCube.name == "Heart") return nearestCube;
        if (nearestCube.transform.parent) return getHeartParent(nearestCube.transform.parent.GetComponent<Cube>());
        return null;
    }
    private void applyHeartRotation(Cube nearestCube)
    {
        Vector3 eulerAngles = nearestCube.heartParent.transform.localEulerAngles;
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
}
