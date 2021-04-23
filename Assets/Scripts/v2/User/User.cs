using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class User : Transform2D
{
    public GameObject[] trackedUserObjects;
    private UserBody[] trackedUserBodys;
    
    public override void Initializing() {
        trackedUserBodys = new UserBody[trackedUserObjects.Length];

        for(int i=0; i<trackedUserObjects.Length; i++) {
            trackedUserBodys[i] = trackedUserObjects[i].GetComponent<UserBody>();
            trackedUserBodys[i].Initializing();
        }

        this.gameObject.layer = LayerMask.NameToLayer("Player");
    }

    // private void Update() {
    //     GetTrackedUserBody();
    // }

    public UserBody GetTrackedUserBody() {
        foreach(var user in trackedUserBodys) {
            if(user.gameObject.activeInHierarchy) {
                return user;
            }
        }

        throw new System.Exception("There are no tracked user");
    }
}
