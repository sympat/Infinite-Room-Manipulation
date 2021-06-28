using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InteractState {
    Initial,
    Step1,
    Step2,
    Step3,
    Step4,
    CoinStep1,
    CoinStep2,
    End
}

public enum InteractInput {
    ClickButton0,
    ClickButton1,
    ClickButton2,
    // ClickButton3,
    CollectCoin,
    OpenDoor,
    EnterRoom,
}

public class InteractionTask : TaskBasedManager<InteractState, InteractInput>
{
    public GameObject coinObjPrefab;
    private GameObject coinObj;

    protected override void GenerateTask() {
        // Generate UI
        GenerateUI("Initial UI", uiInfo[0]);
        GenerateUI("Coin Interaction UI", uiInfo[1]);
        GenerateUI("Knob Interaction UI", uiInfo[2]);
        GenerateUI("Door Interaction UI", uiInfo[3]);
        GenerateUI("End UI", uiInfo[4]);

        // Add task event
        AddTaskEvent(InteractInput.ClickButton0, UIBehaviour.Click, "Initial UI", "image_1", "button_0");
        AddTaskEvent(InteractInput.ClickButton1, UIBehaviour.Click, "Coin Interaction UI", "image_1", "button_0");
        AddTaskEvent(InteractInput.ClickButton2, UIBehaviour.Click, "Knob Interaction UI", "image_1", "button_0");
        // AddTaskEvent(InteractInput.ClickButton3, UIBehaviour.Click, "Door Interaction UI", "image_1", "button_0");
        AddTaskEvent(InteractInput.OpenDoor, Behaviour.Open, "Door");
        AddTaskEvent(InteractInput.EnterRoom, Behaviour.CompletelyEnter, "NextRoom");
        AddTaskEvent(InteractInput.CollectCoin, Behaviour.Release, "Coin");

        // Define task for pre-experiment 2
        task.AddStateStart(InteractState.Initial, () => EnableUI("Initial UI"))
        .AddTransition(InteractState.Initial, InteractState.CoinStep1, InteractInput.ClickButton0, () => DisableUI("Initial UI"))

        .AddStateStart(InteractState.CoinStep1, () => EnableUI("Coin Interaction UI"), () => ToggleDoors(false))
        .AddTransition(InteractState.CoinStep1, InteractInput.ClickButton1, () => DisableUI("Coin Interaction UI"), GenerateCoin)
        .AddTransition(InteractState.CoinStep1, InteractState.Step1, InteractInput.CollectCoin, DestroyCoin)

        .AddStateStart(InteractState.Step1, () => EnableUI("Knob Interaction UI"))
        .AddTransition(InteractState.Step1,  InteractInput.ClickButton2, () => DisableUI("Knob Interaction UI"), () => ToggleDoors(true))
        .AddTransition(InteractState.Step1, InteractState.Step2, InteractInput.OpenDoor)

        .AddStateStart(InteractState.Step2, () => EnableUI("Door Interaction UI"))
        // .AddTransition(InteractState.Step2, DoorInteractInput.ClickButton3, () => DisableUI("Door Interaction UI"))
        .AddTransition(InteractState.Step2, InteractState.CoinStep2, InteractInput.EnterRoom, () => DisableUI("Door Interaction UI"))

        .AddStateStart(InteractState.CoinStep2, () => EnableUI("Coin Interaction UI"), () => ToggleDoors(false))
        .AddTransition(InteractState.CoinStep2, InteractInput.ClickButton1, () => DisableUI("Coin Interaction UI"), GenerateCoin)
        .AddTransition(InteractState.CoinStep2, InteractState.Step3, InteractInput.CollectCoin, DestroyCoin)

        .AddStateStart(InteractState.Step3, () => EnableUI("Knob Interaction UI"))
        .AddTransition(InteractState.Step3,  InteractInput.ClickButton2, () => DisableUI("Knob Interaction UI"), () => ToggleDoors(true))
        .AddTransition(InteractState.Step3, InteractState.Step4, InteractInput.OpenDoor)

        .AddStateStart(InteractState.Step4, () => EnableUI("Door Interaction UI"))
        // .AddTransition(InteractState.Step4, DoorInteractInput.ClickButton3, () => DisableUI("Door Interaction UI"))
        .AddTransition(InteractState.Step4, InteractState.End, InteractInput.EnterRoom, () => DisableUI("Door Interaction UI"))

        .AddStateStart(InteractState.End, () => EnableUI("End UI"));

        // Debug for task process
        task.OnEachInput((newInput) => { Debug.Log($"{newInput} call"); } );
        task.OnChange((fromState, toState) => { Debug.Log($"State {fromState} -> {toState}"); });
        task.OnEnter((fromState) => { Debug.Log($"State {fromState} begin"); });
        task.OnExit((fromState) => { Debug.Log($"State {fromState} ended"); });

        // Start task 
        task.Begin(InteractState.Initial);
    }

    public void GenerateCoin() {
        User user = users.GetActiveUser();
        Vector2 coinPos = user.Body.Position;
        do {
            coinPos = virtualEnvironment.CurrentRoom.SamplingPosition(0.3f, Space.World);
        } while ((coinPos - user.Body.Position).magnitude < 0.3f);

        coinObj = Instantiate(coinObjPrefab, virtualEnvironment.transform);
        coinObj.transform.position = Utility.CastVector2Dto3D(coinPos, 1.2f);
    }

    public void ToggleDoors(bool enabled) {
        virtualEnvironment.ToggleConnectedDoors(virtualEnvironment.CurrentRoom, enabled);
    }

    public void DestroyCoin() {
        AudioSource.PlayClipAtPoint(SoundSetting.Instance.coinCollectSound, coinObj.transform.position);
        Destroy(coinObj);
    }
}
