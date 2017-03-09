using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class BoidsManager : MonoBehaviour {

    public GameObject prefabBoid;

    public bool haveLeaders = false;
    private bool prevStateLeader = false;

    private List<int> leaders;

    public int nbLeaders = 5;

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

    bool isLeader = false;

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
        leaders = new List<int>();
        if(haveLeaders)
        {
            SelectLeaders();
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
        if(prevStateLeader != haveLeaders)
        {
            prevStateLeader = haveLeaders;
            if(haveLeaders)
            {
                SelectLeaders();
            }
            else
            {
                NoLeaders();
            }
        }

        if(Input.GetKeyDown(KeyCode.T))
        {
            ToggleTrails();
        }

        Move_All_Boids();
	}

    void ToggleTrails()
    {
        for(int i = 0; i < tabBoids.Length; i++)
        {
            tabBoids[i].GetComponent<TrailRenderer>().enabled = !tabBoids[i].GetComponent<TrailRenderer>().enabled;
        }
    }

    void Move_All_Boids()
    {
        sumAll = SumAll();
        Vector3 v1, v2, v3, v4, vel;

        for(int i = 0; i < tabBoids.Length; i++)
        {
            vel = tabBoids[i].transform.forward;
            v3 = Vector3.zero;

            v1 = DirToCenter(i);
            v2 = AvoidOthers(i, keepDistance);
            if(haveLeaders)
            {
                isLeader = false;
                for(int j = 0; j < leaders.Count; j++)
                {
                    if(i == leaders[j])
                    {
                        isLeader = true;
                        break;
                    }
                }
                if(!isLeader)
                {
                    v3 = FollowLeaders(i);
                }
            }
            else
            {
                v3 = GroupUp(i);
            }
            v4 = ClampArea(i);

            vel = vel + v1 + v2 + v3 + v4;
            vel = vel.normalized;

            Vector3 dir = Vector3.Lerp(tabBoids[i].transform.position, tabBoids[i].transform.position + vel * speed, lerpSpeed);
            tabBoids[i].transform.LookAt(dir);
            tabBoids[i].transform.position  = dir; 

            //DEBUG
            /*
            if(i == 50)
            {
                Debug.Log("v1 = " + v1 + " ||  v2 = " + v2 + " ||  v3 = " + v3 + " ||  v4 = " + v4);
            }
            */
        }
    }

    private void SelectLeaders()
    {
        string str = "New Leaders are : ";
        if (leaders.Count > 0)
        {
            for (int i = 0; i < leaders.Count; i++)
            {
                tabBoids[leaders[i]].GetComponent<MeshRenderer>().material.color = new Color(1f, 1f, 1f);
                tabBoids[leaders[i]].GetComponent<TrailRenderer>().material.SetColor("_Color", new Color(1f, 1f, 1f));
            }
            leaders.Clear();
        }
        for(int i = 0; i < nbLeaders; i++)
        {
            int rd = Random.Range(0, tabBoids.Length);
            leaders.Add(rd);
            str += rd + "  ";
            tabBoids[rd].GetComponent<MeshRenderer>().material.color = new Color(1f, 0f, 0f);
            tabBoids[leaders[i]].GetComponent<TrailRenderer>().material.SetColor("_Color", new Color(1f, 0f, 0f));
        }
        Debug.Log(str);

    }

    private void NoLeaders()
    {
        if (leaders.Count > 0)
        {
            for (int i = 0; i < leaders.Count; i++)
            {
                tabBoids[leaders[i]].GetComponent<MeshRenderer>().material.color = new Color(1f, 1f, 1f);
                tabBoids[leaders[i]].GetComponent<TrailRenderer>().material.SetColor("_Color", new Color(1f, 1f, 1f));
            }
            leaders.Clear();
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

        Collider[] around = Physics.OverlapSphere(posMe, distance);
        for(int i = 0; i < around.Length; i++)
        {
            posOther = around[i].transform.position;
            dir += posMe - posOther;
        }

        return dir.normalized / avoidInfluence;
    }

    float SquaredDistance(Vector3 a, Vector3 b)
    {
        return ((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y) + (a.z - b.z) * (a.z - b.z));
    }

    Vector3 GroupUp(int index)
    {
        Vector3 dir = Vector3.zero;
        Vector3 posMe = tabBoids[index].transform.position;
        int nb = 0;

        Collider[] around = Physics.OverlapSphere(posMe, otherRange);
        for (int i = 0; i < around.Length; i++)
        {
            dir += around[i].transform.forward;
            nb++;
        }
        if(nb > 0)
        {
            dir /= nb;
        }
        /*
        for (int i = 0; i < tabBoids.Length; i++)
        {
           
            if (i != index)
            {
                if(SquaredDistance(tabBoids[index].transform.position, tabBoids[i].transform.position) < (otherRange* otherRange))
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
        */
        return dir / otherInfluence;
    }

    Vector3 FollowLeaders(int index)
    {
        Vector3 dir;
        int closest = 0;
        Vector3 selfpos = tabBoids[index].transform.position;
        float dist = 10000000f;
        for(int i = 0; i < leaders.Count; i++)
        {
            float ndist = SquaredDistance(selfpos, tabBoids[leaders[i]].transform.position);
            if (ndist < dist)
            {
                dist = ndist;
                closest = leaders[i];
            }
        }
        dir = (tabBoids[closest].transform.position - selfpos);
        dir += tabBoids[closest].transform.forward;
        dir.Normalize();
        
        //Debug.Log(" Closest leader =  " + closest);
        return dir / (otherInfluence*otherInfluence);
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
