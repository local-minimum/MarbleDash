using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LocalMinimum.Grid;
using LocalMinimum.TurnBased;

public class Bumper : MonoBehaviour {

    [SerializeField]
    Animator anim;

    [SerializeField]
    string bumpAnim = "Bump";

    [SerializeField]
    float bumpForce = 10;

    [SerializeField]
    SphereCollider bumpRangeTrigger;

    [SerializeField]
    bool bumpOnTurn;

    int playerLayer;

    private void Awake()
    {
        playerLayer = LayerMask.NameToLayer("player");
    }

    public void SetPosition(BoardGrid board, GridPos pos) {
        transform.localPosition = board.GetLocalPosition(pos);
        gameObject.SetActive(true);
    }

    TurnsManager turnManager;

    private void OnEnable()
    {
        turnManager = TurnsManager.instance;
        turnManager.OnTurnTick += HanldeTurnTick;

    }

    private void OnDisable()
    {
        turnManager.OnTurnTick -= HanldeTurnTick;
    }

    private void HanldeTurnTick(int turnId, float tickTime)
    {
        if (playerRB && bumpOnTurn)
        {
            StartCoroutine(BumpIt(playerRB));
        }
    }

    IEnumerator<WaitForSeconds> BumpIt(Rigidbody playerRB)
    {
        anim.SetTrigger(bumpAnim);
        yield return new WaitForSeconds(0.1f);
        playerRB.AddExplosionForce(bumpForce, transform.position, bumpRangeTrigger.radius * 2);
    }

    Rigidbody playerRB;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == playerLayer)
        {
            playerRB = other.GetComponent<Rigidbody>();
            if (!bumpOnTurn)
            {
                StartCoroutine(BumpIt(playerRB));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Rigidbody>() == playerRB)
        {
            playerRB = null;
        }
    }
}
