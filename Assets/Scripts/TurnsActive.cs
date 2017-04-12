using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TurnsActive<T> : MonoBehaviour
{

    Level lvl;
    int remainingTicks = 0;

    Dictionary<T, int> totalTurnsActions = new Dictionary<T, int>();
    Dictionary<T, int> recentTurnsActions = new Dictionary<T, int>();
    Dictionary<T, int> totalSelectionsActions = new Dictionary<T, int>();
    Dictionary<T, int> recentSelectionActionSelections = new Dictionary<T, int>();

    protected T _currentAction;
    bool _anyAction;

    public int continiousTurnsCurrentAction
    {
        get
        {
            if (_anyAction)
            {
                return recentTurnsActions[_currentAction];
            } else
            {
                return 0;
            }
        }
    }

    public T MostRecentAction
    {
        get
        {
            return _currentAction;
        }
    }

    public int GetTotalSelections(T action)
    {
        if (totalSelectionsActions.ContainsKey(action))
        {
            return 0;
        }
        return totalSelectionsActions[action];
    }

    public int GetMostRecentSelections(T action)
    {
        if (recentSelectionActionSelections.ContainsKey(action))
        {
            return 0;
        }
        return recentSelectionActionSelections[action];
    }

    public int GetTotalTurns(T action)
    {
        if (totalTurnsActions.ContainsKey(action))
        {
            return 0;
        }
        return totalTurnsActions[action];
    }

    public int GetMostRecentTurns(T action)
    {
        if (recentTurnsActions.ContainsKey(action))
        {
            return 0;
        }
        return recentTurnsActions[action];
    }

    bool isRespondingToTicks
    {
        get
        {
            return remainingTicks < 1;
        }
    }

    void OnEnable()
    {
        if (lvl == null)
        {
            lvl = Level.instance;
        }
        lvl.OnTurnTick += Lvl_OnTurnTick;
        lvl.OnNewLevel += Lvl_OnNewLevel;
    }


    void OnDisable()
    {
        lvl.OnNewLevel -= Lvl_OnNewLevel;
        lvl.OnTurnTick -= Lvl_OnTurnTick;
    }

    private void Lvl_OnNewLevel()
    {
        if (!isRespondingToTicks)
        {
            Interrupt(true);
            NoAction();
        }
    }

    void NoAction()
    {
        _anyAction = false;

    }
    


    private void Lvl_OnTurnTick(PlayerController player, int turnIndex, float tickTime)
    {

        remainingTicks--;

        if (isRespondingToTicks)
        {

            System.Func<PlayerController, int, float, T> func = SelectAction(player, turnIndex, tickTime, out remainingTicks);
            if (func != null)
            {
                T nextAction = func(player, turnIndex, tickTime);
                if (_anyAction && ContinuedSelection(nextAction))
                {
                    recentSelectionActionSelections[nextAction]++;
                }
                else
                {
                    recentSelectionActionSelections[nextAction] = 1;
                }

                if (totalSelectionsActions.ContainsKey(nextAction))
                {
                    totalSelectionsActions[nextAction]++;
                }
                else
                {
                    totalSelectionsActions[nextAction] = 1;
                }

                _currentAction = nextAction;
                _anyAction = true;

                recentTurnsActions[_currentAction] = 1;

                if (totalTurnsActions.ContainsKey(_currentAction))
                {
                    totalTurnsActions[_currentAction]++;
                }
                else
                {
                    totalTurnsActions[_currentAction] = 1;
                }
            }
            else
            {
                _anyAction = false;
            }
        }
        else
        {
            if (recentTurnsActions.ContainsKey(_currentAction)) {
                recentTurnsActions[_currentAction]++;
            } else
            {
                recentTurnsActions[_currentAction] = 1;
            }
            if (totalTurnsActions.ContainsKey(_currentAction))
            {
                totalTurnsActions[_currentAction]++;
            } else
            {
                totalTurnsActions[_currentAction] = 1;
            }
        }
    }

    protected abstract bool ContinuedSelection(T newlySelected);

    public abstract bool Interrupt(bool force);

    public abstract System.Func<PlayerController, int, float, T> SelectAction(PlayerController player, int turnIndex, float tickTime, out int turns);

}
