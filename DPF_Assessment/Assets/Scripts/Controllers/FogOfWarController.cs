using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogOfWarController : MonoBehaviour
{
    private FogOfWar fogOfWar;
    private GameCameraController cameraController;
    private Faction playerFaction;

    private void Awake()
    {
        fogOfWar = FindObjectOfType<FogOfWar>();
        cameraController = FindObjectOfType<GameCameraController>();
    }


    // Start is called before the first frame update
    void Start()
    {
        fogOfWar.SetFogHeight(cameraController.CameraHeight() - 1f);
    }

    // Update is called once per frame
    void Update()
    {
        

        if (playerFaction != null && fogOfWar != null)
        {
            fogOfWar.ProcessVisibility(playerFaction.Selectables());
        }
        
    }

    public void SetPlayerFaction(Faction newFaction)
    {
        playerFaction = newFaction;
    }
}
