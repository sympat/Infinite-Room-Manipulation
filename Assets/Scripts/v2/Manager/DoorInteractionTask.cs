using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DoorInteractState {
    Initial,
    Step1,
    Step2,
    Step3,
    Step4,
    End
}

public enum DoorInteractInput {
    ClickButton0,
    ClickButton1,
    ClickButton2,
    OpenDoor,
    EnterRoom,
}

public class DoorInteractionTask : TaskBasedManager<DoorInteractState, DoorInteractInput>
{
    protected override void GenerateTask() {
        // Generate UI
        GenerateUI("Initial UI", uiInfo[0]);
        GenerateUI("Knob Interaction UI", uiInfo[1]);
        GenerateUI("Door Interaction UI", uiInfo[2]);
        GenerateUI("End UI", uiInfo[3]);

        // Add task event
        AddTaskEvent(DoorInteractInput.ClickButton0, UIBehaviour.Click, "Initial UI", "image_1", "button_0");
        AddTaskEvent(DoorInteractInput.ClickButton1, UIBehaviour.Click, "Knob Interaction UI", "image_1", "button_0");
        AddTaskEvent(DoorInteractInput.ClickButton2, UIBehaviour.Click, "Door Interaction UI", "image_1", "button_0");
        AddTaskEvent(DoorInteractInput.OpenDoor, Behaviour.Open, "Door");
        AddTaskEvent(DoorInteractInput.EnterRoom, Behaviour.CompletelyEnter, "NextRoom");

        // Define task for pre-experiment 2
        task.AddStateStart(DoorInteractState.Initial, () => EnableUI("Initial UI"))
        .AddTransition(DoorInteractState.Initial, DoorInteractState.Step1, DoorInteractInput.ClickButton0, () => DisableUI("Initial UI"))

        .AddStateStart(DoorInteractState.Step1, () => EnableUI("Knob Interaction UI"))
        .AddTransition(DoorInteractState.Step1,  DoorInteractInput.ClickButton1, () => DisableUI("Knob Interaction UI"))
        .AddTransition(DoorInteractState.Step1, DoorInteractState.Step2, DoorInteractInput.OpenDoor)

        .AddStateStart(DoorInteractState.Step2, () => EnableUI("Door Interaction UI"))
        .AddTransition(DoorInteractState.Step2, DoorInteractInput.ClickButton2, () => DisableUI("Door Interaction UI"))
        .AddTransition(DoorInteractState.Step2, DoorInteractState.Step3, DoorInteractInput.EnterRoom)

        .AddStateStart(DoorInteractState.Step3, () => EnableUI("Knob Interaction UI"))
        .AddTransition(DoorInteractState.Step3,  DoorInteractInput.ClickButton1, () => DisableUI("Knob Interaction UI"))
        .AddTransition(DoorInteractState.Step3, DoorInteractState.Step4, DoorInteractInput.OpenDoor)

        .AddStateStart(DoorInteractState.Step4, () => EnableUI("Door Interaction UI"))
        .AddTransition(DoorInteractState.Step4, DoorInteractInput.ClickButton2, () => DisableUI("Door Interaction UI"))
        .AddTransition(DoorInteractState.Step4, DoorInteractState.End, DoorInteractInput.EnterRoom)

        .AddStateStart(DoorInteractState.End, () => EnableUI("End UI"));

        // Debug for task process
        task.OnEachInput((newInput) => { Debug.Log($"{newInput} call"); } );
        task.OnChange((fromState, toState) => { Debug.Log($"State {fromState} -> {toState}"); });
        task.OnEnter((fromState) => { Debug.Log($"State {fromState} begin"); });
        task.OnExit((fromState) => { Debug.Log($"State {fromState} ended"); });

        // Start task 
        task.Begin(DoorInteractState.Initial);
    }
}
