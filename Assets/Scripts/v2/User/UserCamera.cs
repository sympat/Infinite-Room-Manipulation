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

        bool bHit = Physics.Raycast(raycast, out hit, Mathf.Infinity, viewLayerMask);

        if(bHit) {
            UserEventArgs caller = new UserEventArgs(hit.transform);
            parentUser.CallEvent(caller, UserEventType.onView);
        }
    }

    
}
