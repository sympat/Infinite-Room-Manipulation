using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public enum GainType { Translation = 0, Rotation = 1, Curvature = 2, Undefined = -1 };

[Serializable]
public class GainRedirector : MonoBehaviour
{
    // curvature gain g_c = 1 / r = theta = pi / 180 * degree
    // therefore, degree = (1 / r) * (180 / pi) and radius = 1 / g_c
    // if you want to detail, see "IEEE TRANSACTIONS ON VISUALIZATION AND COMPUTER GRAPHICS, VOL. 19, NO. 4, APRIL 2013 Comparing Four Approaches to Generalized Redirected Walking: Simulation and Live User Data"
    [HideInInspector]
    public const float MIN_ROTATION_GAIN = -0.33f; // -0.33f
    [HideInInspector]
    public const float MAX_ROTATION_GAIN = 0.24f;
    [HideInInspector]
    public const float MIN_CURVATURE_GAIN = -0.045f; // turn radius : 22m
    [HideInInspector]
    public const float MAX_CURVATURE_GAIN = 0.045f;
    [HideInInspector]
    public const float HODGSON_MIN_CURVATURE_GAIN = -0.133f; // turn radius : 7.5m
    [HideInInspector]
    public const float HODGSON_MAX_CURVATURE_GAIN = 0.133f;
    [HideInInspector]
    public const float MIN_TRANSLATION_GAIN = -0.14f;
    [HideInInspector]
    public const float MAX_TRANSLATION_GAIN = 0.26f;
    protected const float MOVEMENT_THRESHOLD = 0.2f; // meters per second, determine whether this user is moving or not
    protected const float ROTATION_THRESHOLD = 5.0f; // degrees per second, determine whether this user is rotating or not

    protected float translationGain;
    protected float rotationGain;
    protected float curvatureGain;
    protected User user;

    public Users users;
    public VirtualEnvironment virtualEnvironment;

    private void Start() {
        StartCoroutine(ApplyGain());
    }

    public virtual (GainType, float) ApplyRedirection() {
        float degree = 0;
        GainType type = GainType.Undefined;

        if (user.body.deltaPosition.magnitude > MOVEMENT_THRESHOLD && user.body.deltaPosition.magnitude >= Mathf.Abs(user.body.deltaRotation)) // Translation
        {
            degree = user.body.deltaPosition.magnitude * (MAX_TRANSLATION_GAIN);
            type = GainType.Translation;
        }
        // if(user.body.deltaPosition.magnitude > 0.2f && user.body.deltaPosition.magnitude >= Mathf.Abs(user.body.deltaRotation)) // Curvature
        // {
        //     degree = Mathf.Rad2Deg * user.body.deltaPosition.magnitude * (HODGSON_MAX_CURVATURE_GAIN);
        //     type = GainType.Curvature;
        // }
        else if (Mathf.Abs(user.body.deltaRotation) > ROTATION_THRESHOLD && user.body.deltaPosition.magnitude < Mathf.Abs(user.body.deltaRotation)) // Rotation
        {
            degree = user.body.deltaRotation * (MIN_ROTATION_GAIN);
            type = GainType.Rotation;
        }
        else
        {
            type = GainType.Undefined;
        }

        return (type, degree);
    }

    IEnumerator ApplyGain() {
        yield return new WaitForSeconds(2.0f); // just for delay when initializing

        while(true) {
            user = users.GetActiveUser();

            if (user.body.deltaPosition.magnitude > MOVEMENT_THRESHOLD || Mathf.Abs(user.body.deltaRotation) > ROTATION_THRESHOLD) {
                var result = ApplyRedirection();

                GainType gainType = result.Item1;
                float degree = result.Item2;

                switch (gainType)
                {
                    case GainType.Translation:
                        virtualEnvironment.Translate(-user.body.Forward * degree * Time.fixedDeltaTime, Space.World);
                        break;
                    case GainType.Rotation:
                        virtualEnvironment.RotateAround(user.body.Position, degree * Time.fixedDeltaTime);
                        break;
                    case GainType.Curvature:
                        virtualEnvironment.RotateAround(user.body.Position, degree * Time.fixedDeltaTime);
                        break;
                    default:
                        break;
                }
            }
        
            yield return new WaitForFixedUpdate();
        }
    }
}
