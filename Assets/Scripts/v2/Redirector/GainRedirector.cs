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

    protected Users users;
    protected VirtualEnvironment virtualEnvironment;
    protected RealSpace realSpace;

    protected Experiment2 experiment2;

    private void Awake() {
        foreach(Transform child in transform) {
            Transform2D tf = child.GetComponent<Transform2D>();

            if(tf is VirtualEnvironment)
                virtualEnvironment = tf as VirtualEnvironment;
            else if(tf is Users)
                users = tf as Users;
            else if(tf is RealSpace)
                realSpace = tf as RealSpace;
        }

        experiment2 = GetComponent<Experiment2>();
    }

    public virtual (GainType, float) ApplyRedirection() {
        float degree = 0;
        GainType type = GainType.Undefined;

        // if (user.Body.deltaPosition.magnitude > MOVEMENT_THRESHOLD && user.Body.deltaPosition.magnitude >= Mathf.Abs(user.Body.deltaRotation)) // Translation
        // {
        //     degree = user.Body.deltaPosition.magnitude * (MIN_TRANSLATION_GAIN);
        //     if(Vector2.Angle(user.Body.deltaPosition, user.Body.Forward) > 5f) degree = 0;
        //     type = GainType.Translation;
        // }
        if(user.Body.deltaPosition.magnitude > 0.2f && user.Body.deltaPosition.magnitude >= Mathf.Abs(user.Body.deltaRotation)) // Curvature
        {
            degree = Mathf.Rad2Deg * user.Body.deltaPosition.magnitude * (HODGSON_MAX_CURVATURE_GAIN);
            if(Vector2.Angle(user.Body.deltaPosition, user.Body.Forward) > 5f) degree = 0;
            type = GainType.Curvature;
        }
        else if (Mathf.Abs(user.Body.deltaRotation) > ROTATION_THRESHOLD && user.Body.deltaPosition.magnitude < Mathf.Abs(user.Body.deltaRotation)) // Rotation
        {
            degree = user.Body.deltaRotation * (MAX_ROTATION_GAIN);
            type = GainType.Rotation;
        }
        else
        {
            type = GainType.Undefined;
        }

        return (type, degree);
    }

    private void FixedUpdate() {
        experiment2.isLocomotionDone = true;
        user = users.GetActiveUser();

        if (user.Body.deltaPosition.magnitude > MOVEMENT_THRESHOLD || Mathf.Abs(user.Body.deltaRotation) > ROTATION_THRESHOLD) {
            var result = ApplyRedirection();

            GainType gainType = result.Item1;
            float degree = result.Item2;

            // Debug.Log($"gainType {gainType}");
            // Debug.Log($"degree {degree}");
            // Debug.Log($"user.Body.deltaRotation {user.Body.deltaRotation}");
            // Debug.Log($"user.Body.deltaPosition {user.Body.deltaPosition}");

            switch (gainType)
            {
                case GainType.Translation:
                    virtualEnvironment.Translate(-user.Body.Forward * degree * Time.fixedDeltaTime, Space.World);
                    break;
                case GainType.Rotation:
                    virtualEnvironment.RotateAround(user.Body.Position, degree * Time.fixedDeltaTime);
                    break;
                case GainType.Curvature:
                    virtualEnvironment.RotateAround(user.Body.Position, degree * Time.fixedDeltaTime);
                    break;
                default:
                    break;
            }
        }
    }
}
