using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Menu : MonoBehaviour
{
    GameController gameController;

    private Button saveButton;
    private Button loadButton;
    private Button settingsButton;
    private Button exitToMenuButton;
    private Button exitButton;
    private Button singlePlayerButton;
    private Button multiplayerButton;
    private Button backButton;
    private Button newCampaignButton;
    private Button newSkirmishButton;

    private UI_Menu singePlayerMenu;
    private UI_Menu mainMenu;


    private void Awake()
    {
        gameController = FindObjectOfType<GameController>();

        FindButtons();
        LoadOtherMenus();
    }

    private void LoadOtherMenus()
    {
        UI_Menu[] menus = FindObjectsOfType<UI_Menu>();
        foreach (UI_Menu menu in menus)
        {
            if (menu.name == "SinglePlayerMenu") singePlayerMenu = menu;
            if (menu.name == "MainMenu") mainMenu = menu;
        }
    }

    private void FindButtons()
    {
        Button[] childButtons = GetComponentsInChildren<Button>();
        foreach (Button button in childButtons)
        {
            if (button.name == "SaveButton") saveButton = button;
            if (button.name == "LoadButton") loadButton = button;
            if (button.name == "SettingsButton") settingsButton = button;
            if (button.name == "ExitToMenuButton") exitToMenuButton = button;
            if (button.name == "ExitButton") exitButton = button;
            if (button.name == "SinglePlayerButton") singlePlayerButton = button;
            if (button.name == "BackButton") backButton = button;
            if (button.name == "NewCampaignButton") newCampaignButton = button;
            if (button.name == "NewSkirmishButton") newSkirmishButton = button;
            if (button.name == "MultiplayerButton") multiplayerButton = button;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (gameObject.name != "MainMenu") gameObject.SetActive(false);

        if (saveButton != null) saveButton.onClick.AddListener(SaveClicked);
        if (loadButton != null) loadButton.onClick.AddListener(LoadClicked);
        if (settingsButton != null) settingsButton.onClick.AddListener(SettingsClicked);
        if (exitToMenuButton != null) exitToMenuButton.onClick.AddListener(ExitToMenuClicked);
        if (exitButton != null) exitButton.onClick.AddListener(ExitClicked);
        if (singlePlayerButton != null) singlePlayerButton.onClick.AddListener(SingePlayerClicked);
        if (backButton != null) backButton.onClick.AddListener(BackClicked);
        if (newCampaignButton != null) newCampaignButton.onClick.AddListener(NewCampaignClicked);
        if (newSkirmishButton != null) newSkirmishButton.onClick.AddListener(NewSkirmishClicked);

        // deactive buttons for unfinished features;
        if (multiplayerButton != null) multiplayerButton.interactable = false;
        if (saveButton != null) saveButton.interactable = false;
        if (loadButton != null) loadButton.interactable = false;
        if (settingsButton != null) settingsButton.interactable = false;

    }

    private void SaveClicked()
    {
        
    }

    private void LoadClicked()
    {

    }

    private void SettingsClicked()
    {

    }

    private void ExitToMenuClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void ExitClicked()
    {
        Application.Quit();
    }

    private void SingePlayerClicked()
    {
        ToggleShowing();
        singePlayerMenu.ToggleShowing();
    }

    private void BackClicked()
    {
        ToggleShowing();
        mainMenu.ToggleShowing();
    }

    private void NewCampaignClicked()
    {
        SceneManager.LoadScene("SampleScene");
    }

    private void NewSkirmishClicked()
    {
        SceneManager.LoadScene("SkirmishMap");
    }

    // Update is called once per frame
    void Update()
    {
        if (IsShowing())
        {
            if (Input.GetKeyDown(KeyCode.Escape)) ToggleShowing();
        }
    }

    public void Show(bool isShowing = true)
    {
        gameObject.SetActive(isShowing);
    }

    public void ToggleShowing()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
            if (gameController != null)
            {
                gameController.PlayerController().PlayerControl(true);
                gameController.CameraController().allowMovement = true;
            }
            
        }
        else
        {
            gameObject.SetActive(true);
            if (gameController != null)
            {
                gameController.PlayerController().PlayerControl(false);
                gameController.CameraController().allowMovement = false;
            }
        }
    }

    public bool IsShowing()
    {
        return gameObject.activeSelf;
    }
}
