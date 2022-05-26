using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Menu : MonoBehaviour
{
    GameController gameController;

    private Button saveButton;
    private Button loadButton;
    private Button settingsButton;
    private Button exitToMenuButton;
    private Button exitButton;


    private void Awake()
    {
        gameController = FindObjectOfType<GameController>();

        FindButtons();
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
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
        if (saveButton != null) saveButton.onClick.AddListener(SaveClicked);
        if (loadButton != null) loadButton.onClick.AddListener(LoadClicked);
        if (settingsButton != null) settingsButton.onClick.AddListener(SettingsClicked);
        if (exitToMenuButton != null) exitToMenuButton.onClick.AddListener(ExitToMenuClicked);
        if (exitButton != null) exitButton.onClick.AddListener(ExitClicked);
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

    }

    private void ExitClicked()
    {
        Application.Quit();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsShowing())
        {
            if (Input.GetKeyUp(KeyCode.Escape)) ToggleShowing();
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
            gameController.PlayerController().PlayerControl(true);
            gameController.CameraController().allowMovement = true;
        }
        else
        {
            gameObject.SetActive(true);
            gameController.PlayerController().PlayerControl(false);
            gameController.CameraController().allowMovement = false;
        }
    }

    public bool IsShowing()
    {
        return gameObject.activeSelf;
    }
}
