// Writen by Lukasz Dziedziczak
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
    private UI_Menu pauseMenu;

    [Header("New Game")]
    [SerializeField] private bool isNewGame = false;
    [SerializeField] List<Faction.EFaction> listOfPlayers;

    private void Awake()
    {
        _hudManager = GetComponentInChildren<HUD_Manager>();
        _playerController = GetComponentInChildren<PlayerController>();
        _cameraController = GetComponentInChildren<GameCameraController>();
        FindMenus();
    }

    private void FindMenus()
    {
        UI_Menu[] menus = GetComponentsInChildren<UI_Menu>();
        foreach (UI_Menu menu in menus)
        {
            if (menu.name == "PauseMenu") pauseMenu = menu;
        }
    }

    public UI_Menu PauseMenu()
    {
        if (pauseMenu != null) return pauseMenu;
        else
        {
            Debug.LogError(name + " cannot find pause menu.");
            return null;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (isNewGame) SetupNewGame();
        else SetupGame();
    }

    private void SetupNewGame()
    {
        SpawnPoint[] spawnPoints = FindObjectsOfType<SpawnPoint>();
        if (spawnPoints.Length != listOfPlayers.Count) Debug.LogError(name + " missmatch in number of players and number of spawn points");

        for (int i = 0; i < listOfPlayers.Count; i++)
        {
            GameObject newObject = Instantiate(new GameObject(), spawnPoints[i].transform.position, spawnPoints[i].transform.rotation);
            newObject.AddComponent<Faction>();
            Faction newFaction = newObject.GetComponent<Faction>();
            newFaction.SetupFaction(listOfPlayers[i], i + 1);

            newFaction.SpawnFirstBuilding(spawnPoints[i].transform);
        }

        BuildListOfFactions();
        _hudManager.SetPlayerFaction(GetPlayerFaction());
        _playerController.SetHUD_Manager(_hudManager);

        _cameraController.transform.position = _factions[0].transform.position;
    }

    private void SetupGame()
    {
        BuildListOfFactions();
        _hudManager.SetPlayerFaction(GetPlayerFaction());
        _playerController.SetHUD_Manager(_hudManager);
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
// Writen by Lukasz Dziedziczak