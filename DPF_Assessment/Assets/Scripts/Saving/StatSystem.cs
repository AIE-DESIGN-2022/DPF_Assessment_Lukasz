using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatSystem : MonoBehaviour, ISavable
{
    private bool playerWon;
    private SavingSystem savingSystem;
    private GameController gameController;
    private UI_EndScreen endScreen;

    private void Awake()
    {
        savingSystem = GetComponentInChildren<SavingSystem>();
        gameController = GetComponent<GameController>();
        endScreen = FindObjectOfType<UI_EndScreen>(); 
    }

    private void Start()
    {
        if (gameController == null && endScreen != null)
        {
            RestoreState(savingSystem.EndGameLoad());
            endScreen.ShowStats();
        }
    }

    public void SetPlayerWon(bool hasWon)
    {
        playerWon = hasWon;
    }

    public void EndGame(bool playerWon)
    {
        SetPlayerWon(playerWon);
        SaveStats();
    }

    public object CaptureState()
    {
        Dictionary<string, object> state = new Dictionary<string, object>();

        state.Add("playerWon", playerWon);

        return state;
    }

    public void RestoreState(object state)
    {
        Dictionary<string, object> stats = (Dictionary<string, object>)state;

        foreach (KeyValuePair<string, object> pair in stats)
        {
            if (pair.Key == "playerWon") playerWon = (bool)pair.Value;
        }
    }

    public void RestoreStats()
    {
        RestoreState(savingSystem.EndGameLoad());
    }
    
    public void SaveStats()
    {
        savingSystem.EndGameSave(CaptureState());
    }

    public bool PlayerWon { get { return playerWon; } }
}
