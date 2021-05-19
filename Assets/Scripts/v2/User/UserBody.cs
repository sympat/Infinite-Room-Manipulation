using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserBody : Transform2D
{
    private float _deltaRotation;    
    private Vector2 _deltaPosition;
    private Vector2 _previousPosition;
    private float _previousRotation;
    private Vector2 _previousForward;

    public User parentUser {
        get { return transform.parent.GetComponent<User>(); }
    }

    public float deltaRotation {
        get { return _deltaRotation; }
    }

    public Vector2 deltaPosition {
        get { return _deltaPosition; }
    }

    private IEnumerator UpdateCurrentState()
    {
        while(true) {
            _deltaPosition = (this.Position - _previousPosition) / Time.fixedDeltaTime;
            _deltaRotation = Vector2.SignedAngle(_previousForward, this.Forward) / Time.fixedDeltaTime;

            _previousPosition = this.Position;
            _previousForward = this.Forward;

            yield return new WaitForFixedUpdate();
        }
    }

    private void ResetCurrentState()
    {
        _deltaPosition = Vector2.zero;
        _deltaRotation = 0;
        _previousPosition = this.Position;
        _previousForward = this.Forward;
    }

    private void OnEnable() {
        ResetCurrentState();
        StartCoroutine("UpdateCurrentState");
    }

    private void OnDisable() {
        StopAllCoroutines();
    }

    private void OnTriggerEnter(Collider other) {
        UserEventArgs caller = new UserEventArgs(other.transform);
        parentUser.CallEvent(caller, UserEventType.onEnter);
    }

    private void OnTriggerExit(Collider other) {
        UserEventArgs caller = new UserEventArgs(other.transform);
        parentUser.CallEvent(caller, UserEventType.onEnter);
    }
}
