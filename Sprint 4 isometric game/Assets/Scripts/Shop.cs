using System.Collections;
using System.Collections.Generic;
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

    private void Start()
    {
        openShopButton.onClick.AddListener(OpenShop);
        closeShopButton.onClick.AddListener(CloseShop);
        buildingButton1.onClick.AddListener(() => SelectBuilding(buildingPrefab1, buildingCost1));
        buildingButton2.onClick.AddListener(() => SelectBuilding(buildingPrefab2, buildingCost2));
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
}