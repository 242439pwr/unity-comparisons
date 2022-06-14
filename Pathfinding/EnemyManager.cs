using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{

    [SerializeField]
    private GameObject enemyPrefab;

    [SerializeField]
    private int numberOfEnemies;

    [SerializeField]
    private MechanismType mechanismType;

    private List<GameObject> enemies = new List<GameObject>();
    private Task[] _tasks;
    private float loopTime = 2.5f;

    enum MechanismType
    {
        Coroutines,
        Threads,
        Jobs,
        Normal
    }

    void Start()
    {
        Spawn();
    }

    void Spawn()
    {
        for(int i = 0; i < numberOfEnemies; i++)
        {
            float xSpawn = Mathf.Round(Random.Range(-9f, 9f) * 10 / 10);
            Vector2 enemySpawnPos = new Vector2(xSpawn, 5f);
            GameObject enemy = Instantiate(enemyPrefab, enemySpawnPos, Quaternion.identity);
            enemy.GetComponent<EnemyAI>().OnInstantiate();
            enemies.Add(enemy);
        }
        _tasks = new Task[numberOfEnemies];

        switch (mechanismType)
        {
            case MechanismType.Coroutines:
                RunCoroutine(loopTime);
                break;
            case MechanismType.Threads:
                StartCoroutine(RunTasksCoroutine(loopTime));
                break;
            case MechanismType.Normal:
                break;
        }

    }

    void RunCoroutine(float loopTime)
    {
        foreach (var enemy in enemies)
        {
            enemy.GetComponent<EnemyAI>().RunCoroutine(loopTime);
        }
    }

    IEnumerator RunTasksCoroutine(float time)
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(time);
            RunTasks();
        }
    }

    void RunTasks()
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        foreach(var enemy in enemies)
        {
            enemy.GetComponent<EnemyAI>().RunTask();
        }
        sw.Stop();
    }

}
