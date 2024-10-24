using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraficoContoller : MonoBehaviour
{
    public Graficonodirigido nodo; 
    public float speed;
    public int energy;

    private Rigidbody2D rb;
    private Vector2 dir;
    public bool isdescansar;
    private float tiempo;

    [Header("Field Vision")]
    public Transform target;
    public float view_radius = 5f;
    [Range(45, 90)] public float view_angle = 75;
    public float rotation_speed = 200f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        dir = nodo.transform.position - transform.position;

        if (nodo.conexiones.Count > 0) 
        {
            energy -= nodo.GetCostToNextNode(); 
        }
    }

    private void Update()
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(Vector3.forward * angle);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotation_speed * Time.deltaTime);

        if (IsTargetInsideFOV())
        {
            dir = target.position - transform.position;
            return;
        }
        else
        {
            dir = nodo.transform.position - transform.position; 
        }

        if (isdescansar)
        {
            tiempo += Time.deltaTime;
            if (tiempo >= 1)
            {
                energy++;
                tiempo = 0;
                if (energy == 10)
                {
                    isdescansar = false;
                    CheckNodo();
                }
            }
            return;
        }

        if (Vector2.Distance(nodo.transform.position, transform.position) < 0.1f)
        {
            CheckNodo(); 
        }
    }

    private void CheckNodo()
    {
        Graficonodirigido node_tmp = nodo.GetNodeRandom();
        if (node_tmp != null) 
        {
            int cost = nodo.GetCostToNode(node_tmp); 

            if (energy >= cost)
            {
                nodo = node_tmp;  
                energy -= cost;    
                dir = nodo.transform.position - transform.position;
            }
            else
            {
                isdescansar = true;
            }
        }
    }

    private void FixedUpdate()
    {
        if (!isdescansar)
        {
            rb.velocity = dir.normalized * speed;  
        }
        else
        {
            rb.velocity = Vector2.zero; 
        }
    }

    private bool IsTargetInsideFOV()
    {
        if (target == null) return false;

        Vector2 directionToTarget = (target.position - transform.position).normalized;
        float angleToTarget = Vector2.Angle(transform.right, directionToTarget);

        if (angleToTarget < view_angle / 2)
        {
            float distance = Vector2.Distance(target.position, transform.position);
            return distance < view_radius;
        }
        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, view_radius); 

        Gizmos.color = IsTargetInsideFOV() ? Color.green : Color.red;
        Vector3 viewAngleA = DirFromAngle(-view_angle / 2);
        Vector3 viewAngleB = DirFromAngle(view_angle / 2);

        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * view_radius);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * view_radius);
    }

    private Vector3 DirFromAngle(float angle)
    {
        angle += transform.eulerAngles.z;
        return new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0);
    }
}
