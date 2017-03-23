using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyTier {

    public int difficulty;

    //List of behaviours to select from each turn
    public EnemyMode[] availableModes;

    public int startHealthMin;
    public int startHealthMax;

    //Consider each attack type have an attack value range
    //Need as many as the included attack modes in the enum
    public int[] minAttack;
    public int[] maxAttack;

    //Behaviour priority weights
    public float[] behaviourProbWeights;

    //Behaviour cooldowns
    public int[] behaviourCoolDowns;

    //One per submesh
    public int[] damageAbsorption;
    public int[] damageReflection;
    
}
