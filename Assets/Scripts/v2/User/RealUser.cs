using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealUser : Transform2D
{
    public Users trackedUsers;

    private Vector2 offsetPosition;
    private float offsetRotation;

    public override void Initializing() {
        UserBody trackedUserBody = trackedUsers.GetActiveUser().body;

        offsetPosition = this.Position - trackedUserBody.Position;
        offsetRotation = this.Rotation - trackedUserBody.Rotation;

        this.gameObject.layer = LayerMask.NameToLayer("Player");
    }

    private void OnTriggerExit(Collider other) {
        if(other.gameObject.layer == LayerMask.NameToLayer("Real Space")) {
            User trackedUser = trackedUsers.GetActiveUser();
            UserEventArgs caller = new UserEventArgs(other.transform);
            trackedUser.CallEvent(caller, UserEventType.onExit);
        }
    }

    private void Update() {
        UserBody trackedUserBody = trackedUsers.GetActiveUser().body;


        this.Position = trackedUserBody.Position + offsetPosition;
        this.Rotation = trackedUserBody.Rotation + offsetRotation;
    }
}
