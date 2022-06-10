using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Objective : MonoBehaviour
{
    private Text mainText;
    private Text subText;

    private void Awake()
    {
        FindTextFields();
    }

    private void FindTextFields()
    {
        Text[] textFields = GetComponentsInChildren<Text>();
        foreach (Text textField in textFields)
        {
            if (textField.name == "MainText") mainText = textField;
            if (textField.name == "SubText") subText = textField;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Set(string newMainText, string newSubText)
    {
        mainText.text = newMainText;
        subText.text = newSubText;
    }
}
