using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LocalMinimum.Grid;

public class DirtProjectile : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    int attackStrength;
    bool flying = false;
    Vector3 localForce;
    BoardGrid board;

    public void Throw(BoardGrid board, GridPos from, GridPos to, int attackStrength)
    {
        this.board = board;
        gameObject.SetActive(true);
        this.attackStrength = attackStrength;
        StartCoroutine(AirTime(from, to));
    }

    IEnumerator<WaitForSeconds> AirTime(GridPos from, GridPos to)
    {
        flying = true;
        Vector3 boardSource = board.GetLocalPosition(from) + DirtBallsManager.instance.offset;
        Vector3 boardTarget = board.GetLocalPosition(to) + DirtBallsManager.instance.offset;
        localForce = (boardTarget - boardSource).normalized;
        float progress = DirtBallsManager.instance.startTime;
        float startProgress = progress;            
        float speed = DirtBallsManager.instance.flySpeed;
        float distance = Vector3.Distance(boardTarget, boardSource);
        float startTime = Time.timeSinceLevelLoad;
        

        while (flying)
        {

            progress = startProgress + speed * (Time.timeSinceLevelLoad - startTime);
            transform.localPosition = Vector3.Lerp(boardSource, boardTarget, progress / distance);
            if (progress > distance)
            {
                flying = false;
                DirtBallsManager.instance.RecycleProjectile(this);
            }

            yield return new WaitForSeconds(0.016f);
        }

        
    }

    void OnCollisionEnter(Collision col)
    {
        
        Destructable d = col.gameObject.GetComponent<Destructable>();
        d.Hurt(attackStrength, 0);
        flying = false;
        DirtBallsManager.instance.RecycleProjectile(this);
        if (d.gameObject.layer == Level.playerLevel)
        {
            d.GetComponent<Rigidbody>().AddForceAtPosition(board.transform.TransformVector(localForce) * DirtBallsManager.instance.impactForce, col.contacts[0].point);
        }
    }
}
