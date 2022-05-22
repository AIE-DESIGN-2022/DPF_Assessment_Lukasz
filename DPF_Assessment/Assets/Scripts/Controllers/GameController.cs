using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private int _playerNumber = 1;
    private FactionConfig[] _playableFactions;
    private List<Faction> _factions = new List<Faction>();
    private HUD_Manager _hudManager;

    private void Awake()
    {
        _hudManager = GetComponentInChildren<HUD_Manager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        BuildListOfFactions();
        _playableFactions = Resources.LoadAll<FactionConfig>("Factions/");
        _hudManager.SetPlayerFaction(GetPlayerFaction());
    }

    private void SetupNewGame()
    {

    }

    private void BuildListOfFactions()
    {
        Faction[] _newFactions = FindObjectsOfType<Faction>();
        foreach (Faction _faction in _newFactions)
        {
            _factions.Add(_faction);
            _faction.SetGameController(this);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void AddFaction()
    {
        GameObject _new = Instantiate(new GameObject(), transform);
        _new.AddComponent<Faction>();
        Faction _faction = _new.GetComponent<Faction>();
        //_faction.SetFaction();
    }

    public Faction GetFaction(int _playerNumber)
    {
        if (_factions.Count > 0)
        {
            foreach (Faction _faction in _factions)
            {
                if (_faction.PlayerNumber() == _playerNumber)
                {
                    return _faction;
                }
            }

            Debug.LogError("GameController canot not find faction with PlayerNumber =" + _playerNumber);
            return null;
        }
        else
        {
            Debug.LogError("GameController has no list of factions.");
            return null;
        }

        
    }

    public FactionConfig GetFactionConfig(Faction.EFaction _faction)
    {
        if (_playableFactions != null)
        {
            foreach (FactionConfig factionConfig in _playableFactions)
            {
                if (factionConfig.Faction() == _faction) return factionConfig;
            }
        }
        
        return null;
    }

    public Faction GetPlayerFaction()
    {
        return GetFaction(_playerNumber);
    }

    public HUD_Manager HUD_Manager()
    {
        if (_hudManager != null) return _hudManager;
        else return null;
    }

    public bool IsPlayerFaction(int _factionNumber)
    {
        return _playerNumber == _factionNumber;
    }
}
