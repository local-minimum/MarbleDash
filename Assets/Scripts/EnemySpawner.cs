using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EnemySpawner : MonoBehaviour {

    [SerializeField]
    BoardGrid board;

    [SerializeField]
    int enemiesAtLevel = 3;

    List<GridPos> spawnLocations = new List<GridPos>();

    [SerializeField]
    Enemy[] enemyPrefabs;

    [SerializeField]
    Transform enemyParent;

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

    public void SpawnEnemies()
    {
        ClearCurrentEnemies();
        Debug.Log("Enemies: " + spawnLocations.Count);
        foreach (GridPos pos in spawnLocations)
        {
            Enemy e = GetEnemy();
            e.SetPosition(pos, board);
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
}
