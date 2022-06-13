using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatSystem : MonoBehaviour, ISavable
{
    private bool playerWon;
    private SavingSystem savingSystem;
    private GameController gameController;
    private UI_EndScreen endScreen;
    private Dictionary<string, int> stats;

    private void Awake()
    {
        savingSystem = GetComponentInChildren<SavingSystem>();
        gameController = GetComponent<GameController>();
        endScreen = FindObjectOfType<UI_EndScreen>(); 
    }

    private void Start()
    {
        if (gameController == null && endScreen != null)
        {
            RestoreState(savingSystem.EndGameLoad());
            endScreen.ShowStats();
        }
        else
        {
            BuildNewStatsDictionary();
        }
    }

    private void BuildNewStatsDictionary()
    {
        stats = new Dictionary<string, int>();

        stats.Add("builtWorker", 0);
        stats.Add("builtMelee", 0);
        stats.Add("builtRange", 0);
        stats.Add("builtMagic", 0);
        stats.Add("builtHealer", 0);
        stats.Add("builtFarm", 0);
        stats.Add("builtBarraks", 0);
        stats.Add("builtUniversity", 0);
        stats.Add("builtTower", 0);
        stats.Add("builtTownCenter",0);

        stats.Add("killedWorker", 0);
        stats.Add("killedMelee", 0);
        stats.Add("killedRange", 0);
        stats.Add("killedMagic", 0);
        stats.Add("killedHealer", 0);
        stats.Add("killedFarm", 0);
        stats.Add("killedBarraks", 0);
        stats.Add("killedUniversity", 0);
        stats.Add("killedTower", 0);
        stats.Add("killedTownCenter", 0);
    }

    public void SetPlayerWon(bool hasWon)
    {
        playerWon = hasWon;
    }

    public void EndGame(bool playerWon)
    {
        SetPlayerWon(playerWon);
        SaveStats();
    }

    public object CaptureState()
    {
        Dictionary<string, object> state = new Dictionary<string, object>();

        state.Add("playerWon", playerWon);
        state.Add("stats", stats);

        return state;
    }

    public void RestoreState(object state)
    {
        Dictionary<string, object> statSystemState = (Dictionary<string, object>)state;

        foreach (KeyValuePair<string, object> pair in statSystemState)
        {
            if (pair.Key == "playerWon") playerWon = (bool)pair.Value;
            if (pair.Key == "stats") stats = (Dictionary<string, int>)pair.Value;
        }
    }

    public void RestoreStats()
    {
        RestoreState(savingSystem.EndGameLoad());
    }
    
    public void SaveStats()
    {
        savingSystem.EndGameSave(CaptureState());
    }

    public bool PlayerWon { get { return playerWon; } }

    public void AddStatBuilt(Unit.EUnitType unitType)
    {
        switch(unitType)
        {
            case Unit.EUnitType.Worker:
                stats["builtWorker"]++;
                break;

            case Unit.EUnitType.Melee:
                stats["builtMelee"]++;
                break;

            case Unit.EUnitType.Ranged:
                stats["builtRange"]++;
                break;

            case Unit.EUnitType.Magic:
                stats["builtMagic"]++;
                break;

            case Unit.EUnitType.Healer:
                stats["builtHealer"]++;
                break;
        }
    }

    public void AddStatBuilt(Building.EBuildingType buildingType)
    {
        switch(buildingType)
        {
            case Building.EBuildingType.Farm:
                stats["builtFarm"]++;
                break;

            case Building.EBuildingType.Barraks:
                stats["builtBarraks"]++;
                break;

            case Building.EBuildingType.University:
                stats["builtUniversity"]++;
                break;

            case Building.EBuildingType.Tower:
                stats["builtTower"]++;
                break;

            case Building.EBuildingType.TownCenter:
                stats["builtTownCenter"]++;
                break;
        }
    }

    public void AddStatKilled(Unit.EUnitType unitType)
    {
        switch (unitType)
        {
            case Unit.EUnitType.Worker:
                stats["killedWorker"]++;
                break;

            case Unit.EUnitType.Melee:
                stats["killedMelee"]++;
                break;

            case Unit.EUnitType.Ranged:
                stats["killedRange"]++;
                break;

            case Unit.EUnitType.Magic:
                stats["killedMagic"]++;
                break;

            case Unit.EUnitType.Healer:
                stats["killedHealer"]++;
                break;
        }
    }

    public void AddStatKilled(Building.EBuildingType buildingType)
    {
        switch (buildingType)
        {
            case Building.EBuildingType.Farm:
                stats["killedFarm"]++;
                break;

            case Building.EBuildingType.Barraks:
                stats["killedBarraks"]++;
                break;

            case Building.EBuildingType.University:
                stats["killedUniversity"]++;
                break;

            case Building.EBuildingType.Tower:
                stats["killedTower"]++;
                break;

            case Building.EBuildingType.TownCenter:
                stats["killedTownCenter"]++;
                break;
        }
    }

    public int GetStat(string statName)
    {
        if (stats.ContainsKey(statName))
        {
            return stats[statName];
        }
        else return 0;
    }
}
