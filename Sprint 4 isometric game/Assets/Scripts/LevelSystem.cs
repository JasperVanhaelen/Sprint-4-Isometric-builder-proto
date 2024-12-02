using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSystem : MonoBehaviour
{
    private int XPNow;
    private int Level;
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
        UpdateUI();

        if (XPNow >= xpToNext)
        {
            Level++;
            EventManager.Instance.QueueEvent(new LevelChangedGameEvent(Level));
        }
    }

    private void OnLevelChanged(LevelChangedGameEvent info)
    {
        XPNow -= xpToNext;
        xpToNext = xpToNextLevel[info.NewLvl];
        lvlText.text = (info.NewLvl + 1).ToString();
        UpdateUI();

        GameObject window = Instantiate(lvlWindowPrefab, GameManager.current.canvas.transform);
        window.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => Destroy(window));

        foreach (var reward in lvlReward[info.NewLvl])
        {
            EventManager.Instance.QueueEvent(new CurrencyChangeGameEvent(reward, CurrencyType.Coins));
        }
    }
}