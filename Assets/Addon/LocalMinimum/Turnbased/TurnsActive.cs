using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LocalMinimum.TurnBased
{
    public abstract class TurnsActive<T> : MonoBehaviour
    {

        TurnsManager turnsManager;
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
                }
                else
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
            if (turnsManager == null)
            {
                turnsManager = TurnsManager.instance;
            }
            turnsManager.OnTurnTick += OnTurnTick;
            
        }


        void OnDisable()
        {            
            turnsManager.OnTurnTick -= OnTurnTick;
        }

        public void ForceFreeze()
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

        private void OnTurnTick(int turnIndex, float tickTime)
        {

            remainingTicks--;

            if (isRespondingToTicks)
            {

                System.Func<int, float, T> func = SelectAction(turnIndex, tickTime, out remainingTicks);
                if (func != null)
                {
                    T nextAction = func(turnIndex, tickTime);
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
                if (recentTurnsActions.ContainsKey(_currentAction))
                {
                    recentTurnsActions[_currentAction]++;
                }
                else
                {
                    recentTurnsActions[_currentAction] = 1;
                }
                if (totalTurnsActions.ContainsKey(_currentAction))
                {
                    totalTurnsActions[_currentAction]++;
                }
                else
                {
                    totalTurnsActions[_currentAction] = 1;
                }
            }
        }

        protected abstract bool ContinuedSelection(T newlySelected);

        public abstract bool Interrupt(bool force);

        public abstract System.Func<int, float, T> SelectAction(int turnIndex, float tickTime, out int turns);

    }

}
