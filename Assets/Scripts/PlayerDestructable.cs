using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDestructable : Destructable {

    public override int Health
    {
        get
        {
            return PlayerRunData.stats.Health;
        }

        protected set
        {
            PlayerRunData.stats.Health = value;
        }
    }

    public override float PartialHealth
    {
        get
        {
            return PlayerRunData.stats.PartialHealth;
        }
    }

    protected override void SetInitialHealthOnStart()
    {
        //Loading should not change health.
    }

    
}
