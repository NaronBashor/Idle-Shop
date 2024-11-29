using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Shop : MonoBehaviour
{
    GameplayManager gM;

    [SerializeField] private GameObject maxShopLevelGO;
    [SerializeField] private GameObject maxWorkSpeedGO;

    public List<GameObject> disableWhenShopMaxLevel = new List<GameObject>();
    public List<GameObject> disableWhenSpeedMaxLevel = new List<GameObject>();
    public List<GameObject> disableWhenUpgradePriceMaxLevel = new List<GameObject>();

    [Header("Modifiers")]
    [SerializeField] private float shopLevelMultiplier;
    [SerializeField] private float workSpeedMultiplier;
    [SerializeField] private float upgradePriceMultiplier;

    [SerializeField] private float shopLevelCost;
    [SerializeField] private float workSpeedCost;
    [SerializeField] private float upgradePriceCost;

    [SerializeField] private float initialWorkSpeedLevel;
    [SerializeField] private float workSpeedReductionPercent;

    [Header("Attribute Levels")]
    [SerializeField] public int shopLevel = 1;
    [SerializeField] public int shopWorkSpeedLevel = 1;
    [SerializeField] public int shopUpgradePriceLevel = 1;

    [Header("TMPS")]
    [SerializeField] private TextMeshProUGUI shopLevelText;
    [SerializeField] private TextMeshProUGUI shopWorkSpeedLevelText;
    [SerializeField] private TextMeshProUGUI shopUpgradePriceLevelText;
    [SerializeField] private TextMeshProUGUI nextShopLevelText;
    [SerializeField] private TextMeshProUGUI nextShopWorkSpeedLevelText;
    [SerializeField] private TextMeshProUGUI nextShopUpgradePriceLevelText;

    [SerializeField] private TextMeshProUGUI shopLevelCostText;
    [SerializeField] private TextMeshProUGUI workSpeedCostText;
    [SerializeField] private TextMeshProUGUI upgradePriceCostText;

    private bool isMaxLevelShop = false;
    private bool isMaxLevelWorkSpeed = false;
    private bool isMaxLevelPrice = false;

    private void Awake()
    {
        GameObject gameManager = GameObject.Find("GameManager");
        gM = gameManager.GetComponent<GameplayManager>();
    }

    private void Update()
    {
        UpdateTMPObjects();
    }

    private void UpdateTMPObjects()
    {
        shopLevelCost = (50 * Mathf.Pow(shopLevelMultiplier, shopLevel)) * 0.75f;
        workSpeedCost = (50 * Mathf.Pow(workSpeedMultiplier, shopWorkSpeedLevel)) * 0.75f;
        upgradePriceCost = (50 * Mathf.Pow(upgradePriceMultiplier, shopUpgradePriceLevel)) * 0.75f;

        shopLevelCostText.text = CoinManager.CalculateIncome((int)shopLevelCost);
        workSpeedCostText.text = CoinManager.CalculateIncome((int)workSpeedCost);
        upgradePriceCostText.text = CoinManager.CalculateIncome((int)upgradePriceCost);

        shopLevelText.text = shopLevel.ToString();
        shopWorkSpeedLevelText.text = shopWorkSpeedLevel.ToString();
        shopUpgradePriceLevelText.text = shopUpgradePriceLevel.ToString();
        nextShopLevelText.text = (shopLevel + 1).ToString();
        nextShopWorkSpeedLevelText.text = (shopWorkSpeedLevel + 1).ToString();
        nextShopUpgradePriceLevelText.text = (shopUpgradePriceLevel + 1).ToString();
    }

    private void UpgradeFailMessage()
    {
        Debug.Log("Not enough coins to upgrade!");
    }

    private void UpgradeSuccessMessage()
    {
        //Debug.Log("Upgrade Success!");
    }

    public void UpgradeShopLevel()
    {
        if (isMaxLevelShop) {
            return;  // If the shop is already at max level, do nothing
        }

        // Check if the player has enough coins to upgrade
        if (CoinManager.currentCoinTotal <= shopLevelCost) {
            UpgradeFailMessage();
            return;
        }

        // Perform the upgrade
        shopLevel++;  // Increase shop level

        // If the shop reaches the max level (10)
        if (shopLevel >= 10) {
            shopLevel = 10;  // Make sure it's exactly 10
            isMaxLevelShop = true;  // Mark it as the max level
            gM.CurrentShopLevel = shopLevel;  // Update the shop level in the gameplay manager
            gM.UpdateBuilding();  // Update the building to reflect the final level

            // Set the shop's TMP text and UI to indicate it's the max level
            SetShopLevelTMPToMaxLevel();
            UpgradeSuccessMessage();
            CoinManager.RemoveGold((int)shopLevelCost);
            return;
        }

        // If the shop hasn't reached max level, proceed with the upgrade
        UpgradeSuccessMessage();  // Show success message
        CoinManager.RemoveGold((int)shopLevelCost);  // Deduct coins

        gM.CurrentShopLevel = shopLevel;  // Update the current shop level
        gM.UpdateBuilding();  // Update the building accordingly
    }


    private void SetShopLevelTMPToMaxLevel()
    {
        maxShopLevelGO.SetActive(true);
        foreach (GameObject go in disableWhenShopMaxLevel) {
            go.SetActive(false);
        }
    }

    private void SetWorkSpeedLevelTMPToMaxLevel()
    {
        maxWorkSpeedGO.SetActive(true);
        foreach (GameObject go in disableWhenSpeedMaxLevel) {
            go.SetActive(false);
        }
    }

    public void UpgradeWorkSpeedLevel()
    {
        if (isMaxLevelWorkSpeed) {
            return;
        }

        // Check if the player has enough coins to upgrade
        if (CoinManager.currentCoinTotal <= workSpeedCost) {
            UpgradeFailMessage();
            return;
        }

        // Increase work speed level
        shopWorkSpeedLevel++;

        // Calculate the cumulative reduction factor for work speed
        float reductionFactor = Mathf.Pow(1 - workSpeedReductionPercent, (shopWorkSpeedLevel - 1));
        float reducedValue = 5 * reductionFactor;

        if (shopWorkSpeedLevel >= 20) {
            shopWorkSpeedLevel = 20;
            isMaxLevelWorkSpeed = true;

            SetWorkSpeedLevelTMPToMaxLevel();

            UpgradeSuccessMessage();  // Show success message
            CoinManager.RemoveGold((int)workSpeedCost);  // Deduct coins

            // Ensure a minimum cap for the work speed
            reducedValue = Mathf.Max(0.5f, reducedValue);

            // Update future customers with the new work speed
            GameObject.Find("GameManager").GetComponent<GameplayManager>().SetCurrentWorkSpeed(reducedValue);

            // Update all current customers' work speed
            UpdateAllCurrentCustomers(reducedValue);

            // Every 3rd level, increase customer movement speed
            if (shopWorkSpeedLevel % 3 == 0) {
                IncreaseCustomerMovementSpeed();
            }

            return;
        }

        UpgradeSuccessMessage();  // Show success message
        CoinManager.RemoveGold((int)workSpeedCost);  // Deduct coins

        // Ensure a minimum cap for the work speed
        reducedValue = Mathf.Max(0.5f, reducedValue);

        // Update future customers with the new work speed
        GameObject.Find("GameManager").GetComponent<GameplayManager>().SetCurrentWorkSpeed(reducedValue);

        // Update all current customers' work speed
        UpdateAllCurrentCustomers(reducedValue);

        // Every 3rd level, increase customer movement speed
        if (shopWorkSpeedLevel % 3 == 0) {
            IncreaseCustomerMovementSpeed();
        }
    }

    // Method to increase customer movement speed
    private void IncreaseCustomerMovementSpeed()
    {
        // Define the movement speed increase (adjust as necessary)
        float speedIncrease = 0.005f * shopWorkSpeedLevel;
        float newSpeed = 0.003f + speedIncrease;

        // Update future customers with the new work speed
        GameObject.Find("GameManager").GetComponent<GameplayManager>().SetCurrentMoveSpeed(newSpeed);

        // Find all customers in the scene and increase their movement speed
        CustomerController[] allCustomers = UnityEngine.Object.FindObjectsByType<CustomerController>(FindObjectsSortMode.None);

        foreach (CustomerController customer in allCustomers) {
            customer.MoveSpeed += newSpeed;
        }

        //Debug.Log($"Increased movement speed of {allCustomers.Length} customers by {speedIncrease}");
    }

    // Method to update all currently spawned customers with the new reduced value
    private void UpdateAllCurrentCustomers(float newSpeed)
    {
        // Find all objects of type CustomerController currently in the scene
        CustomerController[] allCustomers = UnityEngine.Object.FindObjectsByType<CustomerController>(FindObjectsSortMode.None);

        // Iterate through each customer and update their work speed
        foreach (CustomerController customer in allCustomers) {
            customer.TimePerCustomerToOrder = newSpeed;
        }

        //Debug.Log($"Updated {allCustomers.Length} customers to the new work speed.");
    }

    public void UpgradePriceLevel()
    {
        //if (isMaxLevelPrice) {
        //    return;
        //}

        // Check if the player has enough coins to upgrade
        if (CoinManager.currentCoinTotal <= upgradePriceCost) {
            UpgradeFailMessage();
            return;
        }

        // Increase work speed level
        shopUpgradePriceLevel++;

        int baseCoins = 8;
        float coinIncreasePercentage = 0.45f;

        float coins = baseCoins * Mathf.Pow(1 + coinIncreasePercentage, shopUpgradePriceLevel - 1);
        int newCoins = Mathf.RoundToInt(coins);

        //if (shopWorkSpeedLevel >= 20) {
        //    shopWorkSpeedLevel = 20;
        //    isMaxLevelPrice = true;

        //    SetWorkSpeedLevelTMPToMaxLevel();

        //    UpgradeSuccessMessage();  // Show success message
        //    CoinManager.RemoveGold((int)upgradePriceCost);  // Deduct coins

        //    // Update future customers with the new work speed
        //    GameObject.Find("GameManager").GetComponent<GameplayManager>().SetCurrentPricePerDrink(newCoins);

        //    return;
        //}

        UpgradeSuccessMessage();  // Show success message
        CoinManager.RemoveGold((int)upgradePriceCost);  // Deduct coins

        CustomerController[] allCustomers = UnityEngine.Object.FindObjectsByType<CustomerController>(FindObjectsSortMode.None);

        // Iterate through each customer and update their work speed
        foreach (CustomerController customer in allCustomers) {
            customer.CurrentPricePerDrink = newCoins;
        }

        // Update future customers with the new work speed
        GameObject.Find("GameManager").GetComponent<GameplayManager>().SetCurrentPricePerDrink(newCoins);
    }
}
