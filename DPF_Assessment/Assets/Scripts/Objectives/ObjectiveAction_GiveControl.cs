using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveAction_GiveControl : ObjectiveAction
{ 
    [SerializeField] List<Selectable> list = new List<Selectable>();
    [SerializeField] bool flashOnGiveControl = false;

    public override void DoAction()
    {
        base.DoAction();
        TurnControl();
    }

    private void TurnControl()
    {
        foreach (Selectable selectable in list)
        {
            gameController.GetPlayerFaction().TransferOwnership(selectable);
            if (flashOnGiveControl) selectable.FlashUntillSelected();
        }

        gameController.CameraController().PanCameraTo(list[0].transform.position);
    }
}
