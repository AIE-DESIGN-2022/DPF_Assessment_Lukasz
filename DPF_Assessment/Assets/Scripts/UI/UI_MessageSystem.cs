using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_MessageSystem : MonoBehaviour
{
    private List<UnshowenMessage> unShownMessages = new List<UnshowenMessage>();
    private List<UI_Message> showingMessages = new List<UI_Message>();
    private UI_Message messagePrefab;

    public class UnshowenMessage
    {
        public string messageText;
        public float timeToLive;
        public float timeBeforeShow;
    }

    private void Awake()
    {
        messagePrefab = (UI_Message)Resources.Load<UI_Message>("HUD_Prefabs/UI_Message");
    }

    // Start is called before the first frame update
    void Start()
    {
        if (messagePrefab == null) Debug.LogError(name + " is missing UI_Message prefab");
    }

    // Update is called once per frame
    void Update()
    {
        ShowNewMessages();
        RemoveOldMessages();
    }

    private void ShowNewMessages()
    {
        if (unShownMessages.Count > 0)
        {
            foreach (UnshowenMessage unShownMessage in unShownMessages)
            {
                unShownMessage.timeBeforeShow -= Time.deltaTime;
            }

            UnshowenMessage[] messages = unShownMessages.ToArray();
            foreach (UnshowenMessage unseenMessage in messages)
            {
                if (unseenMessage.timeBeforeShow <= 0)
                {
                    unShownMessages.Remove(unseenMessage);
                    Show(unseenMessage.messageText, unseenMessage.timeToLive);
                }
            }
        }    
    }

    private void RemoveOldMessages()
    {
        if (showingMessages.Count > 0)
        {
            UI_Message[] messages = showingMessages.ToArray();
            foreach (UI_Message message in messages)
            {
                if (message.TimeToLive() <= 0)
                {
                    showingMessages.Remove(message);
                    Destroy(message.gameObject);
                }
            }
        }

        
    }

    public void ShowMessage(string messageText, float timeBeforeShow = 0f, float timeToLive = 20f)
    {
        
        if (messagePrefab != null)
        {
            if (timeBeforeShow == 0)
            {
                Show(messageText, timeToLive);
            }
            else
            {
                UnshowenMessage newMessage = new UnshowenMessage();
                newMessage.messageText = messageText;
                newMessage.timeToLive = timeToLive;
                newMessage.timeBeforeShow = timeBeforeShow;
                unShownMessages.Add(newMessage);
            }
        }
    }

    private void Show(string messageText, float timeToLive)
    {
        UI_Message newMessage = Instantiate(messagePrefab, transform);
        newMessage.Set(messageText, timeToLive);
        showingMessages.Add(newMessage);
    }
}
