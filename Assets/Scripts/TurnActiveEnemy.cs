using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LocalMinimum.TurnBased;

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

    public override Func<int, float, EnemyMode> SelectAction(int turnIndex, float tickTime, out int turns)
    {
        _enemy.SelectActionBehaviour(turnIndex, tickTime);
        turns = _enemy.GetActionDuration();
        return _enemy.GetActionFunction();
    }

    protected override bool ContinuedSelection(EnemyMode newlySelected)
    {
        return newlySelected == _currentAction;
    }
}
