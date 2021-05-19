using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoOneTurnResetter : MonoBehaviour
{
    protected float targetAngle;
    protected float ratio;

    public VirtualEnvironment virtualEnvironment;
    public Users users;

    // Start is called before the first frame update
    void Start()
    {
        targetAngle = 180;
        ratio = 2;

        // users.AddExitRealSpaceEvent(StartReset);
    }

    public void StartReset() {
        StartCoroutine(ApplyReset());
    }

    IEnumerator ApplyReset() {
        User user = users.GetActiveUser();

        float startRotation = user.body.Rotation;
        float virtualStartRotation = virtualEnvironment.Rotation;

        while(true) {
            if(Mathf.Abs(user.body.Rotation - startRotation) > 180f) break;
            virtualEnvironment.Rotate(-user.body.deltaRotation * Time.fixedDeltaTime);    

            yield return new WaitForFixedUpdate();
        }

        virtualEnvironment.Rotation = virtualStartRotation;
    }
}
