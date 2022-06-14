using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_EndScreen : MonoBehaviour
{
    private UI_Menu menu;
    private StatSystem statSystem;
    private Text topText;

    private Text townCenterProduced;
    private Text towerProduced;
    private Text universityProduced;
    private Text barracksProduced;
    private Text farmsProduced;
    private Text totalBuildingsProduced;
    private Text healersProduced;
    private Text magicProduced;
    private Text rangedProduced;
    private Text meleeProduced;
    private Text workersProduced;
    private Text totalUnitsProduced;
    private Text totalProduced;

    private Text townCenterDestroyed;
    private Text towerDestroyed;
    private Text universityDestroyed;
    private Text barracksDestroyed;
    private Text farmsDestroyed;
    private Text totalBuildingsDestroyed;
    private Text healersKilled;
    private Text magicKilled;
    private Text rangedKilled;
    private Text meleeKilled;
    private Text workersKilled;
    private Text totalUnitsKilled;
    private Text totalKilled;


    private void Awake()
    {
        menu = GetComponentInChildren<UI_Menu>();
        statSystem = FindObjectOfType<StatSystem>();
        FindTextBoxes();
    }

    private void FindTextBoxes()
    {
        Text[] textBoxes = GetComponentsInChildren<Text>();
        foreach (Text box in textBoxes)
        {
            if (box.name == "TopText") topText = box;
            if (box.name == "TownCenterProduced") townCenterProduced = box;
            if (box.name == "TowerProduced") towerProduced = box;
            if (box.name == "UniversityProduced") universityProduced = box;
            if (box.name == "BarracksProduced") barracksProduced = box;
            if (box.name == "FarmsProduced") farmsProduced = box;
            if (box.name == "TotalBuildingsProduced") totalBuildingsProduced = box;
            if (box.name == "HealersProduced") healersProduced = box;
            if (box.name == "MagicProduced") magicProduced = box;
            if (box.name == "RangedProduced") rangedProduced = box;
            if (box.name == "MeleeProduced") meleeProduced = box;
            if (box.name == "WorkersProduced") workersProduced = box;
            if (box.name == "TotalUnitsProduced") totalUnitsProduced = box;
            if (box.name == "TotalProduced") totalProduced = box;
            if (box.name == "TownCenterDestroyed") townCenterDestroyed = box;
            if (box.name == "TowersDestroyed") towerDestroyed = box;
            if (box.name == "UniversityDestroyed") universityDestroyed = box;
            if (box.name == "BarracksDestroyed") barracksDestroyed = box;
            if (box.name == "FarmsDestroyed") farmsDestroyed = box;
            if (box.name == "TotalBuildingsDestroyed") totalBuildingsDestroyed = box;
            if (box.name == "HealersKilled") healersKilled = box;
            if (box.name == "MagicKilled") magicKilled = box;
            if (box.name == "RangedKilled") rangedKilled = box;
            if (box.name == "MeleeKilled") meleeKilled = box;
            if (box.name == "WorkersKilled") workersKilled = box;
            if (box.name == "TotalUnitKills") totalUnitsKilled = box;
            if (box.name == "TotalKills") totalKilled = box;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        menu.Show();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Winner(bool isWinner)
    {
        if (topText == null) return;

        if (isWinner)
        {
            topText.text = "Victory!";
        }
        else
        {
            topText.text = "Defeat!";
        }
    }

    public void ShowStats()
    {
        Winner(statSystem.PlayerWon);

        townCenterProduced.text = statSystem.GetStat("builtTownCenter").ToString();
        towerProduced.text = statSystem.GetStat("builtTower").ToString();
        universityProduced.text = statSystem.GetStat("builtUniversity").ToString();
        barracksProduced.text = statSystem.GetStat("builtBarraks").ToString();
        farmsProduced.text = statSystem.GetStat("builtFarm").ToString();

        int totalBuildingsProducedInt = statSystem.GetStat("builtTownCenter") + statSystem.GetStat("builtTower") + statSystem.GetStat("builtUniversity") + statSystem.GetStat("builtBarraks") + statSystem.GetStat("builtFarm");
        totalBuildingsProduced.text = totalBuildingsProducedInt.ToString();

        healersProduced.text = statSystem.GetStat("builtHealer").ToString();
        magicProduced.text = statSystem.GetStat("builtMagic").ToString();
        rangedProduced.text = statSystem.GetStat("builtRange").ToString();
        meleeProduced.text = statSystem.GetStat("builtMelee").ToString();
        workersProduced.text = statSystem.GetStat("builtWorker").ToString();

        int totalUnitsProducedInt = statSystem.GetStat("builtHealer") + statSystem.GetStat("builtMagic") + statSystem.GetStat("builtRange") + statSystem.GetStat("builtMelee") + statSystem.GetStat("builtWorker");
        totalUnitsProduced.text = totalUnitsProducedInt.ToString();

        totalProduced.text = (totalBuildingsProducedInt + totalUnitsProducedInt).ToString();

        townCenterDestroyed.text = statSystem.GetStat("killedTownCenter").ToString();
        towerDestroyed.text = statSystem.GetStat("killedTower").ToString();
        universityDestroyed.text = statSystem.GetStat("killedUniversity").ToString();
        barracksDestroyed.text = statSystem.GetStat("killedBarraks").ToString();
        farmsDestroyed.text = statSystem.GetStat("killedFarm").ToString();

        int totalBuildingsDestroyedInt = statSystem.GetStat("killedTownCenter") + statSystem.GetStat("killedTower") + statSystem.GetStat("killedUniversity") + statSystem.GetStat("killedBarraks") + statSystem.GetStat("killedFarm");
        totalBuildingsDestroyed.text = totalBuildingsDestroyedInt.ToString();

        healersKilled.text = statSystem.GetStat("killedHealer").ToString();
        magicKilled.text = statSystem.GetStat("killedMagic").ToString();
        rangedKilled.text = statSystem.GetStat("killedRange").ToString();
        meleeKilled.text = statSystem.GetStat("killedMelee").ToString();
        workersKilled.text = statSystem.GetStat("killedWorker").ToString();

        int totalUnitsKilledInt = statSystem.GetStat("killedHealer") + statSystem.GetStat("killedMagic") + statSystem.GetStat("killedRange") + statSystem.GetStat("killedMelee") + statSystem.GetStat("killedWorker");
        totalUnitsKilled.text = totalUnitsKilledInt.ToString();

        totalKilled.text = (totalBuildingsDestroyedInt + totalUnitsKilledInt).ToString();

    }
}
