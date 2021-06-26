using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public enum PreExp1State {
    Initial,
    Step1,
    Step2,
    End
}

public enum PreExp1Input {
    ClickButton0,
    ClickButton1,
    WaitForSeconds
}

public class PreExperiment1 : TaskBasedManager<PreExp1State, PreExp1Input>
{
    protected override void GenerateTask() {
        // Gnerate UI
        GenerateUI("Initial UI", uiInfo[0]);
        GenerateUI("Step1 UI", uiInfo[1]);
        GenerateUI("End UI", uiInfo[2]);

        // Add task event
        AddTaskEvent(PreExp1Input.ClickButton0, UIBehaviour.Click, "Initial UI", "image_1", "button_0");
        AddTaskEvent(PreExp1Input.ClickButton1, UIBehaviour.Click, "Step1 UI", "image_1", "button_0");

        // Define task for pre-experiment 1
        task.AddStateStart(PreExp1State.Initial, () => EnableUI("Initial UI"))
        .AddTransition(PreExp1State.Initial, PreExp1State.Step1, PreExp1Input.ClickButton0, () => DisableUI("Initial UI"))
        .AddStateStart(PreExp1State.Step1, () => EnableUI("Step1 UI"), PrintStartTime)
        .AddTransition(PreExp1State.Step1, PreExp1State.Step2, PreExp1Input.ClickButton1, () => DisableUI("Step1 UI"), () => WaitForSeconds(10.0f))
        .AddTransition(PreExp1State.Step2, PreExp1State.End, PreExp1Input.WaitForSeconds)
        .AddStateStart(PreExp1State.End, () => EnableUI("End UI"), PrintEndTime);

        // Debug for task process
        task.OnEachInput((newInput) => { Debug.Log($"{newInput} call"); } );
        task.OnChange((fromState, toState) => { Debug.Log($"State {fromState} -> {toState}"); });
        task.OnEnter((fromState) => { Debug.Log($"State {fromState} begin"); });
        task.OnExit((fromState) => { Debug.Log($"State {fromState} ended"); });

        // start task
        task.Begin(PreExp1State.Initial);
    }

    public void WaitForSeconds(float time) {
        StartCoroutine(CallAfterSeconds(time));
    }

    public IEnumerator CallAfterSeconds(float time) {
        yield return new WaitForSeconds(time);

        task.Processing(PreExp1Input.WaitForSeconds);
    }

    public void PrintStartTime() {
        Debug.Log($"Start Time: {DateTime.Now.ToString()}");
    }

    public void PrintEndTime() {
        Debug.Log($"End Time: {DateTime.Now.ToString()}");
    }
}