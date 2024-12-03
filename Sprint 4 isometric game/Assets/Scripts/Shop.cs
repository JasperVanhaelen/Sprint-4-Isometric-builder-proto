using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    public GameObject shopPanel;
    public Button openShopButton;
    public Button closeShopButton;
    public Button buildingButton1;
    public Button buildingButton2;

    public GameObject buildingPrefab1;
    public GameObject buildingPrefab2;

    public BuildingPlacer buildingPlacer;

    public int buildingCost1 = 100;
    public int buildingCost2 = 150;

    private LevelSystem levelSystem; // Reference to the LevelSystem
    private int requiredLevelForBuilding2 = 2; // Level required to unlock building 2

    private void Start()
    {
        // Get the LevelSystem component (assumes it's attached to the same GameObject or is a singleton)
        levelSystem = FindObjectOfType<LevelSystem>();
        if (levelSystem == null)
        {
            Debug.LogError("LevelSystem not found!");
            return;
        }

        // Subscribe to shop button events
        openShopButton.onClick.AddListener(OpenShop);
        closeShopButton.onClick.AddListener(CloseShop);
        buildingButton1.onClick.AddListener(() => SelectBuilding(buildingPrefab1, buildingCost1));
        buildingButton2.onClick.AddListener(() => SelectBuilding(buildingPrefab2, buildingCost2));

        // Initialize button states
        UpdateBuildingButtonStates();

        // Listen for level changes to dynamically update button states
        EventManager.Instance.AddListener<LevelChangedGameEvent>(OnLevelChanged);
    }

    private void OpenShop()
    {
        shopPanel.SetActive(true);
    }

    private void CloseShop()
    {
        shopPanel.SetActive(false);
    }

    private void SelectBuilding(GameObject buildingPrefab, int buildingCost)
    {
        if (CurrencySystem.CurrencyAmounts[CurrencyType.Coins] >= buildingCost)
        {
            // Deduct cost
            EventManager.Instance.QueueEvent(new CurrencyChangeGameEvent(-buildingCost, CurrencyType.Coins));

            // Start dragging the building
            buildingPlacer.StartDragging(buildingPrefab);

            Debug.Log("Bought " + CurrencySystem.CurrencyAmounts[CurrencyType.Coins]);
        }
        else
        {
            Debug.Log("Not enough coins!");
        }
    }

    private void UpdateBuildingButtonStates()
    {
        // Enable/disable buttons based on the player's current level
        buildingButton1.interactable = true; // Always available
        buildingButton2.interactable = levelSystem.Level >= requiredLevelForBuilding2;
    }

    private void OnLevelChanged(LevelChangedGameEvent info)
    {
        UpdateBuildingButtonStates(); // Update button states when the level changes
    }
}