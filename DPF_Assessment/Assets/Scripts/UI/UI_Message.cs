using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Message : MonoBehaviour
{
    private Text textbox;
    private float timeToLive;


    private void Awake()
    {
        textbox = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        timeToLive -= Time.deltaTime;
    }

    public void Set(string newText, float newTimeToLive)
    {
        textbox.text = newText;
        timeToLive = newTimeToLive;
    }

    public float TimeToLive() { return timeToLive; }
}
