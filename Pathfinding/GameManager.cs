using UnityEngine;

public class GameManager : MonoBehaviour
{

    [SerializeField]
    private GameObject enemyManagerPrefab;

    [SerializeField]
    private int numberOfEnemyManagers;

    void Start()
    {
        Spawn();
    }

    void Spawn()
    {
        for (int i = 0; i < numberOfEnemyManagers; i++)
        {
            GameObject enemyManager = Instantiate(enemyManagerPrefab);
        }
    }

}
