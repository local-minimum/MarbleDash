using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using LocalMinimum.Grid;
using LocalMinimum.Collections;

public class EnemySpawner : MonoBehaviour {

    static EnemySpawner _instance;

    public static EnemySpawner instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<EnemySpawner>();
            }
            return _instance;
        }
    }

    [SerializeField]
    BoardGrid board;

    [SerializeField]
    int enemyDifficultyLoadPerLevel = 3;

    [SerializeField]
    int enemyDifficultyLoadMinRelative = -3;

    [SerializeField]
    int enemyDifficutlyLoadMaxRelative = 4;

    [SerializeField]
    int enemyDifficultyLoadMinAbs = 3;

    [SerializeField]
    int minEnemyDifficutly = 0;

    [SerializeField]
    int maxEnemyDifficutly = 1;

    [SerializeField]
    int minEnemyDiffIncreaseEach = 2;

    [SerializeField]
    int minEnemyDiffIncreaseAmount = 1;

    [SerializeField]
    int maxEnemyDifficultyIncreaseEach = 5;

    [SerializeField]
    int maxEnemyDiffIncreaseAmount = 6;

    [SerializeField]
    int minSpawnDistanceFromPlayerDrop = 3;

    [SerializeField]
    int minEnemyDifficultyCap = 1;

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

    Level lvl;

    private void OnEnable()
    {
        lvl = Level.instance;
        lvl.OnTurnTick += Instance_OnTurnTick;
    }

    private void OnDisable()
    {
        if (lvl)
        {
            lvl.OnTurnTick -= Instance_OnTurnTick;
        }
    }

    private void Instance_OnTurnTick(PlayerController player, int turnIndex, float tickTime)
    {
        for (int i=0, l=enemiesOnLevel.Count; i<l; i++)
        {
            enemiesOnLevel[i].Lvl_OnTurnTick(player, turnIndex, tickTime);
        }
    }

    List<KeyValuePair<Enemy, int>> toSpawn = new List<KeyValuePair<Enemy, int>>();
    int targetDifficultyLoad;
    int currentDifficultyLoad;

    public void AllocatePlacesAndDecideEnemies()
    {
        targetDifficultyLoad = PlayerRunData.stats.currentLevel * enemyDifficultyLoadPerLevel;
        currentDifficultyLoad = 0;
        int level = PlayerRunData.stats.currentLevel;
        if (level % minEnemyDiffIncreaseEach == 0)
        {
            minEnemyDifficutly = Mathf.Min(minEnemyDifficultyCap, minEnemyDifficutly + minEnemyDiffIncreaseAmount);
            
        }
        if (level % maxEnemyDifficultyIncreaseEach == 0)
        {
            maxEnemyDifficutly = Mathf.Min(
                enemyPrefabs.Max(e => e.GetMaxDifficulty()),
                maxEnemyDifficutly + maxEnemyDiffIncreaseAmount);
        }

        toSpawn.Clear();

        var validPrefabs = GetValidEnemies();

        if (validPrefabs.Length == 0)
        {
            Debug.LogWarning("No valid enemies for this difficulty");
            return;
        }

        int iterations = 0;
        int loadDelta = targetDifficultyLoad;
        
        while (iterations < 100)
        {
            int enemyIndex = PlayerRunData.stats.lvlRnd.Range(0, validPrefabs.Length);
            int enemyTierIndex = PlayerRunData.stats.lvlRnd.Range(0, validPrefabs[enemyIndex].Value.Count);
            KeyValuePair<int, int> tierData = validPrefabs[enemyIndex].Value[enemyTierIndex];

            if (Mathf.Abs(currentDifficultyLoad + tierData.Value - targetDifficultyLoad) < loadDelta)
            {

                toSpawn.Add(new KeyValuePair<Enemy, int>(validPrefabs[enemyIndex].Key, tierData.Key));
                currentDifficultyLoad += tierData.Value;
                loadDelta = Mathf.Abs(targetDifficultyLoad - currentDifficultyLoad);

            } else if (currentDifficultyLoad >= Mathf.Max(targetDifficultyLoad + enemyDifficultyLoadMinRelative,
                enemyDifficultyLoadMinAbs))
            {
                break;
            }

            if (currentDifficultyLoad > targetDifficultyLoad + enemyDifficutlyLoadMaxRelative)
            {
                int removeIndex = PlayerRunData.stats.lvlRnd.Range(0, toSpawn.Count);
                currentDifficultyLoad -= toSpawn[removeIndex].Key.GetDifficulty(toSpawn[removeIndex].Value);
                loadDelta = Mathf.Abs(targetDifficultyLoad - currentDifficultyLoad);
                toSpawn.RemoveAt(removeIndex);
            }
            iterations++;
        }

        ReservePositions();

    }

    void ReservePositions()
    {
        GridPos playerDrop = board.Find(Occupancy.BallPathSource).First();
        spawnLocations.Clear();
        GridPos[] potentials = board
            .FindIsOnlyAny(Occupancy.Free, Occupancy.BallPath)
            .Where(e => GridPos.ShortestDimension(e, playerDrop) >= minSpawnDistanceFromPlayerDrop)
            .ToArray()
            .Shuffle();

        for (int i = 0, l = toSpawn.Count; i < l; i++)
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
        for(int i=0, l=spawnLocations.Count; i<l; i++)        
        {
            Enemy e = GetEnemy(toSpawn[i]);
            e.SetPosition(spawnLocations[i], board);
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

    KeyValuePair<Enemy, List<KeyValuePair<int, int>>>[] GetValidEnemies()
    {
        int lvlIndex = PlayerRunData.stats.currentLevel;

        return enemyPrefabs
            .Select(e => new KeyValuePair<Enemy, List<KeyValuePair<int, int>>>(
                e, e.GetTiersInDifficutlyRange(lvlIndex, minEnemyDifficutly, maxEnemyDifficutly)))
            .Where(e => e.Value.Count > 0)
            .ToArray();
    }

    Enemy GetEnemy(KeyValuePair<Enemy, int> prefabAndTier)
    {


        Enemy enemy = Instantiate(prefabAndTier.Key, enemyParent, false);
        enemy.SetTier(prefabAndTier.Value);
               
        return enemy;
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
