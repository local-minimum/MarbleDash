using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDestructable : Destructable {

    int[] damageAbsorption;

    public void SetInitial(EnemyTier tier)
    {
        maxHealth = PlayerRunData.stats.lvlRnd.Range(tier.startHealthMin, tier.startHealthMax);
        health = maxHealth;
        damageAbsorption = tier.damageAbsorption;
    }

    protected override int DamageAbsorption(int hitPart)
    {
        if (damageAbsorption != null && damageAbsorption.Length > hitPart)
        {
            return damageAbsorption[hitPart];
        }

        return 0;
    }
}
