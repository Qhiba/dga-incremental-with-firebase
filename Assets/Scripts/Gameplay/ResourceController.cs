using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceController : MonoBehaviour
{
    public Button resourceButton;
    public Image resourceImage;
    public Text resourceDescription;
    public Text resourceUpgradeCost;
    public Text resourceUnlockCost;

    public bool IsUnlocked { get; private set; }

    private ResourceConfig _config;

    private int _index;
    private int _level
    {
        get
        {
            //Mengecek apakah index sudah terdapat pada Progress Data
            if (!UserDataManager.HasResources(_index))
            {
                //Jika tidak maka tampilkan level 1
                return 1;
            }

            //Jika iya maka tampilkan berdasarkan Progress data
            return UserDataManager.progress.ResourceLevels[_index];
        }
        set
        {
            //Menyimpan value yang di set ke _level pada Progress Data
            UserDataManager.progress.ResourceLevels[_index] = value;
            UserDataManager.Save(true);
        }
    }

    private void Start()
    {
        resourceButton.onClick.AddListener(() =>
        {
            if (IsUnlocked)
            {
                UpgradeLevel();
            }
            else
            {
                UnlockResource();
            }
        });
    }

    public void SetConfig(int index, ResourceConfig config)
    {
        _index = index;
        _config = config;

        //ToString("0") berfungsi untuk membuang anga di belakang koma
        resourceDescription.text = $"{_config.name} Lv. {_level}\n+{GetOutput().ToString("0")}";
        resourceUnlockCost.text = $"Unlock Cost\n{_config.unlockCost}";
        resourceUpgradeCost.text = $"Upgrade Cost\n{GetUpgradeCost()}";

        SetUnlocked(_config.unlockCost == 0 || UserDataManager.HasResources(_index));
    }

    public double GetOutput()
    {
        return _config.output * _level;
    }

    public double GetUpgradeCost()
    {
        return _config.upgradeCost * _level;
    }

    public double GetUnlockCost()
    {
        return _config.unlockCost;
    }

    public void UpgradeLevel()
    {
        double upgradeCost = GetUpgradeCost();
        if (UserDataManager.progress.gold < upgradeCost)
        {
            return;
        }

        GameManager.Instance.AddGold(-upgradeCost);
        _level++;

        resourceUpgradeCost.text = $"Upgrade Cost\n{GetUpgradeCost()}";
        resourceDescription.text = $"{_config.name} Lv. {_level}\n+{GetOutput().ToString("0")}";
        AnalyticsManager.LogUpgradeEvent(_index, _level);
    }

    public void UnlockResource()
    {
        double unlockCost = GetUnlockCost();
        if (UserDataManager.progress.gold < unlockCost)
        {
            return;
        }

        SetUnlocked(true);
        GameManager.Instance.ShowNextResource();
        AchievementController.Instance.UnlockAchievement(AchievementType.UnlockResource, _config.name);
        AnalyticsManager.LogUnlockEvent(_index);
    }

    public void SetUnlocked(bool unlocked)
    {
        IsUnlocked = unlocked;
        if (unlocked)
        {
            //Jika resource baru di unlock dan belum ada di progress data, maka tambahkan data
            if (!UserDataManager.HasResources(_index))
            {
                UserDataManager.progress.ResourceLevels.Add(_level);
                UserDataManager.Save(true);
            }
        }

        resourceImage.color = IsUnlocked ? Color.white : Color.grey;
        resourceUnlockCost.gameObject.SetActive(!unlocked);
        resourceUpgradeCost.gameObject.SetActive(unlocked);
    }
}