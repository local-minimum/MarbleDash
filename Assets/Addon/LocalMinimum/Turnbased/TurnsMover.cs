using System;
using System.Collections.Generic;
using UnityEngine;

using LocalMinimum.Grid;

namespace LocalMinimum.TurnBased
{
    public class TurnsMover : Singleton<TurnsMover>
    {

        IGridder grid;

        [SerializeField]
        AnimationCurve noNormalAnimation = new AnimationCurve();

        public void SetGrid(IGridder grid)
        {
            this.grid = grid;
        }

        public void Move(ITurnsActive turnsActive, GridPos source, GridPos target, AnimationCurve planarAnimation, Action callback)
        {
            Coroutine c = StartCoroutine(Animate(turnsActive, source, target, planarAnimation, noNormalAnimation, 1, 1, 0, 0, callback));
            movers[turnsActive] = c;
        }

        public void Move(ITurnsActive turnsActive, GridPos source, GridPos target, AnimationCurve planarAnimation, int turns, Action callback)
        {
            Coroutine c = StartCoroutine(Animate(turnsActive, source, target, planarAnimation, noNormalAnimation, 1, turns, 0, 0, callback));
            movers[turnsActive] = c;
        }

        public void Move(ITurnsActive turnsActive, GridPos source, GridPos target, AnimationCurve planarAnimation, int turns, float delayStartTurnFraction, float preemptiveEndTurnFraction, Action callback)
        {
            Coroutine c = StartCoroutine(Animate(turnsActive, source, target, planarAnimation, noNormalAnimation, 1, turns, delayStartTurnFraction, preemptiveEndTurnFraction, callback));
            movers[turnsActive] = c;
        }

        public void Move(ITurnsActive turnsActive, GridPos source, GridPos target, AnimationCurve planarAnimation, AnimationCurve normalAnimation, Action callback)
        {

            Coroutine c = StartCoroutine(Animate(turnsActive, source, target, planarAnimation, normalAnimation, 1, 1, 0, 0, callback));
            movers[turnsActive] = c;
        }

        public void Move(ITurnsActive turnsActive, GridPos source, GridPos target, AnimationCurve planarAnimation, AnimationCurve normalAnimation, float normalHeight, Action callback)
        {

            Coroutine c = StartCoroutine(Animate(turnsActive, source, target, planarAnimation, normalAnimation, normalHeight, 1, 0, 0, callback));
            movers[turnsActive] = c;
        }

        public void Move(ITurnsActive turnsActive, GridPos source, GridPos target, AnimationCurve planarAnimation, AnimationCurve normalAnimation, float normalHeight, int turns, Action callback)
        {
            Coroutine c = StartCoroutine(Animate(turnsActive, source, target, planarAnimation, normalAnimation, normalHeight, turns, 0, 0, callback));
            movers[turnsActive] = c;
        }

        public void Move(ITurnsActive turnsActive, GridPos source, GridPos target, AnimationCurve planarAnimation, AnimationCurve normalAnimation, float normalHeight, int turns, float delayStartTurnFraction, float preemptiveEndTurnFraction, Action callback)
        {
            Coroutine c = StartCoroutine(Animate(turnsActive, source, target, planarAnimation, normalAnimation, normalHeight, turns, delayStartTurnFraction, preemptiveEndTurnFraction, callback));
            movers[turnsActive] = c;
        }

        Dictionary<ITurnsActive, Coroutine> movers = new Dictionary<ITurnsActive, Coroutine>();

        public bool InterruptMove(ITurnsActive turnsActive)
        {
            if (movers.ContainsKey(turnsActive))
            {
                StopCoroutine(movers[turnsActive]);
                movers.Remove(turnsActive);
                return true;
            }

            return false;
        }
        bool debugging = true;

        IEnumerator<WaitForSeconds> Animate(
            ITurnsActive turnsActive, GridPos source, GridPos target,
            AnimationCurve planarAnimation, AnimationCurve normalAnimation,
            float normalHeight, int turns, float delayStartTurnFraction, float preemptiveEndTurnFraction, Action callback)
        {
            float turnTime = TurnsManager.instance.TurnTime;
            float totalTime = turnTime * turns;
            float startTime = Time.timeSinceLevelLoad;
                        
            Vector3 localSpaceSource = grid.Position(source, Space.Self);
            Vector3 localSpaceTarget = grid.Position(target, Space.Self);

            Vector3 normal = grid.Normal;

            yield return new WaitForSeconds(delayStartTurnFraction * turnTime);
            bool debugMe = !debugging;
            debugging = true;

            float endTime = startTime + totalTime - preemptiveEndTurnFraction * turnTime;
            float animStartTime = Time.timeSinceLevelLoad;
            float animationDuration = endTime - animStartTime;
            float progress = 0;
            if (debugMe)
            {
                Debug.Log(animationDuration);
            }
            float animTick = 0.017f;
            while (progress < 1f)
            {
                progress = (Time.timeSinceLevelLoad - animStartTime) / animationDuration;

                turnsActive.transform.position = grid.GetWorldPosition(

                    Vector3.Lerp(localSpaceSource, localSpaceTarget, planarAnimation.Evaluate(progress)) +
                    normal * (turnsActive.gridNormalOffset + normalAnimation.Evaluate(progress) * normalHeight)
                    );

                if (debugMe)
                {
                    Debug.Log(progress);
                    Debug.Log(planarAnimation.Evaluate(progress));
                }

                if (animationDuration - (Time.timeSinceLevelLoad - startTime) > animTick)
                {
                    yield return new WaitForSeconds(animTick);
                } else
                {
                    break;
                }
            }

            progress = 1;

            turnsActive.transform.position = grid.GetWorldPosition(

                Vector3.Lerp(localSpaceSource, localSpaceTarget, planarAnimation.Evaluate(progress)) +
                normal * (turnsActive.gridNormalOffset + normalAnimation.Evaluate(progress) * normalHeight)
                );

            if (debugMe)
            {
                debugging = false;
            }

            movers.Remove(turnsActive);
            if (callback != null)
            {
                callback();
            }
        }
    }
}