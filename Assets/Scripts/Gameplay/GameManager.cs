using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance = null;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
            }

            return _instance;
        }
    }

    //Fungsi [Range(min, max)] ialah menjaga value agar tetap berada diantara min dan max-nya
    [Range(0f, 1f)]
    public float autoCollectPercentage = 0.1f;
    public ResourceConfig[] resourceConfigs;
    public Sprite[] resourcesSprites;

    public Transform resourceParent;
    public ResourceController resourcePrefab;
    public TapText tapTextPrefab;

    public Transform coinIcon;
    public Text goldInfo;
    public Text autoCollectInfo;

    private List<ResourceController> _activeResources = new List<ResourceController>();
    private List<TapText> _tapTextPool = new List<TapText>();
    private float _collectSecond;

    // Start is called before the first frame update
    void Start()
    {
        AddAllResources();

        goldInfo.text = $"Gold: {UserDataManager.progress.gold.ToString("0")}";
    }

    // Update is called once per frame
    void Update()
    {
        //Fungsi untuk selalu mengeksekusi CollectPerSecond setiap detik
        _collectSecond += Time.unscaledDeltaTime;
        if (_collectSecond >= 1f)
        {
            CollectPerSecond();
            _collectSecond = 0f;
        }

        CheckResourceCost();

        coinIcon.transform.localScale = Vector3.LerpUnclamped(coinIcon.transform.localScale, Vector3.one * 2f, 0.15f);
        coinIcon.transform.Rotate(0f, 0f, Time.deltaTime * -100f);
    }

    private void AddAllResources()
    {
        bool showResources = true;
        int index = 0;
        foreach (ResourceConfig config in resourceConfigs)
        {
            GameObject obj = Instantiate(resourcePrefab.gameObject, resourceParent, false);
            ResourceController resource = obj.gameObject.GetComponent<ResourceController>();

            resource.SetConfig(index, config);
            obj.gameObject.SetActive(showResources);

            if (showResources && !resource.IsUnlocked)
            {
                showResources = false;
            }

            _activeResources.Add(resource);
            index++;
        }
    }

    public void ShowNextResource()
    {
        foreach (ResourceController resource in _activeResources)
        {
            if (!resource.gameObject.activeSelf)
            {
                resource.gameObject.SetActive(true);
                break;
            }
        }
    }

    private void CheckResourceCost()
    {
        foreach (ResourceController resource in _activeResources)
        {
            bool isBuyable = false;
            if (resource.IsUnlocked)
            {
                isBuyable = UserDataManager.progress.gold >= resource.GetUpgradeCost();
            }
            else
            {
                isBuyable = UserDataManager.progress.gold >= resource.GetUnlockCost();
            }

            resource.resourceImage.sprite = resourcesSprites[isBuyable ? 1 : 0];
        }
    }

    private void CollectPerSecond()
    {
        double output = 0;
        foreach (ResourceController resource in _activeResources)
        {
            if (resource.IsUnlocked)
            {
                output += resource.GetOutput();
            }
        }

        output *= autoCollectPercentage;

        //Fungsi ToString("F1") ialah membualatkan angka menjadi desimal yang memiliki 1 angka di belakang koma
        autoCollectInfo.text = $"Auto Collect: {output.ToString("F1")} / second";

        AddGold(output);
    }

    public void AddGold(double value)
    {
        UserDataManager.progress.gold += value;
        goldInfo.text = $"Gold: {UserDataManager.progress.gold.ToString("0")}";

        UserDataManager.Save();
    }

    public void CollectByTap(Vector3 tapPosition, Transform parent)
    {
        double output = 0;
        foreach (ResourceController resource in _activeResources)
        {
            if (resource.IsUnlocked)
            {
                output += resource.GetOutput();
            }
        }

        TapText tapText = GetOrCreateTapText();
        tapText.transform.SetParent(parent, false);
        tapText.transform.position = tapPosition;

        tapText.showedText.text = $"+{output.ToString("0")}";
        tapText.gameObject.SetActive(true);
        coinIcon.transform.localScale = Vector3.one * 1.75f;

        AddGold(output);
    }

    private TapText GetOrCreateTapText()
    {
        TapText tapText = _tapTextPool.Find(t => !t.gameObject.activeSelf);
        if (tapText == null)
        {
            tapText = Instantiate(tapTextPrefab).GetComponent<TapText>();
            _tapTextPool.Add(tapText);
        }

        return tapText;
    }

    public void ClearGameData()
    {
        UserDataManager.ClearData();
    }
}

//Fungsi System.Serializeable adalah agar object bisa di-serialize dan value dapat di-set dari inspector
[System.Serializable]
public struct ResourceConfig
{
    public string name;
    public double unlockCost;
    public double upgradeCost;
    public double output;
}
