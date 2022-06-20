using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Settings : MonoBehaviour
{
    private Button applyButton;
    private Button cancelButton;
    private UI_Menu menu;
    private GameCameraController cameraController;
    private Slider scrollSpeedSlider;

    private void Awake()
    {
        FindButtons();
        FindSliders();
        menu = FindObjectOfType<UI_Menu>();
        cameraController = FindObjectOfType<GameCameraController>();
    }

    private void FindSliders()
    {
        Slider[] sliders = GetComponentsInChildren<Slider>();
        foreach (Slider slider in sliders)
        {
            if (slider.name == "ScrollSpeedSlider") scrollSpeedSlider = slider;
        }
    }

    private void FindButtons()
    {
        Button[] buttons = GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
        {
            if (button.name == "ApplyButton") applyButton = button;
            if (button.name == "CancelButton") cancelButton = button;
        }
    }

    private void Start()
    {
        applyButton.onClick.AddListener(ApplySettings);
        cancelButton.onClick.AddListener(GoBack);
        Show(false);
    }

    public void ToggleShowing()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    public void Show(bool isShowing = true)
    {
        if (isShowing && !gameObject.activeSelf)
        {
            ToggleShowing();

        }
        else if (!isShowing && gameObject.activeSelf)
        {
            ToggleShowing();

        }
    }

    private void GoBack()
    {
        Show(false);
        menu.Show();
    }

    private void ApplySettings()
    {
        cameraController.SetScrollSpeed(scrollSpeedSlider.value);

        GoBack();
    }

    public void ShowSettings(UI_Menu newPreviousMenu)
    {
        menu = newPreviousMenu;
        Show();

        if (cameraController != null)
        {
            if (!scrollSpeedSlider.gameObject.activeSelf) scrollSpeedSlider.gameObject.SetActive(true);
            scrollSpeedSlider.value = cameraController.ScrollSpeed;
        }
        else
        {
            scrollSpeedSlider.gameObject.SetActive(false);
        }
        
    }

    public bool IsShowing { get { return gameObject.activeSelf; } }
}
