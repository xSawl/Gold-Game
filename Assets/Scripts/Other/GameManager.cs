using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private float respawnDelay;

    public Player player;

    [Header("Fruit Management")]
    public bool fruitsAreRandom;
    public int fruitCollected;
    public int totalFruits;

    private void Awake()
    {
        if(instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {   
        CollectFruitsInfo();
    }

    private void CollectFruitsInfo()
    {
        Fruit[] allFruits = FindObjectsByType<Fruit>(FindObjectsSortMode.None);
        totalFruits = allFruits.Length;
    }

    public void UpdateRespawnPosition(Transform newRespawnPoint) => respawnPoint = newRespawnPoint;
    public void RespawnPlayer() => StartCoroutine(RespawnRoutine());

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        GameObject newPlayer = Instantiate(playerPrefab, respawnPoint.position, Quaternion.identity);
        player = newPlayer.GetComponent<Player>();
    }

    public void AddFruit() => fruitCollected++;
    public bool FruitsHaveRandomLook() => fruitsAreRandom;
    
    }