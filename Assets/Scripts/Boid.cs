using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Boid : MonoBehaviour
{

    public Vector3 velocity;


    [HideInInspector] public float cohesionRadius = 10;
    [HideInInspector] public float separationDistance = 5;
    [HideInInspector] public float maxSpeed = 15;
    [HideInInspector] public float maxDistance = 40;

    private Collider[] allBoids;
    private Vector3 cohesion;
    private Vector3 separation;
    private int separationCount;
    private Vector3 alignment;

    Collider collid;

    // Use this for initialization
    void Awake ()
    {
        collid = GetComponent<Collider>();


	}
	
	// Update is called once per frame
	void Update ()
    {
        CalculateVelocity();

        if (transform.position.magnitude > maxDistance)
        {
            velocity += -transform.position.normalized;
        }

        transform.position += velocity * Time.deltaTime;
        transform.LookAt(transform.position + velocity);

        //Debug.DrawRay(transform.position, separation, Color.green);
        //Debug.DrawRay(transform.position, cohesion, Color.magenta);
        //Debug.DrawRay(transform.position, alignment, Color.blue);
    }

    void CalculateVelocity()
    {
        velocity = Vector3.zero;
        cohesion = Vector3.zero;
        separation = Vector3.zero;
        separationCount = 0;
        alignment = Vector3.zero;

        allBoids = Physics.OverlapSphere(transform.position, cohesionRadius);

        foreach (var boid in allBoids)
        {
            cohesion += boid.transform.position;
            alignment += boid.GetComponent<Boid>().velocity;

            if (boid != collid && (transform.position - boid.transform.position).magnitude < separationDistance)
            {
                separation += (transform.position - boid.transform.position) / (transform.position - boid.transform.position).magnitude;
                separationCount++;
            }
        }

        cohesion = cohesion / allBoids.Length;
        cohesion = cohesion - transform.position;
        cohesion = Vector3.ClampMagnitude(cohesion, maxSpeed);
        if (separationCount > 0)
        {
            separation = separation / separationCount;
            separation = Vector3.ClampMagnitude(separation, maxSpeed);
        }
        alignment = alignment / allBoids.Length;
        alignment = Vector3.ClampMagnitude(alignment, maxSpeed);

        velocity += cohesion + separation * 10 + alignment * 1.5f;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
    }


    void OnDrawGizmosSelected()
    {

        Gizmos.DrawWireSphere(transform.position, cohesionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, separationDistance);

    }

}
