using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EnemySpawner : MonoBehaviour {

    static EnemySpawner _instance;

    public static EnemySpawner instance
    {
        get
        {
            if (_instance == null)
            {
                _instance =FindObjectOfType<EnemySpawner>();
            }
            return _instance;
        }
    }

    [SerializeField]
    BoardGrid board;

    [SerializeField]
    int enemiesAtLevel = 3;

    List<GridPos> spawnLocations = new List<GridPos>();

    [SerializeField]
    Enemy[] enemyPrefabs;

    [SerializeField]
    Transform enemyParent;

    private void Awake()
    {
        if (_instance == null || _instance == this)
        {
            _instance = this;
        } else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    public void AllocatePlaces()
    {
        spawnLocations.Clear();
        GridPos[] potentials = board.FindIsOnlyAny(Occupancy.Free, Occupancy.BallPath).ToArray().Shuffle();
        for (int i=0; i<enemiesAtLevel; i++)
        {
            spawnLocations.Add(potentials[i]);
            board.Occupy(potentials[i], Occupancy.Enemy);
        }
    }

    List<Enemy> enemiesOnLevel = new List<Enemy>();

    public void SpawnEnemies()
    {
        enemiesOnLevel.Clear();
        ClearCurrentEnemies();

        Debug.Log("Enemies: " + spawnLocations.Count);
        foreach (GridPos pos in spawnLocations)
        {
            Enemy e = GetEnemy();
            e.SetPosition(pos, board);
            enemiesOnLevel.Add(e);
        }
    }

    void ClearCurrentEnemies()
    {
        for (int i=0, l=enemyParent.childCount; i<l; i++)
        {
            Destroy(enemyParent.GetChild(i).gameObject, 0.1f);
        }
    }

    Enemy GetEnemy()
    {
        return Instantiate(enemyPrefabs[Random.Range(0, enemyPrefabs.Length)], enemyParent, false);
    }

    public void iDied(Enemy deader)
    {
        enemiesOnLevel.Remove(deader);
        if (enemiesOnLevel.Count == 0)
        {
            StoreSwapper.instance.ShowStoreSwapping();
        }
    }
}
