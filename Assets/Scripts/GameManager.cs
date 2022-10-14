using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager gameManager;
    public GameObject dadPlayerPrefab;
    public GameObject skeletonPlayerPrefab;
    private int targetFps = 60;

    public Transform spawnPoint;//For later
    public int spawnDelay = 2;

    // Start is called before the first frame update
    void Start()
    {
        if (gameManager == null)
        {
            gameManager = this;
        }
        Instantiate(skeletonPlayerPrefab);
        Instantiate(dadPlayerPrefab);
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFps;
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.targetFrameRate != targetFps)
        {
            Application.targetFrameRate = targetFps;
        }
    }

    public IEnumerator RespawnPlayer()
    {
        yield return new WaitForSeconds(spawnDelay);
        Instantiate(skeletonPlayerPrefab);
        Instantiate(dadPlayerPrefab);
    }     

    public static void KillPlayer(PlayerController player)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; i++)
        {
            Destroy(players[i]);
        }
        gameManager.StartCoroutine(gameManager.RespawnPlayer());
    }
}
