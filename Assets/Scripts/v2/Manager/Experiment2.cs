using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LocomotionType { Manipulation, Teleportation, S2C };

public class Experiment2 : Manager
{
    public UIHandler userUI;
    public CustomLaserPointer userPointer;

    // Start is called before the first frame update
    public override void Awake()
    {
        base.Awake();

        // UserBody userBody = user.GetTrackedUserBody();

        // userBody.AddClickEvent(userUI.DisableUI, 0);
        // userBody.AddClickEvent(userPointer.HidePointer, 0);

        // // userPointer.ShowPointer();
        // userUI.PopUpOkParagraph();

        // virtualEnvironment.MoveWallWithLimit(virtualEnvironment.CurrentRoom, 3, -3.0f);
        // virtualEnvironment.MoveWallWithLimit(virtualEnvironment.CurrentRoom, 2);

        // virtualEnvironment.MoveWall(virtualEnvironment.CurrentRoom, 3, -1f);
    }

}
