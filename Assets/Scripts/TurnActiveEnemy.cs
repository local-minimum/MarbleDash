using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnActiveEnemy : TurnsActive<EnemyMode>
{
    Enemy _enemy;

    void Awake()
    {
        _enemy = GetComponent<Enemy>();
    }

    public override bool Interrupt(bool force)
    {
        throw new NotImplementedException();
    }

    public override Func<PlayerController, int, float, EnemyMode> SelectAction(out int turns)
    {
        throw new NotImplementedException();
    }
}
