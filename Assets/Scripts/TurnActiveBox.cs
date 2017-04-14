using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LocalMinimum.TurnBased;
using System;

public enum BoxStates {Standing, Sliding, Breaking};

public class TurnActiveBox : TurnsActive<BoxStates> {

    CoinBox box;

    private void Awake()
    {
        box = GetComponent<CoinBox>();
    }

    protected override bool ContinuedSelection(BoxStates newlySelected)
    {
        return _currentAction == newlySelected;
    }

    public override Func<int, float, BoxStates> SelectAction(int turnIndex, float tickTime, out int turns)
    {
        return box.SelectAction(turnIndex, tickTime, out turns);
    }

    public override bool Interrupt(bool force)
    {
        return true;
    }
}
