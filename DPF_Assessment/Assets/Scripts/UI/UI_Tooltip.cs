using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Tooltip : MonoBehaviour
{
    private Text topText;
    private Text bottomText;
    private RectTransform rectTransform;

    private void Awake()
    {
        FindTextBoxes();
        rectTransform = GetComponent<RectTransform>();
    }

    private void FindTextBoxes()
    {
        Text[] texts = GetComponentsInChildren<Text>();
        foreach (Text text in texts)
        {
            if (text.name == "TopText") topText = text;
            if (text.name == "BottomText") bottomText = text;
        }
    }

    private void Start()
    {
        if (IsShowing()) Show(false);
    }

    private void FixedUpdate()
    {
        if (IsShowing()) SetPosition();
    }

    public void SetPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.x -= rectTransform.sizeDelta.x;
        mousePosition.y += 0.1f;
        transform.position = mousePosition;
    }

    public void Show(bool isShowing = true)
    {
        if (!IsShowing() && isShowing) SetPosition();
        gameObject.SetActive(isShowing);
    }

    public bool IsShowing()
    {
        return gameObject.activeSelf;
    }

    public void SetText(string newTopText, string newBottomText)
    {
        topText.text = newTopText;
        bottomText.text = newBottomText;
    }
}
