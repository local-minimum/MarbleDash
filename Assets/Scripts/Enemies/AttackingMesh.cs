using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackingMesh : MonoBehaviour {

    [SerializeField]
    int minDmg;

    [SerializeField]
    int maxDmg;

    Enemy enemy;

    private void Start()
    {
        enemy = GetComponentInParent<Enemy>();
    }

    public int RollDamage
    {
        get
        {
            return enemy.Behaviour == EnemyMode.Attack1 ? Random.Range(minDmg, maxDmg) : 0;
        }
    }

    
}
