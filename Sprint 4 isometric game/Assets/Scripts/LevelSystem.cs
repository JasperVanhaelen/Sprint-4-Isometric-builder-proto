using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSystem : MonoBehaviour
{
    private int XPNow;
    public int Level = 1; // Start at level 1
    private int xpToNext;

    [SerializeField]
    private GameObject levelPanel;

    [SerializeField]
    private GameObject lvlWindowPrefab;

    private Slider slider;
    private TextMeshProUGUI xpText;
    private TextMeshProUGUI lvlText;
    private Image starImage;

    private static bool initialized;
    private static Dictionary<int, int> xpToNextLevel = new();
    private static Dictionary<int, int[]> lvlReward = new();

    private void Awake()
    {
    
        slider = levelPanel.GetComponentInChildren<Slider>();
        xpText = levelPanel.transform.Find("XP text").GetComponent<TextMeshProUGUI>();

        var starTransform = levelPanel.transform.Find("Star");
        if (starTransform != null)
        {
            starImage = starTransform.Find("Star Image").GetComponent<Image>();
            lvlText = starImage?.transform.Find("Level Text").GetComponent<TextMeshProUGUI>();
        }

        if (!initialized)
        {
            Initialize();
        }

        xpToNextLevel.TryGetValue(Level, out xpToNext);
        if (xpToNext == 0)
        {
            Debug.LogError($"XP to next level not found for Level {Level}. Defaulting to 100.");
            xpToNext = 100; // Fallback value
        }
    }

    private static void Initialize()
    {
        try
        {
            string path = "LevelsXP";
            TextAsset textAsset = Resources.Load<TextAsset>(path);
            if (textAsset == null)
            {
                Debug.LogError($"CSV file not found at path: {path}");
                return;
            }

            string[] lines = textAsset.text.Split('\n');

            for (int i = 1; i < lines.Length - 1; i++)
            {
                string[] columns = lines[i].Split(';');
                if (columns.Length < 4)
                {
                    Debug.LogWarning($"Skipping malformed row {i}: {lines[i]}");
                    continue;
                }

                if (int.TryParse(columns[0], out int lvl) && 
                    int.TryParse(columns[1], out int xp) && 
                    int.TryParse(columns[2], out int curr1) && 
                    int.TryParse(columns[3], out int curr2))
                {
                    if (!xpToNextLevel.ContainsKey(lvl))
                    {
                        xpToNextLevel[lvl] = xp;
                        lvlReward[lvl] = new[] { curr1, curr2 };
                    }
                }
            }

            initialized = true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error initializing LevelSystem: {ex.Message}");
        }
    }

    private void Start()
    {
        EventManager.Instance.AddListener<XPAddedGameEvent>(OnXPAdded);
        EventManager.Instance.AddListener<LevelChangedGameEvent>(OnLevelChanged);
        Debug.Log("Subscribing OnLevelChanged to LevelChangedGameEvent");
        UpdateUI();
    }

    private void UpdateUI()
    {
        slider.value = (float)XPNow / xpToNext;
        xpText.text = $"{XPNow}/{xpToNext}";
    }

    private void OnXPAdded(XPAddedGameEvent info)
    {
        XPNow += info.amount;

        while (XPNow >= xpToNext) // Loop to handle multiple level-ups
        {
            Level++; // Increment level
            XPNow -= xpToNext; // Subtract current level's XP requirement

            if (xpToNextLevel.TryGetValue(Level, out xpToNext))
            {
                EventManager.Instance.QueueEvent(new LevelChangedGameEvent(Level));
                Debug.Log($"Levelled up to {Level}. New xpToNext: {xpToNext}, XPNow: {XPNow}");
            }
            else
            {
                Debug.LogError($"XP requirements for level {Level} not found! Stopping level-up.");
                xpToNext = int.MaxValue; // Prevent infinite leveling
                break;
            }
        }

        UpdateUI(); // Update the UI after XP and level changes
    }

    private void OnLevelChanged(LevelChangedGameEvent info)
    {
        // Update the level display text (Level starts at 1 in UI)
        lvlText.text = info.NewLvl.ToString();

        // Create the level-up window
        if (lvlWindowPrefab != null)
        {
            GameObject window = Instantiate(lvlWindowPrefab, GameManager.current.canvas.transform);

            // Safely find the close button
            Button closeButton = window.GetComponentInChildren<Button>();
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(() => Destroy(window));
            }
            else
            {
                Debug.LogError("Level-up window prefab is missing a button!");
            }
        }

        // Distribute rewards for the new level
        if (lvlReward.TryGetValue(info.NewLvl, out int[] rewards))
        {
            foreach (var reward in rewards)
            {
                Debug.Log($"Processing reward: {reward} for Coins at level {info.NewLvl}");
                EventManager.Instance.QueueEvent(new CurrencyChangeGameEvent(reward, CurrencyType.Coins));
            }
        }
        else
        {
            Debug.LogWarning($"No rewards defined for level {info.NewLvl}!");
        }
    }
}