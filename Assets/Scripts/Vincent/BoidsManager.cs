using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class BoidsManager : MonoBehaviour {

    public GameObject prefabBoid;

    private InputField fieldBoids;
    private InputField fieldLeaders;
    private Toggle toggleFollow;

    private MeshFilter myMesh;

    public Mesh[] Meshes;

    bool mesh = true;
    bool cone = false;

    public bool haveLeaders = false;
    private bool prevStateLeader = false;

    private List<int> leaders;

    public int nbLeaders = 5;

    public int nbBoids = 20;

    public float speed = 0.1f;

    public float keepDistance = 5.0f;

    public float avoidInfluence = 10f;

    public float centerInfluence = 100;

    public float radiusClamp = 20.0f;
    public float heightClamp = 10.0f;
    public float clampInfluence = 10.0f;

    public float otherRange = 5.0f;
    public float otherInfluence = 10;

    public float lerpSpeed = 0.5f;

    private GameObject[] tabBoids;

    private Vector3 sumAll;

    GameObject BoidsContainer;

    bool isLeader = false;

	// Use this for initialization
	void Start ()
    {
        myMesh = GetComponent<MeshFilter>();
        myMesh.mesh = Meshes[0];

        BoidsContainer = new GameObject("BOIDS");
        BoidsContainer.transform.position = transform.position;

        fieldBoids = GameObject.Find("InputBoids").GetComponent<InputField>();
        fieldLeaders = GameObject.Find("InputLeaders").GetComponent<InputField>();
        toggleFollow = GameObject.Find("Leader").GetComponent<Toggle>();


        CreateBoids();    
	}

    public void CreateBoids()
    {
        tabBoids = new GameObject[nbBoids];

        float rangev = heightClamp;
        float rangeh = radiusClamp;

        for (int i = 0; i < nbBoids; i++)
        {
            Vector3 pos;
            pos = new Vector3(Random.Range(-rangeh, rangeh), Random.Range(-rangev, rangev), Random.Range(-rangeh, rangeh));
            GameObject boid = Instantiate(prefabBoid, BoidsContainer.transform.position + pos, Random.rotation) as GameObject;
            boid.name = i.ToString();
            Transform child;
            if(boid.transform.GetChild(0) != null)
            {
                child = boid.transform.GetChild(0);
                child.GetComponent<Animation>()["CrowFlap"].time = Random.Range(0f, 1f);
            }
            boid.transform.parent = BoidsContainer.transform;
            tabBoids[i] = boid;
        }
        leaders = new List<int>();
        if (haveLeaders)
        {
            SelectLeaders();
        }
    }
    
    public void BoidValueChange()
    {
        if ( fieldBoids.text.Length > 0)
        {
            nbBoids = int.Parse(fieldBoids.text);
        }
    }
    public void LeaderValueChange()
    {
        if (fieldLeaders.text.Length > 0)
        {
            nbLeaders = int.Parse(fieldLeaders.text);
        }
    }

    public void FollowToggle()
    {
        haveLeaders = toggleFollow.isOn;
    }
    
    public void ApplyReset()
    {
        for(int i = 0; i<tabBoids.Length; i++)
        {
            Destroy(tabBoids[i]);
        }
        CreateBoids();

    }

	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            heightClamp = 75;
            radiusClamp = 30;
            myMesh.mesh = Meshes[0];
            cone = false;
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            heightClamp = 25;
            radiusClamp = 150;
            myMesh.mesh = Meshes[0];
            cone = false;
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            heightClamp = 75;
            radiusClamp = 30;
            myMesh.mesh = Meshes[1];
            cone = true;
        }
        if(Input.GetKeyDown(KeyCode.B))
        {
            mesh = !mesh;
            GetComponent<MeshRenderer>().enabled = mesh;
        }


        transform.localScale = new Vector3(radiusClamp * 2, radiusClamp * 2, heightClamp * 2);
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

            if(cone)
            {
                v4 = ClampCone(i);
            }
            else
            {
                v4 = ClampArea(i);
            }
            

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
                tabBoids[leaders[i]].GetComponent<TrailRenderer>().material.SetColor("_EmisColor", new Color(0f, 0f, 0f));
            }
            leaders.Clear();
        }
        for(int i = 0; i < nbLeaders; i++)
        {
            int rd = Random.Range(0, tabBoids.Length);
            leaders.Add(rd);
            str += rd + "  ";
            tabBoids[rd].GetComponent<MeshRenderer>().material.color = new Color(1f, 0f, 0f);
            tabBoids[leaders[i]].GetComponent<TrailRenderer>().material.SetColor("_EmisColor", new Color(1f, 0f, 0f));
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
                tabBoids[leaders[i]].GetComponent<TrailRenderer>().material.SetColor("_EmisColor", new Color(0f, 0f, 0f));
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
        Vector3 posMe = tabBoids[index].transform.position;
        //méthode en parcours total
        /*
        int nb = 0;

        for(int i = 0; i < tabBoids.Length; i++)
        {
            if(SquaredDistance(posMe, tabBoids[i].transform.position) < distance*distance)
            {
                dir += (posMe - tabBoids[i].transform.position).normalized;
                nb++;
            }
        }

        if(nb>0)
        {
            dir /= nb;
        }
        */
        // méthode en OverlapSphere
        Collider[] around = Physics.OverlapSphere(posMe, distance);
        int l = around.Length;
        for (int i = 0; i < l; i++)
        {
            dir += (posMe - around[i].transform.position).normalized;
        }
        if (l > 0)
        {
            dir /= l;
        }
        
        return dir / avoidInfluence;
    }

    float SquaredDistance(Vector3 a, Vector3 b)
    {
        return ((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y) + (a.z - b.z) * (a.z - b.z));
    }

    Vector3 GroupUp(int index)
    {
        Vector3 dir = Vector3.zero;
        Vector3 posMe = tabBoids[index].transform.position;

        Collider[] around = Physics.OverlapSphere(posMe, otherRange);
        int l = around.Length;
        for (int i = 0; i < l; i++)
        {
            dir += around[i].transform.forward;
        }
        if(l > 0)
        {
            dir /= l;
        }
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
        
        return dir / (otherInfluence*otherInfluence);
    }

    Vector3 ClampArea( int index)
    {
        Vector3 dir = Vector3.zero;
        Vector3 boid = tabBoids[index].transform.position;
        if (boid.y > transform.position.y + heightClamp)
        {
            dir += Vector3.up * -1;
        }
        else
        {
            if (boid.y < transform.position.y - heightClamp)
            {
                dir += Vector3.up;
            }
        }
        Vector3 posXZ = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 boidXZ = new Vector3(tabBoids[index].transform.position.x, 0, tabBoids[index].transform.position.z);
        if (SquaredDistance(posXZ, boidXZ) > radiusClamp* radiusClamp)
        {
            // dir += tabBoids[index].transform.position.normalized * -1f;

            dir += (posXZ - boidXZ).normalized;
        }
        return dir / clampInfluence;
    }

    Vector3 ClampCone(int index)
    {
        Vector3 dir = Vector3.zero;
        Vector3 boid = tabBoids[index].transform.position;
        if (boid.y > transform.position.y + heightClamp)
        {
            dir += Vector3.up * -1;
        }
        else
        {
            if (boid.y < transform.position.y - heightClamp)
            {
                dir += Vector3.up;
            }
        }
        Vector3 posXZ = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 boidXZ = new Vector3(tabBoids[index].transform.position.x, 0, tabBoids[index].transform.position.z);
        float heightPercent = 0.5f + (tabBoids[index].transform.position.y - transform.position.y) / (heightClamp*2);
        if (SquaredDistance(posXZ, boidXZ) > radiusClamp * radiusClamp * heightPercent)
        {
            // dir += tabBoids[index].transform.position.normalized * -1f;

            dir += (posXZ - boidXZ).normalized;
        }
        return dir / clampInfluence;
    }

}
