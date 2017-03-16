using UnityEngine;
using System.Collections;

[System.Serializable]
public class Monster
{

    //Propriétées

    public const int level = 80;
    public const int NombreDeProps = 8; // Ne pas changer

    public bool animating = false;

    #region Properties
    public float walkSpeed { get { return stats[0]; } set { stats[0] = value; } } // walk distance per tick
    public float attackSpeed { get { return stats[1]; } set { stats[1] = value; } } // number of attacks per tick
    public float attackRange { get { return stats[2]; } set { stats[2] = value; } } // attack range
    public float attackDamages { get { return stats[3]; } set { stats[3] = value; } }  //x1 mult - attack damage
    public float cowardise { get { return stats[4]; } set { stats[4] = value; } }  // if life is below this, the monster flee
    public float lifeRegen { get { return stats[5]; } set { stats[5] = value; } }  // life regen per tick
    public float maxLife { get { return stats[6]; } set { stats[6] = value; } }  // x100 mult - maximum life
    public float lifeSpawn { get { return stats[7]; } set { stats[7] = value; } }  // Life spawn in tick
    /*
    public const float walkSpeedMultiplier = 1;
    public const float attackSpeededMultiplier = 1;
    public const float attackRangeedMultiplier = 1;
    public const float attackDamagesedMultiplier = 1;
    public const float aggresivityedMultiplier = 50;
    public const float lifeRegenedMultiplier = 1;
    public const float maxLifeedMultiplier = 100;
    public const float lifeSpawneMultiplier = 5;
    */
    public float[] stats = new float[NombreDeProps] {0, 0, 0, 0, 0, 0, 0, 0};
    [HideInInspector] public float[] statsMultipliers = new float[NombreDeProps] { 1, 1, 1, 1, 30, 1, 100, 5 };
    #endregion

    [Header("Stats")]
    public Monster opponent;
    public float currentLife = 1000;
    public float currentLifeSpawn = 10;
    public Vector2 position;
    public bool Dead = false;


#region Constructors

    public Monster()
    {
        GivePoints();
    }

    public Monster(Vector2 pos)
    {
        GivePoints();
        position = pos;
    }

    public Monster(Vector2 pos, Monster opp)
    {
        GivePoints();
        position = pos;
        opponent = opp;
    }

    public Monster(Monster clone)
    {
        walkSpeed = clone.walkSpeed;
        attackSpeed = clone.attackSpeed;
        attackRange = clone.attackRange;
        attackDamages = clone.attackDamages;
        cowardise = clone.cowardise;
        lifeRegen = clone.lifeRegen;
        maxLife = clone.maxLife;
        lifeSpawn = clone.lifeSpawn;

        currentLife = maxLife;
        currentLifeSpawn = lifeSpawn;
    } 
#endregion

    void GivePoints()
    {
        int points = level;

        while(points > 0)
        {
            int rand = Random.Range(0, NombreDeProps);
            stats[rand] += statsMultipliers[rand];
            points--;
        }
        currentLife = maxLife;
        currentLifeSpawn = lifeSpawn;
    }

    public void Itteration()
    {
        if(opponent == null || Dead || opponent.Dead)
        {
            return;
        }

        currentLifeSpawn -= 1;
        if(currentLifeSpawn <= 0)
        {
            Dead = true;
            return;
        }

        ModifyLife(lifeRegen);

        if(currentLife < cowardise)
        {
            Flee();
            return;
        }

        if (isOpponentNearby())
        {
            Attack();
        }
        else
        {
            MoveToOpponent();
        }
    }

    public IEnumerator IterationAnimation(Transform transformDisplay, Vector3 startPos, Vector3 endPos)
    {
        while (animating)
        {
            yield return null;
        }
        animating = true;
        //2s d'amimation
        float OriginDeltaTime = Time.deltaTime;
        float animTime = 2 / OriginDeltaTime;
        float attackInterval = animTime/attackSpeed;
        Debug.Log(animTime);
        for (float i = 0; i < animTime; i+= 1)
        {
            yield return null;

            transformDisplay.position = Vector3.Lerp(transformDisplay.position, endPos, OriginDeltaTime + 0.1f);

            if(i%attackInterval < 1 && isOpponentNearby())
            {
                Bullet.Shoot(transformDisplay, new Vector3(opponent.position.x, 0, opponent.position.y), attackSpeed * 3);
            }

        }

        transformDisplay.position = endPos;
        animating = false;
    }


    bool isOpponentNearby()
    {
        float distSquare = (position.x - opponent.position.x) * (position.x - opponent.position.x) + (position.y - opponent.position.y) * (position.y - opponent.position.y);
        return distSquare <= (attackRange * attackRange); 
    }

    void MoveToOpponent()
    {
        Vector2 moveDir = opponent.position - position;
        position += moveDir.normalized * walkSpeed;
    }

    void Attack()
    {
        for (int i = 0; i < attackSpeed; i++)
        {
            opponent.ModifyLife(-attackDamages);
        }
    }

    public void ModifyLife(float val)
    {
        currentLife += val;
        if (currentLife > maxLife) currentLife = maxLife;
        if (currentLife <= 0) Dead = true;
    }

    void Flee()
    {
        Vector2 fleeDir = position - opponent.position;
        if (fleeDir.magnitude == 0) fleeDir = Random.insideUnitCircle;
        position += fleeDir.normalized * walkSpeed;
    }

    public void SetOpponent(Monster opp)
    {
        opponent = opp;
    }

    public void Normalize()
    {
        float sum = 0;
        for (int i = 0; i < stats.Length; i++)
        {
            sum += stats[i] / statsMultipliers[i];
        }

        float coef = level / sum;
        string deb = "";
        for (int i = 0; i < stats.Length; i++)
        {
            stats[i] *= coef;
            deb += stats[i] + ", ";
        }
    }

    public void Heal()
    {
        currentLife = maxLife;
        currentLifeSpawn = lifeSpawn;
    }

}
