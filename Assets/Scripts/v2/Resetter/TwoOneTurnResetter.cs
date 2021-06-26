using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TTResetState {
    Idle,
    Rotating,
    Translating,
}

public enum TTResetInput {
    ExitRealSpace,
    EnterRealSpace,
    UserRotationDone
}

public class TwoOneTurnResetter : TaskBasedManager<TTResetState, TTResetInput>
{
    protected float targetAngle;
    protected float ratio;

    private IEnumerator coroutine1, coroutine2;

    private GainRedirector redirector;

    protected override void GenerateTask()
    {
        targetAngle = 180;
        ratio = 2;
        redirector = this.GetComponent<GainRedirector>();

        GenerateUI("Rotation UI", uiInfo[0]);
        GenerateUI("Translation UI", uiInfo[1]);
        GenerateUI("Translation Guide UI", uiInfo[2]);

        AddTaskEvent(TTResetInput.ExitRealSpace, Behaviour.Exit, "RealSpace");
        AddTaskEvent(TTResetInput.EnterRealSpace, Behaviour.CompletelyEnter, "RealSpace");
        AddTaskEvent(TTResetInput.UserRotationDone, Behaviour.Rotate, "Default");

        task.AddStateStart(TTResetState.Idle)
        .AddTransition(TTResetState.Idle, TTResetState.Rotating, TTResetInput.ExitRealSpace, () => ToggleRedirector(false))
        .AddStateStart(TTResetState.Rotating, StartRotation, () => EnableUI("Rotation UI"), () => CallAfterRotation(180f))
        .AddTransition(TTResetState.Rotating, TTResetState.Translating, TTResetInput.UserRotationDone, StopRotation, () => DisableUI("Rotation UI"))
        .AddStateStart(TTResetState.Translating, StartTranslation, () => EnableUI("Translation UI"), () => EnableUI("Translation Guide UI", true))
        .AddTransition(TTResetState.Translating, TTResetState.Idle, TTResetInput.EnterRealSpace, StopTranslation, () => DisableUI("Translation UI"), () => DisableUI("Translation Guide UI", true), () => ToggleRedirector(true));

        // Debug for task process
        task.OnEachInput((newInput) => { Debug.Log($"{newInput} call"); } );
        task.OnChange((fromState, toState) => { Debug.Log($"State {fromState} -> {toState}"); });
        task.OnEnter((fromState) => { Debug.Log($"State {fromState} begin"); });
        task.OnExit((fromState) => { Debug.Log($"State {fromState} ended"); });

        task.Begin(TTResetState.Idle);
    }

    public void ToggleRedirector(bool enabled) {
        if(redirector != null) redirector.enabled = enabled;
    }

    public void CallAfterRotation(float degree) {
        User user = users.GetActiveUser();
        StartCoroutine(user.CallAfterRotation(degree));
    }

    public void StartRotation() {
        StartCoroutine(coroutine1 = _ApplyRotation());
    }

    public void StopRotation() {
        StopCoroutine(coroutine1);
    }

    public void StartTranslation() {
        StartCoroutine(coroutine2 = _ApplyTranslation());
    }

    public void StopTranslation() {
        StopCoroutine(coroutine2);
    }

    IEnumerator _ApplyRotation() {
        // Debug.Log("_ApplyRotation");
        User user = users.GetActiveUser();

        while(true) {
            virtualEnvironment.RotateAround(user.Body.Position, user.Body.deltaRotation * Time.fixedDeltaTime);    
            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator _ApplyTranslation() {
        // Debug.Log("_ApplyTranslation");
        User user = users.GetActiveUser();
        UICanvas flagUI = UIManager.Instance.GetUI("Translation Guide UI");

        while(true) {
            virtualEnvironment.Translate(user.Body.deltaPosition * Time.fixedDeltaTime, Space.World);
            flagUI.Translate(user.Body.deltaPosition * Time.fixedDeltaTime, Space.World);
            yield return new WaitForFixedUpdate();
        }
    }

}
