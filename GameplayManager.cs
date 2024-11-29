using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    public List<GameObject> buildingLevels = new List<GameObject>();

    [SerializeField] private TextMeshProUGUI coinTotalText;
    [SerializeField] private TextMeshProUGUI shopNameText;
    [SerializeField] private TMP_InputField shopNameInputFieldText;

    [SerializeField] private GameObject inputFieldPanel;

    [SerializeField] private string shopName;
    [SerializeField] private float spawnTimer;
    [SerializeField] private float minSpawnTime = 4f;  // Initial minimum spawn time
    [SerializeField] private float maxSpawnTime = 8f; // Initial maximum spawn time
    [SerializeField] private float minSpawnTimeCap = 0.75f; // Minimum cap for min spawn time
    [SerializeField] private float maxSpawnTimeCap = 1.5f; // Minimum cap for max spawn time

    [SerializeField] private Transform customerSpawnLocation;
    [SerializeField] private GameObject customerPrefab;

    [SerializeField] private int currentShopLevel = 0;
    [SerializeField] private int maxCustomersInLine = 7;  // Max number of customers in the line

    private List<GameObject> activeCustomers = new List<GameObject>();  // To track spawned customers
    private bool canSpawn = false;
    private int currentIndex;

    private float currentWorkSpeed = 5;
    private float currentMoveSpeed = 0.003f;
    private int currentPricePerDrink = 8;

    private Coroutine spawnCoroutine;

    public int CurrentShopLevel
    {
        get { return currentShopLevel; }
        set {
            currentShopLevel = value;
            AdjustSpawnTime();  // Adjust the spawn time when the shop level is updated
        }
    }


    private void Awake()
    {
        //CoinManager.currentCoinTotal = 1000000;

        InitBuilding();  // Initialize building

        SetCanSpawn(true);  // Use SetCanSpawn to track changes
    }

    private void Start()
    {
        inputFieldPanel.SetActive(false);

        currentIndex = 0;

        shopName = "chose shop name";
        shopNameText.text = shopName.ToUpper();
        StartSpawningCustomers();
    }

    private void Update()
    {
        coinTotalText.text = $"{CoinManager.CalculateIncome(CoinManager.currentCoinTotal)}";

        // Check if the line is backed up (either too many customers or distance too small)
        if (IsLineBackedUp()) {
            StopSpawningCustomers();
        } else {
            StartSpawningCustomers();  // Resume spawning if the line is not backed up
        }
    }

    public int GetCurrentPricePerDrink()
    {
        return currentPricePerDrink;
    }

    public void SetCurrentPricePerDrink(int newPrice)
    {
        currentPricePerDrink = newPrice;
    }

    public float GetCurrentMoveSpeed()
    {
        return currentMoveSpeed;
    }

    public void SetCurrentMoveSpeed(float newSpeed)
    {
        currentMoveSpeed = newSpeed;
    }

    // Method to get the current work speed (called by customers when they spawn)
    public float GetCurrentWorkSpeed()
    {
        return currentWorkSpeed;
    }

    // This method is called when the shop's work speed is upgraded
    public void SetCurrentWorkSpeed(float newSpeed)
    {
        currentWorkSpeed = newSpeed;
    }

    // Method to start spawning customers
    public void StartSpawningCustomers()
    {
        if (spawnCoroutine == null && canSpawn)  // Only start if not already running and allowed to spawn
        {
            spawnCoroutine = StartCoroutine(CustomerSpawnRoutine());
            //Debug.Log("Customer spawning started.");
        }
    }

    // Method to stop spawning customers
    public void StopSpawningCustomers()
    {
        if (spawnCoroutine != null) {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
            //Debug.Log("Customer spawning stopped.");
        }
    }

    // Coroutine for spawning customers
    private IEnumerator CustomerSpawnRoutine()
    {
        while (canSpawn) {
            // Wait for a random time between minSpawnTime and maxSpawnTime
            float waitTime = Random.Range(minSpawnTime, maxSpawnTime);
            //Debug.Log("Waiting " + waitTime + " seconds before spawning the next customer.");

            yield return new WaitForSeconds(waitTime);  // Wait for a random interval

            // Spawn a customer
            SpawnCustomer();
        }
    }

    // Method to spawn a customer
    private void SpawnCustomer()
    {
        GameObject newCustomer = Instantiate(customerPrefab, customerSpawnLocation.position, Quaternion.identity);
        activeCustomers.Add(newCustomer);  // Track the spawned customer
        //Debug.Log("Customer spawned. Total customers in line: " + activeCustomers.Count);
    }

    // Check if the line is backed up (too many customers or distance too small)
    private bool IsLineBackedUp()
    {
        // If too many customers in line
        if (activeCustomers.Count >= maxCustomersInLine) {
            //Debug.Log("Line is backed up: too many customers.");
            return true;
        }

        return false;  // Line is not backed up
    }

    // Method to remove customers from the active list
    public void CustomerLeftLine(GameObject customer)
    {
        if (activeCustomers.Contains(customer)) {
            activeCustomers.Remove(customer);
            //Debug.Log("Customer left the line. Remaining customers: " + activeCustomers.Count);
        }
    }

    // Adjust spawn times based on the shop level
    private void AdjustSpawnTime()
    {
        // Every 3 shop levels, decrease both minSpawnTime and maxSpawnTime by 1 second
        if (currentShopLevel % 2 == 0) {
            minSpawnTime = Mathf.Max(minSpawnTime - 1f, minSpawnTimeCap);  // Ensure it doesn't go below the minimum cap
            maxSpawnTime = Mathf.Max(maxSpawnTime - 1.75f, maxSpawnTimeCap);  // Ensure it doesn't go below the minimum cap

            //Debug.Log($"Shop level {currentShopLevel}: Reduced spawn times. Min: {minSpawnTime}, Max: {maxSpawnTime}");
        }
    }

    public void OnSetShopNamePanelButtonPressed()
    {
        inputFieldPanel.SetActive(!inputFieldPanel.activeInHierarchy);
    }

    public void OnShopNameUpdate()
    {
        shopNameText.text = shopNameInputFieldText.text.ToUpper();
        OnSetShopNamePanelButtonPressed();
    }

    private void InitBuilding()
    {
        // Deactivate all objects first
        for (int i = 0; i < buildingLevels.Count; i++) {
            buildingLevels[i].SetActive(false);
        }

        // Activate the current object
        buildingLevels[currentIndex].SetActive(true);
    }

    public void UpdateBuilding()
    {
        currentIndex++;

        // Deactivate all objects first
        for (int i = 0; i < buildingLevels.Count; i++) {
            buildingLevels[i].SetActive(false);
        }

        // Activate the current object
        buildingLevels[currentIndex].SetActive(true);
    }

    private void SetCanSpawn(bool value)
    {
        canSpawn = value;
    }
}
