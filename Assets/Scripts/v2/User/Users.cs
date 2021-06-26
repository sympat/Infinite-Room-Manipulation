using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Users : Transform2D
{
    public bool useVRMode;
    private User[] users;

    // private void Awake() {
    //     Initializing();
    // }

    public override void Initializing() {        
        users = GetComponentsInChildren<User>();  

        if(users.Length != 2) throw new System.Exception("there must be 2 'User' class in 'Users' class");

        foreach(var user in users) {
            // Debug.Log(user.gameObject);
            user.Initializing();
        }
        // Debug.Log("");

        if(useVRMode) users[1].gameObject.SetActive(false);
        else users[0].gameObject.SetActive(false);

        // this.gameObject.tag = "Users";
    }

    public User GetActiveUser() {
        foreach(var user in users) {
            if(user.gameObject.activeInHierarchy) {
                return user;
            }
        }

        throw new System.Exception("There are no tracked user");
    }


    // public void AddEnterEvent(string layer, string tag, UnityAction call) {
    //     foreach(var user in users)
    //         user.AddEnterEvent(layer, tag, call);
    // }

    // public void AddEnterEvent(string layer, string tag, UnityAction<Transform> call) {
    //     foreach(var user in users)
    //         user.AddEnterEvent(layer, tag, call);
    // }

    // public void RemoveEnterEvent(string layer, string tag, UnityAction call) {
    //     foreach(var user in users)
    //         user.RemoveEnterEvent(layer, tag, call);
    // }

    // public void RemoveEnterEvent(string layer, string tag, UnityAction<Transform> call) {
    //     foreach(var user in users)
    //         user.RemoveEnterEvent(layer, tag, call);
    // }

    // public void AddExitEvent(string layer, string tag, UnityAction call) {
    //     foreach(var user in users)
    //         user.AddExitEvent(layer, tag, call);
    // }

    // public void AddExitEvent(string layer, string tag, UnityAction<Transform> call) {
    //     foreach(var user in users)
    //         user.AddExitEvent(layer, tag, call);
    // }

    // public void RemoveExitEvent(string layer, string tag, UnityAction call) {
    //     foreach(var user in users)
    //         user.RemoveExitEvent(layer, tag, call);
    // }

    // public void RemoveExitEvent(string layer, string tag, UnityAction<Transform> call) {
    //     foreach(var user in users)
    //         user.RemoveExitEvent(layer, tag, call);
    // }

    // public void AddStayEvent(string layer, string tag, UnityAction<Transform> call) {
    //     foreach(var user in users)
    //         user.AddEnterEvent(layer, tag, call);
    // }

    // public void RemoveStayEvent(string layer, string tag, UnityAction<Transform> call) {
    //     foreach(var user in users)
    //         user.RemoveStayEvent(layer, tag, call);
    // }

    // public void AddCompletelyStayEvent(string layer, string tag, UnityAction call) {
    //     foreach(var user in users)
    //         user.AddCompletelyStayEvent(layer, tag, call);
    // }

    // public void RemoveCompletelyStayEvent(string layer, string tag, UnityAction call) {
    //     foreach(var user in users)
    //         user.RemoveCompletelyStayEvent(layer, tag, call);
    // }

    // public void AddViewEvent(string layer, string tag, UnityAction call) {
    //     foreach(var user in users)
    //         user.AddViewEvent(layer, tag, call);
    // }

    // public void RemoveViewEvent(string layer, string tag, UnityAction call) {
    //     foreach(var user in users)
    //         user.RemoveViewEvent(layer, tag, call);
    // }

    // public void AddClickEvent(string layer, string tag, UnityAction call) {
    //     foreach(var user in users)
    //         user.AddClickEvent(layer, tag, call);
    // }

    // public void RemoveClickEvent(string layer, string tag, UnityAction call) {
    //     foreach(var user in users)
    //         user.RemoveClickEvent(layer, tag, call);
    // }

    // public void AddReleaseEvent(string layer, string tag, UnityAction call) {
    //     foreach(var user in users)
    //         user.AddReleaseEvent(layer, tag, call);
    // }

    // public void RemoveReleaseEvent(string layer, string tag, UnityAction call) {
    //     foreach(var user in users)
    //         user.RemoveReleaseEvent(layer, tag, call);
    // }

    // public void AddRotateEvent(UnityAction call) {
    //     foreach(var user in users)
    //         user.AddRotateEvent(call);
    // }

    // public void RemoveRotateEvent(UnityAction call) {
    //     foreach(var user in users)
    //         user.RemoveRotateEvent(call);
    // }
}
