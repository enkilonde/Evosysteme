using UnityEngine;
using System.Collections;

public class BoidsManager : MonoBehaviour {

    public GameObject prefabBoid;

    public int nbBoids = 20;

    public float spawnCube = 10.0f;

    public float speed = 0.1f;

    public float keepDistance = 5.0f;

    public float avoidInfluence = 10f;

    public float centerInfluence = 100;

    public float sphereClamp = 20.0f;
    public float clampInfluence = 10.0f;

    public float otherRange = 5.0f;
    public float otherInfluence = 10;

    public float lerpSpeed = 0.5f;

    private GameObject[] tabBoids;

    private Vector3 sumAll;

	// Use this for initialization
	void Start ()
    {
        tabBoids = new GameObject[nbBoids];
        Vector3 pos;
        float range = spawnCube * 0.5f;

        for (int i = 0; i < nbBoids; i++)
        {
            pos = new Vector3(Random.Range(-range, range), Random.Range(-range, range), Random.Range(-range, range));
            GameObject boid = Instantiate(prefabBoid, pos, Random.rotation) as GameObject;
            boid.transform.parent = transform;
            tabBoids[i] = boid;
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
        Move_All_Boids();
	}


    void Move_All_Boids()
    {
        sumAll = SumAll();
        Vector3 v1, v2, v3, v4, vel;

        for(int i = 0; i < tabBoids.Length; i++)
        {
            vel = tabBoids[i].transform.forward;

            v1 = DirToCenter(i);
            v2 = AvoidOthers(i, keepDistance);
            v3 = GroupUp(i);

            v4 = ClampArea(i);

            vel = vel + v1 + v2 + v3 + v4;
            vel = vel.normalized;

            Vector3 dir = Vector3.Lerp(tabBoids[i].transform.position, tabBoids[i].transform.position + vel * speed, lerpSpeed);
            tabBoids[i].transform.LookAt(dir);
            tabBoids[i].transform.position  = dir; 

            //DEBUG
            /*
            if(i == 0)
            {
                Debug.Log("v1 = " + v1 + " ||  v2 = " + v2 + " ||  v3 = " + v3);
            }
            */
        }
    }

    private Vector3 SumAll()
    {
        Vector3 ret = Vector3.zero;
        for (int i = 0; i < tabBoids.Length; i++)
        {
            ret += tabBoids[i].transform.position;
        }

        return ret;
    }

    Vector3 DirToCenter(int index)
    {
        Vector3 center = sumAll;

        center -= tabBoids[index].transform.position;
        center /= tabBoids.Length - 1;

        Vector3 dir = center - tabBoids[index].transform.position;

        return dir / centerInfluence;
    } 

    Vector3 AvoidOthers(int index, float distance)
    {
        Vector3 dir = Vector3.zero;
        Vector3 posMe, posOther;
        posMe = tabBoids[index].transform.position;

        for (int i = 0; i < tabBoids.Length; i++)
        {
            posOther = tabBoids[i].transform.position;

            if (Vector3.Distance(posMe, posOther) < distance)
            {
                dir += posMe - posOther;
            }

        }
        return dir.normalized / avoidInfluence;
    }


    Vector3 GroupUp(int index)
    {
        Vector3 dir = Vector3.zero;
        int nb = 0;
        for (int i = 0; i < tabBoids.Length; i++)
        {
           
            if (i != index)
            {
                if(Vector3.Distance(tabBoids[index].transform.position, tabBoids[i].transform.position) < otherRange)
                {
                    dir += tabBoids[i].transform.forward;
                    nb++;
                }
            }
            if(nb > 0)
            {
                dir /= nb;
            }
            //dir /= (tabBoids.Length - 1);
        }

        return dir / otherInfluence;
    }

    Vector3 ClampArea( int index)
    {
        Vector3 dir = Vector3.zero;
        if(Vector3.Distance(Vector3.zero, tabBoids[index].transform.position) > sphereClamp)
        {
            dir = tabBoids[index].transform.position.normalized * -1f;
        }
        return dir / clampInfluence;
    }
}
