using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreExperiment1 : Manager
{
    [TextArea]
    public string initialText, endText;

    private FiniteStateMachine<string, string> task;

    public override void Awake() {
        base.Awake();

        // Add User event as input for task
        User user = users.GetActiveUser();
        user.AddClickEvent("UI", "OKButton", () => task.Processing("onClickOK"));

        // Define task for pre-experiment 1
        task = new FiniteStateMachine<string, string>("Initial", "Idle", "End");
        task.AddStateStart("Initial", () => EnableOKUI(initialText))
        .AddTransition("Initial", "Idle", "onClickOK", DisableUIandPointer, () => WakeAfterSeconds(60.0f))
        .AddTransition("Idle", "End", "onAfterSeconds")
        .AddStateStart("End", () => EnableEndUI(endText));

        // Debug for task process
        task.OnEachInput((newInput) => { Debug.Log($"{newInput} call"); } );
        task.OnChange((fromState, toState) => { Debug.Log($"State {fromState} -> {toState}"); });
        task.OnEnter((fromState) => { Debug.Log($"State {fromState} begin"); });
        task.OnExit((fromState) => { Debug.Log($"State {fromState} ended"); });

        // start task
        task.Begin("Initial");
    }

    public void WakeAfterSeconds(float time) {
        CoroutineManager.Instance.CallWaitForSeconds(time, () => task.Processing("onAfterSeconds"));
    }

    public void DisableUIandPointer() {
        User user = users.GetActiveUser();

        user.ui.DisableUI();
        if(user.pointer != null) user.pointer.HidePointer(); 
    }

    public void EnableEndUI(string paragraphText) {
        User user = users.GetActiveUser();

        user.ui.PopUpParagraph(paragraphText); 
    }

    public void EnableOKUI(string paragraphText) {
        User user = users.GetActiveUser();

        user.ui.PopUpParagraph(paragraphText); 
        user.ui.PopUpOkButton();
        if(user.pointer != null) user.pointer.ShowPointer(); 
    }
}
