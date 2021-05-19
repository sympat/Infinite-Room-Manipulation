using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Experiement1State {
    public static Experiment1 experiment1;
    public virtual void onDefault() {}
    public virtual void onClickOK() {}
    public virtual void onClickYes() {}
    public virtual void onClickNo() {}
    public virtual void onEnter() {}
    public virtual void onExit() {}
    public virtual void onDetach() {}
    public virtual void onView() {}
}

public class InitialState : Experiement1State {

    public InitialState(Experiment1 currentExperiment) {
        experiment1 = currentExperiment;
    }

    public override void onDefault()
    {
        experiment1.EnableOKUI(experiment1.initialText);
    }

    public override void onClickOK()
    {
        experiment1.DisableUIandPointer();
        experiment1.currentState = new WatchAroundState();
    }
}

public class WatchAroundState : Experiement1State {
    public override void onDefault() {
        experiment1.EnableOKUI(experiment1.watchAroundText);
    }

    public override void onClickOK()
    {
        experiment1.DisableUIandPointer();
        CoroutineManager.Instance.CallWaitForSeconds(10.0f, () => experiment1.currentState = new TargetState());
    }

}


 public class TargetState : Experiement1State {
    public override void onDefault()
    {
        if(experiment1.AreGainQueuesEmpty())
            experiment1.InitializeAppliedGain();
        
        if(experiment1.IsDistanceQueueEmpty())
            experiment1.InitializeDistance();

        experiment1.EnableOKUI(experiment1.targetText);
        // CoroutineManager.Instance.CallWaitForSeconds(6.0f, () => experiment1.EnableOKUI(experiment1.targetText));
    }
    
    public override void onClickOK()
    {
        experiment1.DisableUIandPointer();
        experiment1.GenerateTarget();
    }

    public override void onEnter()
    {
        experiment1.DestroyTarget();
        experiment1.currentState = new DoorState();
    }
}

public class DoorState : Experiement1State {

    public override void onDefault()
    {
        Debug.Log("DoorState");
        experiment1.EnableOKUI(experiment1.viewDoorText);
    }

    public override void onClickOK()
    {
        experiment1.DisableUIandPointer();
        experiment1.ColoringFacingDoor();
    }

    public override void onView()
    {
        Debug.Log("onView");
        experiment1.currentState = new BehindState();
    }
}

public class BehindState : Experiement1State {

    public override void onDefault()
    {
        Debug.Log("BehindState");
        CoroutineManager.Instance.CallWaitForSeconds(3.0f, ApplyWallMove);
    }
    
    public override void onClickOK()
    {
        experiment1.DisableUIandPointer();
        experiment1.currentState = new SelectionState();
    }

    public void ApplyWallMove() {
        experiment1.MoveOppositeWall();
        experiment1.CleanDoors();
        experiment1.EnableOKUI(experiment1.turnBehindText);
    }
}

public class SelectionState : Experiement1State {
    public override void onDefault()
    {
        CoroutineManager.Instance.CallWaitForSeconds(3.0f, () => experiment1.EnableSelectionUI(experiment1.selectionText));
    }

    public override void onClickYes()
    {
        experiment1.WriteAnswer(true);
        experiment1.DisableUIandPointer();
        experiment1.currentState = new GoToCenterState();
    }

    public override void onClickNo()
    {
        experiment1.WriteAnswer(false);
        experiment1.DisableUIandPointer();
        experiment1.currentState = new GoToCenterState();
    }
}

public class GoToCenterState : Experiement1State {

    public override void onDefault()
    {
        experiment1.EnableOKUI(experiment1.goToCenterText);
    }

    public override void onClickOK() {
        experiment1.DisableUIandPointer();
        experiment1.GenerateCenterPoint();
    }

    public override void onEnter()
    {
        experiment1.DestroyCenterPoint();
        experiment1.UserCameraFadeOut();
        CoroutineManager.Instance.CallWaitForSeconds(3.0f, ResetSpace);

        // if(experiment1.IsEndExperiment())
        //     experiment1.currentState = new EndState();
        // else
        //     experiment1.currentState = new WatchAroundState();
    }

    public void ResetSpace() {
        // experiment1.ResetSpace();

        // if(experiment1.IsEndExperiment())
        //     experiment1.currentState = new EndState();
        // else
        //     experiment1.currentState = new WatchAroundState();
    }
}

public class EndState : Experiement1State {
    public override void onDefault()
    {
        experiment1.PrintResult();
        experiment1.EnableEndUI(experiment1.endText);
    }
}






