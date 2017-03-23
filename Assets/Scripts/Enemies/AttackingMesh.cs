using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackingMesh : MonoBehaviour {


    Enemy enemy;

    private void Start()
    {
        enemy = GetComponentInParent<Enemy>();
    }

    public int RollDamage
    {
        get
        {
            return enemy.GetAttackStrength();
        }
    }

    
}
