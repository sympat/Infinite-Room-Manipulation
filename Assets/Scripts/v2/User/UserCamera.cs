using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserCamera : MonoBehaviour
{
    public LayerMask viewLayerMask;

    public User parentUser {
        get { return transform.parent.GetComponent<User>(); }
    }

    // Update is called once per frame
    private void Update()
    {        
        Ray raycast = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        bool bHit = Physics.Raycast(raycast, out hit, Mathf.Infinity);

        if(bHit) {

            UserEventArgs caller = new UserEventArgs(Behaviour.Watch, hit.transform.gameObject);

            // if(hit.transform.gameObject.layer == LayerMask.NameToLayer("Door")) {
            //     caller.target = UserTarget.Door;
            // }
            // else {
            //     caller.target = UserTarget.Any;
            // }

            parentUser.ProcessingEvent(caller);
            // parentUser.CallEvent(caller, UserEventType.onView);
        }
    }

    
}
