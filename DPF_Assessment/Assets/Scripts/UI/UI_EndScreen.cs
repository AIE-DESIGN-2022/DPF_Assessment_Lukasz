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
            topText.text = "You Won!";
        }
        else
        {
            topText.text = "You Lost!";
        }
    }

    public void ShowStats()
    {
        Winner(statSystem.PlayerWon);
    }
}
