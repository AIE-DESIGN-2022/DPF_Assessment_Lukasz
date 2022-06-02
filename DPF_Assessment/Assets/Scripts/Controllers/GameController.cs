// Writen by Lukasz Dziedziczak
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [SerializeField] private int playerNumber = 1;
    private List<Faction> factions = new List<Faction>();
    private HUD_Manager hud_Manager;
    private PlayerController playerController;
    private GameCameraController cameraController;
    private UI_Menu pauseMenu;
    private Transform mapTransform;

    [Header("New Game")]
    [SerializeField] private bool isNewGame = false;
    [SerializeField] List<Faction.EFaction> listOfPlayers;
    [SerializeField] int startingFood = 0;
    [SerializeField] int startingWood = 0;
    [SerializeField] int startingGold = 0;

    public int treeCount = 0;

    private void Awake()
    {
        hud_Manager = GetComponentInChildren<HUD_Manager>();
        playerController = GetComponentInChildren<PlayerController>();
        cameraController = GetComponentInChildren<GameCameraController>();
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
            //GameObject newObject = Instantiate(new GameObject(), spawnPoints[i].transform.position, spawnPoints[i].transform.rotation);
            GameObject newObject = new GameObject();
            newObject.AddComponent<Faction>();
            Faction newFaction = newObject.GetComponent<Faction>();
            newFaction.SetupFaction(listOfPlayers[i], i + 1);
            newFaction.transform.position = spawnPoints[i].transform.position;
            newFaction.SpawnFirstBuilding(spawnPoints[i].GroundCoordinates());
            factions.Add(newFaction);
            newFaction.SetGameController(this);
            GiveStartingResources(newFaction);

            if (i == 0)
            {
                hud_Manager.SetPlayerFaction(newFaction);
                cameraController.transform.position = spawnPoints[i].GroundCoordinates();
            }
        }
        playerController.SetHUD_Manager(hud_Manager);
    }

    public void FactionDefated(Faction faction)
    {
        if (faction.PlayerNumber() != playerNumber)
        {
            factions.Remove(faction);
            Destroy(faction.gameObject);

            if (factions.Count == 1)
            {
                EndGame(true);
            }
        }
        else if (faction.PlayerNumber() == playerNumber)
        {
            EndGame(false);
        }
    }

    private void EndGame(bool playerWon)
    {
        SceneManager.LoadScene("EndScreen");
    }


    private void SetupGame()
    {
        BuildListOfFactions();
        hud_Manager.SetPlayerFaction(GetPlayerFaction());
        playerController.SetHUD_Manager(hud_Manager);
    }

    private void BuildListOfFactions()
    {
        Faction[] newFactions = FindObjectsOfType<Faction>();
        foreach (Faction faction in newFactions)
        {
            //Debug.Log(name + " found " + faction.ToString() + " with FactionType " + faction.FactionType().ToString());
            factions.Add(faction);
            faction.SetGameController(this);

        }

        if (factions.Count == 0 && !isNewGame) Debug.LogError(name + " found no factions at game start.");
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void AddFaction()
    {
        GameObject newFaction = Instantiate(new GameObject(), transform);
        newFaction.AddComponent<Faction>();
        Faction faction = newFaction.GetComponent<Faction>();
        //faction.SetFaction();
    }

    public Faction GetFaction(int playerNumber)
    {
        if (factions.Count > 0)
        {
            foreach (Faction faction in factions)
            {
                if (faction.PlayerNumber() == playerNumber)
                {
                    return faction;
                }
            }

            Debug.LogError("GameController canot not find faction with PlayerNumber = " + playerNumber);
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
        return GetFaction(playerNumber);
    }

    public HUD_Manager HUD_Manager()
    {
        if (hud_Manager != null) return hud_Manager;
        else return null;
    }

    public bool IsPlayerFaction(int factionNumber)
    {
        return playerNumber == factionNumber;
    }

    public PlayerController PlayerController() { return playerController; }

    public GameCameraController CameraController() { return cameraController; }

    private void GiveStartingResources(Faction faction)
    {
        faction.AddToStockpile(CollectableResource.EResourceType.Wood, startingWood);
        faction.AddToStockpile(CollectableResource.EResourceType.Food, startingFood);
        faction.AddToStockpile(CollectableResource.EResourceType.Gold, startingGold);
    }

    public Transform GetMapTransform()
    {
        if (mapTransform == null)
        {
            Transform[] transforms = FindObjectsOfType<Transform>();
            foreach (Transform t in transforms)
            {
                if (t.gameObject.name == "Map") mapTransform = t;
            }
        }

        return mapTransform;
    }

    public void CountTree()
    {
        treeCount++;
    }
}
// Writen by Lukasz Dziedziczak