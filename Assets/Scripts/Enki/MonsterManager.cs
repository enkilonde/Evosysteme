using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonsterManager : MonoBehaviour
{
    public int indexToDisplay1 = 0;
    public int indexToDisplay2 = 1;


    public List<Monster> winners = new List<Monster>();

    public List<Text> Displays = new List<Text>();
    public List<InputField> Selectors = new List<InputField>();

    GameObject[] Cubes = new GameObject[2];

    Monster M1;
    Monster M2;

    // Use this for initialization
    void Awake ()
    {

        winners = GenerateNewMonsters(500);
        Application.targetFrameRate = 60;
        Selectors[0].text = 0.ToString();
        Selectors[1].text = 1.ToString();


    }

    // Update is called once per frame
    void Update ()
    {
        indexToDisplay1 = int.Parse(Selectors[0].text);
        indexToDisplay2 = int.Parse(Selectors[1].text);
        int m1 = indexToDisplay1;
        int m2 = indexToDisplay2;
        M1 = winners[(int)Mathf.Clamp(m1, 0, winners.Count)];
        M2 = winners[(int)Mathf.Clamp(m2, 0, winners.Count)];
        DisplayMonsterStats(Displays[0], M1);
        DisplayMonsterStats(Displays[1], M2);


        if (Input.GetKeyDown(KeyCode.M))
        {


        }


        if (Input.GetKeyDown(KeyCode.F))
        {
            winners = OneRound(winners);
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            OneGeneration();
        }

    }

    public  void AnimatedFight()
    {
        if (M1.animating || M2.animating) return;
        OneIteration(M1, M2);
        MoveCube(0, M1.position);
        MoveCube(1, M2.position);
    }

    List<Monster> GenerateNewMonsters(int number)
    {
        List<Monster> result = new List<Monster>();
        for (int i = 0; i < number; i++)
        {
            result.Add(new Monster());
        }

        return result;
    }

    void OneGeneration()
    {

        while(winners.Count > 1)
        {
            winners = OneRound(winners);
            if (winners.Count == 1) break;
            winners = MutateMonster(winners);
            winners = CroisementMonsters(winners);
        }

    }

    List<Monster> OneRound(List<Monster> candidates)
    {
        List<Monster> result = new List<Monster>();
        
        if(candidates.Count%2 != 0)
        {
            result.Add(candidates[candidates.Count-1]);
            candidates.RemoveAt(candidates.Count - 1);
        }
        
        
        for (int i = 0; i < candidates.Count / 2; i++)
        {
            PrepareCandidates(candidates[i * 2], candidates[i * 2 + 1], true);

            result.Add(FightTwoMonsters(candidates[i * 2], candidates[i * 2 + 1]));
            result[result.Count - 1].Heal();
        }

        return result;
    }

    Monster FightTwoMonsters(Monster M1, Monster M2)
    {
        while (true)
        {
            OneIteration(M1, M2);
            
            if (M1.Dead) return M2;
            if (M2.Dead) return M1;
        }
    }

    void OneIteration(Monster M1, Monster M2)
    {
        //Debug.Log();

        if(M1.opponent != M2 && M2.opponent != M1)
        {
            PrepareCandidates(M1, M2);
        }

        M1.Itteration();
        M2.Itteration();
        //Debug.Log(M1.currentLifeSpawn + "    " + winners[0].currentLifeSpawn);
    }

    void PrepareCandidates(Monster M1, Monster M2, bool heal = false)
    {
        M1.position = new Vector2(-10, -10);
        M2.position = new Vector2(10, 10);

        M1.SetOpponent(M2);
        M2.SetOpponent(M1);

        if (heal)
        {
            M1.Heal();
            M2.Heal(); 
        }
    }

    Monster MutateMonster(Monster M1)
    {

        Monster M2 = new Monster(M1);

        float moyenne = Monster.level / Monster.NombreDeProps;
        float newVal = Random.Range((int)moyenne - Monster.NombreDeProps, (int)moyenne + Monster.NombreDeProps);
        int rand = Random.Range(0, Monster.NombreDeProps);
        float ecart = newVal - M2.stats[rand];

        M2.stats[rand] = newVal * M2.statsMultipliers[rand];
        M2.Normalize();
        return M2;
    }

    List<Monster> MutateMonster(List<Monster> monsters)
    {
        for (int i = 0; i < monsters.Count; i++)
        {
            monsters[i] = MutateMonster(monsters[i]);
        }
        return monsters;
    }

    List<Monster> CroisementMonsters(Monster M1, Monster M2)
    {
        List<Monster> result = new List<Monster>() { new Monster(M1), new Monster(M2) };

        int rand = Random.Range(0, Monster.NombreDeProps);

        float[] stats1 = new float[Monster.NombreDeProps];
        float[] stats2 = new float[Monster.NombreDeProps];

        for (int i = 0; i < Monster.NombreDeProps; i++)
        {
            if(i < rand)
            {
                stats1[i] = result[0].stats[i];
                stats2[i] = result[1].stats[i];
            }
            else
            {
                stats1[i] = result[1].stats[i];
                stats2[i] = result[0].stats[i];
            }
        }

        result[0].stats = stats1;
        result[1].stats = stats2;

        return result;
    }

    List<Monster> CroisementMonsters(List<Monster> monsters)
    {

        for (int i = 0; i < monsters.Count/2; i++)
        {
            List<Monster> M = CroisementMonsters(monsters[i * 2], monsters[i * 2 + 1]);
            monsters[i * 2] = M[0];
            monsters[i * 2 + 1] = M[1];
        }

        return monsters;
    }

    void DisplayMonsterStats(Text targetText, Monster M1)
    {
        

        targetText.text = "Life : " + M1.currentLife + " / " + M1.maxLife +
            "\nLifeSpawn : " + M1.currentLifeSpawn + " / " + M1.lifeSpawn +
            "\nWalk Speed : " + M1.walkSpeed +
            "\nAttack Speed: " + M1.attackSpeed +
            "\nAttack Damages : " + M1.attackDamages +
            "\nAttack Range : " + M1.attackRange +
            "\nCowardise : " + M1.cowardise +
            "\nLife Regen : "+ M1.lifeRegen;

    }

    void MoveCube(int index/*0 ou 1*/, Vector2 pos)
    {

        if(Cubes[index] == null)
        {
            Cubes[index] = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Cubes[index].name = "Monster " + index.ToString();
            if(index == 0)
            {
                Cubes[index].GetComponent<Renderer>().material.color = Color.red;
            }
            else
            {
                Cubes[index].GetComponent<Renderer>().material.color = Color.blue;
            }
            Cubes[index].transform.position = new Vector3(pos.x, 0, pos.y);
        }
        Monster M = index == 0 ? winners[indexToDisplay1] : winners[indexToDisplay2];

        StartCoroutine(M.IterationAnimation(Cubes[index].transform, Cubes[index].transform.position, new Vector3(pos.x, 0, pos.y)));


        //Cubes[index].transform.position = new Vector3(pos.x, 0, pos.y);
    }

    void CleanCubes()
    {
        Destroy(Cubes[0]);
        Destroy(Cubes[1]);

    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {

        if (UnityEditor.Selection.activeGameObject != null)
        {
            GameObject selected = UnityEditor.Selection.activeGameObject;
            if (!selected.name.Contains("Monster")) return;
            Monster M = selected.name == "Monster 0" ? winners[indexToDisplay1] : winners[indexToDisplay2];

            if (selected.name.Contains("Monster"))
            {
                Gizmos.color = new Color(1, 0, 0, 0.2f);
                Gizmos.DrawSphere(selected.transform.position, M.attackRange);

                Vector3 opponentPos = new Vector3(M.opponent.position.x, 0, M.opponent.position.y);
                //Gizmos.DrawCube(selected.transform.position + (opponentPos - selected.transform.position).normalized * M.walkSpeed, new Vector3(1, 0, 0));
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(selected.transform.position, M.walkSpeed);
            }

        }
    }

#endif

}
