using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TestState {
    Initial,
    Idle,
    End
}

public enum TestInput {
    ExitRoom,
    ClickInitial
}

public class TestManager : TaskBasedManager<TestState, TestInput>
{
    protected override void GenerateTask() {
        // generate ui
        GenerateUI("Initial UI", uiInfo[0]);
        GenerateUI("End UI", uiInfo[1]);

        // add task event
        AddTaskEvent(TestInput.ExitRoom, Behaviour.Exit, "Room");
        AddTaskEvent(TestInput.ClickInitial, UIBehaviour.Click, "Initial UI", "image_1", "button_0");

        // create task
        task.AddStateStart(TestState.Initial, () => EnableUI("Initial UI"))
        .AddTransition(TestState.Initial, TestState.Idle, TestInput.ClickInitial, () => DisableUI("Initial UI"), () => EnableUI("End UI"));

        // Debug for task process
        task.OnEachInput((newInput) => { Debug.Log($"{newInput} call"); } );
        task.OnChange((fromState, toState) => { Debug.Log($"State {fromState} -> {toState}"); });
        task.OnEnter((fromState) => { Debug.Log($"State {fromState} begin"); });
        task.OnExit((fromState) => { Debug.Log($"State {fromState} ended"); });

        // start task
        task.Begin(TestState.Initial);
    }
}
