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

    public void AddEnterEvent(string layer, string tag, UnityAction call) {
        foreach(var user in users)
            user.AddEnterEvent(layer, tag, call);
    }

    public void RemoveEnterEvent(string layer, string tag, UnityAction call) {
        foreach(var user in users)
            user.RemoveEnterEvent(layer, tag, call);
    }

    public void AddExitEvent(string layer, string tag, UnityAction call) {
        foreach(var user in users)
            user.AddExitEvent(layer, tag, call);
    }

    public void RemoveExitEvent(string layer, string tag, UnityAction call) {
        foreach(var user in users)
            user.RemoveExitEvent(layer, tag, call);
    }

    public void AddViewEvent(string layer, string tag, UnityAction call) {
        foreach(var user in users)
            user.AddViewEvent(layer, tag, call);
    }

    public void RemoveViewEvent(string layer, string tag, UnityAction call) {
        foreach(var user in users)
            user.RemoveViewEvent(layer, tag, call);
    }

    public void AddClickEvent(string layer, string tag, UnityAction call) {
        foreach(var user in users)
            user.AddClickEvent(layer, tag, call);
    }

    public void RemoveClickEvent(string layer, string tag, UnityAction call) {
        foreach(var user in users)
            user.RemoveClickEvent(layer, tag, call);
    }
}
