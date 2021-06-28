using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TeleportTaskState {
    Initial,
    Step1,
    End
}

public enum TeleportTaskInput {
    ClickButton0,
    ClickButton1,
    EnterPortal,
    CompleteTeleport,
    EnterRoom,
    CollectCoin
}


public class TeleportTask : TaskBasedManager<TeleportTaskState, TeleportTaskInput>
{
    public GameObject portalObjPrefab;
    private GameObject portalObj;
    private int portalCount = 0;

    protected override void GenerateTask() {
        // Generate UI
        GenerateUI("Initial UI", uiInfo[0]);
        GenerateUI("Teleport UI", uiInfo[1]);
        GenerateUI("End UI", uiInfo[2]);

        // Add task event
        AddTaskEvent(TeleportTaskInput.ClickButton0, UIBehaviour.Click, "Initial UI", "image_1", "button_0");
        AddTaskEvent(TeleportTaskInput.ClickButton1, UIBehaviour.Click, "Teleport UI", "image_1", "button_0");
        AddTaskEvent(TeleportTaskInput.EnterPortal, Behaviour.Enter, "Portal");

        // Define task for pre-experiment 2
        task.AddStateStart(TeleportTaskState.Initial, () => EnableUI("Initial UI"))
        .AddTransition(TeleportTaskState.Initial, TeleportTaskInput.ClickButton0, () => DisableUI("Initial UI"), () => EnableUI("Teleport UI"))
        .AddTransition(TeleportTaskState.Initial, TeleportTaskState.Step1, TeleportTaskInput.ClickButton1, () => DisableUI("Teleport UI"), CheckPortalDone, GeneratePortal)

        // .AddStateStart(TeleportTaskState.Step1, GeneratePortal)
        .AddTransition(TeleportTaskState.Step1, TeleportTaskInput.EnterPortal, DestroyPortal, GeneratePortal)
        .AddTransition(TeleportTaskState.Step1, TeleportTaskState.End, TeleportTaskInput.CompleteTeleport, DestroyPortal)

        .AddStateStart(TeleportTaskState.End, () => EnableUI("End UI"));

        // Debug for task process
        task.OnEachInput((newInput) => { Debug.Log($"{newInput} call"); } );
        task.OnChange((fromState, toState) => { Debug.Log($"State {fromState} -> {toState}"); });
        task.OnEnter((fromState) => { Debug.Log($"State {fromState} begin"); });
        task.OnExit((fromState) => { Debug.Log($"State {fromState} ended"); });

        // Start task 
        task.Begin(TeleportTaskState.Initial);

    }

    public virtual void CheckPortalDone() {
        StartCoroutine(_CheckPortalDone());
    }

    public IEnumerator _CheckPortalDone() {
        yield return new WaitUntil(() => (portalCount >= 5));
        
        task.Processing(TeleportTaskInput.CompleteTeleport);
    }

    public void GeneratePortal() {
        User user = users.GetActiveUser();
        Vector2 portalPos = user.Position;
        do {
            portalPos = virtualEnvironment.CurrentRoom.SamplingPosition(0.3f, Space.World);
        } while ((portalPos - user.Position).magnitude < 0.7f);

        portalObj = Instantiate(portalObjPrefab, virtualEnvironment.transform);
        portalObj.transform.position = Utility.CastVector2Dto3D(portalPos);
    }

    public void DestroyPortal() {
        if(portalObj != null) { 
            Destroy(portalObj);
            portalCount +=1;
        }
    }
}
