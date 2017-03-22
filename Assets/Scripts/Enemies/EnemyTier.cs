using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyTier {

    public int difficulty;

    public int startHealth;

    public int[] minAttack;
    public int[] maxAttack;

    public EnemyMode[] availableModes;
}
