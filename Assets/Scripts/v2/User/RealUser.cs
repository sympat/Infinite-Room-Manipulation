using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealUser : Circle2D
{
    private Users trackedUsers;
    private Vector2 offsetPosition;
    private float offsetRotation;
    private bool isFirstEnter;
    
    public override void Initializing() {
        base.Initializing();

        // trackedUsers = this.transform.parent.parent.GetComponent<Manager>().users;
        // User trackedUser = trackedUsers.GetActiveUser();

        // offsetPosition = this.Position - trackedUser.Body.Position;
        // offsetRotation = this.Rotation - trackedUser.Body.Rotation;

        this.gameObject.layer = LayerMask.NameToLayer("RealUser");
    }

    private void Start() {
        trackedUsers = this.transform.parent.parent.GetComponent<Manager>().users;
        User trackedUser = trackedUsers.GetActiveUser();

        offsetPosition = this.Position - trackedUser.Body.Position;
        offsetRotation = this.Rotation - trackedUser.Body.Rotation;
    }

    private void OnTriggerEnter(Collider other) {
        if(other.gameObject.layer == LayerMask.NameToLayer("RealSpace")) {
            User user = trackedUsers.GetActiveUser();
            UserEventArgs caller = new UserEventArgs(Behaviour.Enter, other.gameObject);
            user.ProcessingEvent(caller);
            // trackedUser.CallEvent(caller, UserEventType.onExit);
        }
    }

    private void OnTriggerExit(Collider other) {
        if(other.gameObject.layer == LayerMask.NameToLayer("RealSpace")) {
            User user = trackedUsers.GetActiveUser();
            UserEventArgs caller = new UserEventArgs(Behaviour.Exit, other.gameObject);
            user.ProcessingEvent(caller);
            // trackedUser.CallEvent(caller, UserEventType.onExit);
            isFirstEnter = true;
        }
    }

    private void OnTriggerStay(Collider other) {
        if(other.gameObject.layer == LayerMask.NameToLayer("RealSpace")) {
            User user = trackedUsers.GetActiveUser();
            if(other.GetComponent<Bound2D>().IsInSide(this)) {
                if(isFirstEnter) {
                    UserEventArgs caller = new UserEventArgs(Behaviour.CompletelyEnter, other.gameObject);
                    user.ProcessingEvent(caller);
                    isFirstEnter = false;
                }
                else {
                    UserEventArgs caller = new UserEventArgs(Behaviour.CompletelyStay, other.gameObject);
                    user.ProcessingEvent(caller);
                }
            }
            else {
                UserEventArgs caller = new UserEventArgs(Behaviour.Stay, other.gameObject);
                user.ProcessingEvent(caller);
            }
        }
    }

    private void FixedUpdate() {
        User trackedUser = trackedUsers.GetActiveUser();

        this.Position = trackedUser.Body.Position + offsetPosition;
        this.Rotation = trackedUser.Body.Rotation + offsetRotation;
    }
}
