using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyTier {

    public int difficulty;

    [Tooltip("Available only from this level index")]
    public int firstLevel;

    //List of behaviours to select from each turn
    [Tooltip("All action modes, order of attacks is important for attack values.")]
    public EnemyMode[] availableModes;
    
    public int startHealthMin;
    public int startHealthMax;

    //Consider each attack type have an attack value range
    //Need as many as the included attack modes in the enum
    [Tooltip("The last value will be used on attacks with higher index")]
    public int[] minAttack;
    [Tooltip("The last value will be used on attacks with higher index")]
    public int[] maxAttack;

    //Behaviour priority weights
    [Tooltip("Order of weights same as behaviours")]
    public float[] behaviourProbWeights;

    //Behaviour cooldowns
    public int[] behaviourCoolDowns;

    //One per submesh
    [Tooltip("If not matching number of submeshes will use first value")]
    public int[] damageAbsorption;
    [Tooltip("If not matching number of submeshes will use first value")]
    public int[] damageReflection;
    
}
