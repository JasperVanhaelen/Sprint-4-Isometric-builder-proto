using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CurrencySystem : MonoBehaviour
{
    public static Dictionary<CurrencyType, int> CurrencyAmounts = new Dictionary<CurrencyType, int>();

    [SerializeField]
    private List<GameObject> texts;

    private Dictionary<CurrencyType, TextMeshProUGUI> currencyTexts = new Dictionary<CurrencyType, TextMeshProUGUI>();

    private const string CoinsKey = "Coins"; // Key for saving coins

    private void Awake()
    {

        for (int i = 0; i < texts.Count; i++)
        {
            CurrencyType currencyType = (CurrencyType)i;

            // Load saved coin value or start with 200 as default
            int startingAmount = (currencyType == CurrencyType.Coins) ? PlayerPrefs.GetInt(CoinsKey, 200) : 0;
            CurrencyAmounts.Add(currencyType, startingAmount);

            // Assign the corresponding TextMeshProUGUI
            TextMeshProUGUI currencyText = texts[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            currencyTexts.Add(currencyType, currencyText);

            // Update UI with starting amount
            currencyText.text = startingAmount.ToString();
        }
    }

    private void Start()
    {
        EventManager.Instance.AddListener<CurrencyChangeGameEvent>(OnCurrencyChange);
        EventManager.Instance.AddListener<NotEnoughCurrencyGameEvent>(OnNotEnough);
    }

    private void OnCurrencyChange(CurrencyChangeGameEvent info)
    {
        Debug.Log($"Processing {info.amount} for {info.currencyType}");

        // Update the currency amount
        CurrencyAmounts[info.currencyType] += info.amount;

        // Log the updated amount
        Debug.Log($"New {info.currencyType} amount: {CurrencyAmounts[info.currencyType]}");

        // Update the UI
        currencyTexts[info.currencyType].text = CurrencyAmounts[info.currencyType].ToString();

        // Save the updated coin amount
        if (info.currencyType == CurrencyType.Coins)
            SaveCoins();
    }

    private void OnNotEnough(NotEnoughCurrencyGameEvent info)
    {
        Debug.Log($"You don't have enough of {info.amount} {info.currencyType}");
    }

    public void AddTestCoins()
    {
        int testAmount = 100;

        // Update the coins and trigger the UI update
        CurrencyAmounts[CurrencyType.Coins] += testAmount;
        currencyTexts[CurrencyType.Coins].text = CurrencyAmounts[CurrencyType.Coins].ToString();

        // Save the updated coins
        SaveCoins();

        Debug.Log($"Added {testAmount} coins for testing. Total now: {CurrencyAmounts[CurrencyType.Coins]}");
    }

    private void SaveCoins()
    {
        PlayerPrefs.SetInt(CoinsKey, CurrencyAmounts[CurrencyType.Coins]);
        PlayerPrefs.Save();
    }
}


public enum CurrencyType
{
    Coins,
    //CrystalMeth //#Waltuh
}