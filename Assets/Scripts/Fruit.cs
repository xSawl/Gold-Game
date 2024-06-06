using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FruitType{Apple, Banana, Cherry, Kiwi, Melon, Orange, Pineapple, Strawberry}

public class Fruit : MonoBehaviour
{

    [SerializeField] private FruitType fruitType;
    [SerializeField] private GameObject pickupVFX;

    private GameManager gameManager;
    private Animator anim;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
    }
    
    void Start()
    {
        gameManager = GameManager.instance;
        SetRandomLookIfNeeded();

    }

    private void SetRandomLookIfNeeded()
    {
        if(gameManager.FruitsHaveRandomLook() == false) 
        {
            UpdateFruitVisuals();
            return;
        }

        int randomIndex = Random.Range(0,8); //le 8 est exclu donc dernier chiffre = 7
        anim.SetFloat("fruitIndex", randomIndex);
    }

    private void UpdateFruitVisuals() => anim.SetFloat("fruitIndex", (int)fruitType);

    private void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.GetComponent<Player>();

        if(player != null)
        {
            gameManager.AddFruit();
            Destroy(gameObject);
            
            GameObject newFX = Instantiate(pickupVFX, transform.position, Quaternion.identity);
            Destroy(newFX, .5f);
        }
    }


}
