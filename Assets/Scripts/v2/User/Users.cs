using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Users : Transform2D
{
    private User[] users;

    public override void Initializing() {
        users = GetComponentsInChildren<User>();  

        foreach(var user in users) {
            user.Initializing();
        }

        this.gameObject.tag = "Users";
    }

    public User GetActiveUser() {
        foreach(var user in users) {
            if(user.gameObject.activeInHierarchy) {
                return user;
            }
        }

        throw new System.Exception("There are no tracked user");
    }

    // public void AddEnterNewRoomEvent(UnityAction<Room> call) {
    //     foreach(var user in users) {
    //         user.AddEnterNewRoomEvent(call);
    //     }
    // }

    // public void AddExitRoomEvent(UnityAction<Room> call) {
    //     foreach(var user in users) {
    //         user.AddExitRoomEvent(call);
    //     }
    // }

    // public void AddExitRealSpaceEvent(UnityAction call) {
    //     foreach(var user in users) {
    //         user.AddExitRealSpaceEvent(call);
    //     }
    // }

    // public void AddEnterTargetEvent(UnityAction call) {
    //     foreach(var user in users) {
    //         user.AddEnterTargetEvent(call);
    //     }
    // }

    // public void AddDetachTargetEvent(UnityAction call) {
    //     foreach(var user in users) {
    //         user.AddDetachTargetEvent(call);
    //     }
    // }

    // public void AddClickEvents(UnityAction call) {
    //     foreach(var user in users) {
    //         user.AddClickEvents(call);
    //     }
    // }

    // public void AddClickOkEvent(UnityAction call) {
    //     foreach(var user in users) {
    //         user.AddClickOkEvent(call);
    //     }
    // }

    // public void RemoveClickOkEvent(UnityAction call) {
    //     foreach(var user in users) {
    //         user.RemoveClickOkEvent(call);
    //     }
    // }

    // public void AddClickYesEvent(UnityAction call) {
    //     foreach(var user in users) {
    //         user.AddClickYesEvent(call);
    //     }
    // }

    // public void RemoveClickYesEvent(UnityAction call) {
    //     foreach(var user in users) {
    //         user.RemoveClickYesEvent(call);
    //     }
    // }

    // public void AddClickNoEvent(UnityAction call) {
    //     foreach(var user in users) {
    //         user.AddClickNoEvent(call);
    //     }
    // }

    // public void RemoveClickNoEvent(UnityAction call) {
    //     foreach(var user in users) {
    //         user.RemoveClickNoEvent(call);
    //     }
    // }
}
