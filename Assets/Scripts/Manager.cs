using UnityEngine;
using System.Collections;

public class Manager : MonoBehaviour {

    public GameObject boidPrefab;

    public int numberOfBoids = 10;
    public float spawnRange = 20;

    public bool displayBoidProperties;
    [Header("Boids Properties")]
    public float cohesionRadius = 10;
    public float separationDistance = 5;
    public float maxSpeed = 15;

    // Use this for initialization
    void Start ()
    {
        for (int i = 0; i < numberOfBoids; i++)
        {
            GameObject boidObj = Instantiate(boidPrefab, Random.insideUnitSphere * spawnRange, Quaternion.identity) as GameObject;
            Boid boid = boidObj.GetComponent<Boid>();
            boid.cohesionRadius = cohesionRadius;
            boid.separationDistance = separationDistance;
            boid.maxSpeed = maxSpeed;
            boid.maxDistance = spawnRange;
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}



    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, spawnRange);

        if (displayBoidProperties)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, cohesionRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, separationDistance);

            Gizmos.color = new Color(0, 1, 0, 0.2f);
            Gizmos.DrawMesh(boidPrefab.GetComponent<MeshFilter>().sharedMesh, transform.position, boidPrefab.transform.rotation, boidPrefab.transform.localScale);

        }
    }

}
