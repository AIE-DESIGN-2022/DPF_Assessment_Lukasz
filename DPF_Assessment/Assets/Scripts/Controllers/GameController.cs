using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private int _playerNumber = 1;
    private List<Faction> _factions = new List<Faction>();
    private HUD_Manager _hudManager;
    private PlayerController _playerController;
    private GameCameraController _cameraController;

    private void Awake()
    {
        _hudManager = GetComponentInChildren<HUD_Manager>();
        _playerController = GetComponentInChildren<PlayerController>();
        _cameraController = GetComponentInChildren<GameCameraController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        BuildListOfFactions();
        _hudManager.SetPlayerFaction(GetPlayerFaction());
        _playerController.SetHUD_Manager(_hudManager);
    }

    private void SetupNewGame()
    {

    }

    private void BuildListOfFactions()
    {
        Faction[] _newFactions = FindObjectsOfType<Faction>();
        foreach (Faction _faction in _newFactions)
        {
            //Debug.Log(name + " found " + _faction.ToString() + " with FactionType " + _faction.FactionType().ToString());
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

    public PlayerController PlayerController() { return _playerController; }

    public GameCameraController CameraController() { return _cameraController; }
}
