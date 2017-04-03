using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LocalMinimum.Grid;
using LocalMinimum;

public class DirtBallsManager : Singleton<DirtBallsManager> {

    [SerializeField]
    DirtProjectile projectileTemplate;

    List<DirtProjectile> pool = new List<DirtProjectile>();

    BoardGrid board;

    [Range(0, 5)]
    public float flySpeed = 1.5f;

    [Range(0, 2)]
    public float startTime = 0.25f;

    public float impactForce = 20;

    public Vector3 offset;

    void Start()
    {
        board = BoardGrid.instance;
    }

    public void Throw(GridPos from, GridPos to, int attackStrength)
    {        
        DirtProjectile p = GetProjectile();
        pool.Remove(p);
        p.Throw(board, from, to, attackStrength);
    }

    DirtProjectile GetProjectile()
    {
        if (pool.Count > 0)
        {
            DirtProjectile p = pool[0];
            pool.RemoveAt(0);
            return p;
        } else
        {
            return Instantiate(projectileTemplate, transform, false);
        }
    }

    public void RecycleProjectile(DirtProjectile projectile)
    {
        projectile.gameObject.SetActive(false);
        pool.Add(projectile);
    }
    
}
