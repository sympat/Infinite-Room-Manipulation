using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    protected VirtualEnvironment virtualEnvironment;
    protected User user;
    protected RealSpace realSpace;

    // Start is called before the first frame update
    public virtual void Start()
    {
        foreach(Transform child in transform) {
            Transform2D tf = child.GetComponent<Transform2D>();
            if(tf != null) tf.Initializing();

            if(tf is VirtualEnvironment)
                virtualEnvironment = tf as VirtualEnvironment;
            else if(tf is User)
                user = tf as User;
            else if(tf is RealSpace)
                realSpace = tf as RealSpace;
        }
    }

}
