using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ResetTaskState {
    Initial,
    GoToPortal1,
    GoToPortal2,
    End
}

public enum ResetTaskInput {
    ClickButton0,
    ClickButton1,
    EnterPortal,
}

public class ResetTask : TaskBasedManager<ResetTaskState, ResetTaskInput>
{
    public GameObject portalObjPrefab;
    private GameObject portalObj;

    protected override void GenerateTask() {
        GenerateUI("Initial UI", uiInfo[0]);
        GenerateUI("Step1 UI", uiInfo[1]);
        GenerateUI("End UI", uiInfo[2]);

        AddTaskEvent(ResetTaskInput.ClickButton0, UIBehaviour.Click, "Initial UI", "image_1", "button_0");
        AddTaskEvent(ResetTaskInput.ClickButton1, UIBehaviour.Click, "Step1 UI", "image_1", "button_0");
        AddTaskEvent(ResetTaskInput.EnterPortal, Behaviour.Enter, "Portal");

        task.AddStateStart(ResetTaskState.Initial, () => EnableUI("Initial UI"))
        .AddTransition(ResetTaskState.Initial, ResetTaskState.GoToPortal1, ResetTaskInput.ClickButton0, () => DisableUI("Initial UI"))

        .AddStateStart(ResetTaskState.GoToPortal1, () => GeneratePortal(new Vector2(0, 6)))
        .AddTransition(ResetTaskState.GoToPortal1, ResetTaskState.GoToPortal2, ResetTaskInput.EnterPortal, DestroyPortal)

        .AddStateStart(ResetTaskState.GoToPortal2, () => EnableUI("Step1 UI"), () => GeneratePortal(new Vector2(0, 0)))
        .AddTransition(ResetTaskState.GoToPortal2, ResetTaskInput.ClickButton1, () => DisableUI("Step1 UI"))
        .AddTransition(ResetTaskState.GoToPortal2, ResetTaskState.End, ResetTaskInput.EnterPortal, DestroyPortal)

        .AddStateStart(ResetTaskState.End, () => EnableUI("End UI"));

        task.Begin(ResetTaskState.Initial);

        // Debug for task process
        // task.OnEachInput((newInput) => { Debug.Log($"{newInput} call"); } );
        // task.OnChange((fromState, toState) => { Debug.Log($"State {fromState} -> {toState}"); });
        // task.OnEnter((fromState) => { Debug.Log($"State {fromState} begin"); });
        // task.OnExit((fromState) => { Debug.Log($"State {fromState} ended"); });

        // Start task 
        task.Begin(ResetTaskState.Initial);

    }

    public void GeneratePortal(Vector2 localPos) {
        User user = users.GetActiveUser();

        portalObj = Instantiate(portalObjPrefab, virtualEnvironment.transform);
        portalObj.transform.localPosition = Utility.CastVector2Dto3D(localPos);
    }

    public void DestroyPortal() {
        if(portalObj != null) { 
            Destroy(portalObj);
        }
    }
}
