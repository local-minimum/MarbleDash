using System;
using System.Collections.Generic;
using UnityEngine;

namespace LocalMinimum.TurnBased
{
    public delegate void TurnTick(int turnIndex, float tickTime);

    public class TurnsManager : Singleton<TurnsManager>
    {

        public event TurnTick OnTurnTick;

        [SerializeField, Range(0, 2)]
        float turnTime;

        [SerializeField, Range(0, 10)]
        float firstTickDelay = 2f;

        bool ticking;

        bool makeTurns = false;

        public bool MakingTurns
        {
            get
            {
                return makeTurns;
            }

            set
            {
                if (value)
                {
                    makeTurns = true;
                }
                else {
                    makeTurns = false;
                    ticking = false;
                }
            }
        }

        void Start()
        {
            StartCoroutine(TurnTicker());
        }

        Action postTickAction = null;

        public void SetActionAfterTickEvents(Action postTickAction)
        {
            this.postTickAction = postTickAction;
        }

        IEnumerator<WaitForSeconds> TurnTicker()
        {
            ticking = makeTurns;
            int turnIndex = 0;
            while (true)
            {
                if (makeTurns == false)
                {
                    ticking = false;
                    yield return new WaitForSeconds(Mathf.Min(0.1f, turnTime));
                }
                else if (ticking == false)
                {
                    ticking = true;
                    if (firstTickDelay > 0)
                    {
                        yield return new WaitForSeconds(firstTickDelay);
                    }
                    turnIndex = 0;
                }
                else
                {
                    if (OnTurnTick != null)
                    {
                        OnTurnTick(turnIndex, turnTime);
                    }
                    if (postTickAction != null)
                    {
                        postTickAction();
                    }
                    postTickAction = null;
                    yield return new WaitForSeconds(turnTime);
                    turnIndex++;
                }
            }
        }

    }

}