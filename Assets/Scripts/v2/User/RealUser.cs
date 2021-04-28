using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealUser : Transform2D
{
    public User virtualUser;

    public override void Initializing() {
        this.gameObject.layer = LayerMask.NameToLayer("Player");
    }

    private void Awake() {
        Initializing();
    }

    private void OnTriggerExit(Collider other) {
        if(other.gameObject.layer == LayerMask.NameToLayer("Real Space")) {
            UserBody.UserEventArgs caller = new UserBody.UserEventArgs(other.transform);
            virtualUser.GetTrackedUserBody().OnExitTrigger(caller);
        }
    }
}
