using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteerToTargetRedirector : GainRedirector
{
    private const float DISTANCE_THRESHOLD_FOR_DAMPENING = 1.25f; // Distance threshold to apply dampening (meters)
    private const float ANGLE_THRESHOLD_FOR_DAMPENING = 60f; // Angle threshold to apply dampening (degrees)
    private const float SMOOTHING_FACTOR = 0.125f; // Smoothing factor for redirection rotations

    private float previousMagnitude = 0f;

    // public Bound2D realSpace;
    // protected Transform2D realUser;
    protected RealUser realUser;
    protected Vector2 userPosition; // realUser position
    protected Vector2 userDirection; // realUser direction (realUser forward)
    protected Vector2 targetPosition; // steerting target localPosition

    public virtual void PickSteeringTarget() {}

    public override (GainType, float) ApplyRedirection()
    {        
        // define some variables for redirection
        realUser = realSpace.realUser;
        userPosition = realUser.Position;
        userDirection = realUser.Forward;

        // pick a target to where user steer
        PickSteeringTarget();

        Vector2 userToTarget = targetPosition - userPosition;
        float angleToTarget = Vector2.Angle(userDirection, userToTarget);
        float distanceToTarget = userToTarget.magnitude;

        // control applied gains according to user and target
        float directionToTarget = Mathf.Sign(Vector2.SignedAngle(userDirection, userToTarget)); // if target is to the left of the user, directionToTarget > 0
        float directionRotation = Mathf.Sign(user.Body.deltaRotation); // If user is rotating to the left, directionRotation > 0

        if (directionToTarget > 0)  // If the target is to the left of the user,
            curvatureGain = HODGSON_MIN_CURVATURE_GAIN;
        else
            curvatureGain = HODGSON_MAX_CURVATURE_GAIN;

        if (directionToTarget * directionRotation < 0) // if user rotates away from the target (if their direction are opposite),
            rotationGain = MIN_ROTATION_GAIN;
        else
            rotationGain = MAX_ROTATION_GAIN;

        // select the largest magnitude
        float rotationMagnitude = 0, curvatureMagnitude = 0;

        if (Mathf.Abs(user.Body.deltaRotation) >= ROTATION_THRESHOLD)
            rotationMagnitude = rotationGain * user.Body.deltaRotation;
        if (user.Body.deltaPosition.magnitude > MOVEMENT_THRESHOLD)
            curvatureMagnitude = Mathf.Rad2Deg * curvatureGain * user.Body.deltaPosition.magnitude;

        float selectedMagnitude = Mathf.Max(Mathf.Abs(rotationMagnitude), Mathf.Abs(curvatureMagnitude)); // selectedMagnitude is ABS(절대값)
        bool isCurvatureSelected = Mathf.Abs(curvatureMagnitude) > Mathf.Abs(rotationMagnitude);

        // dampening 
        if (angleToTarget <= ANGLE_THRESHOLD_FOR_DAMPENING)
           selectedMagnitude *= Mathf.Sin(Mathf.Deg2Rad * 90 * angleToTarget / ANGLE_THRESHOLD_FOR_DAMPENING);
        if (distanceToTarget <= DISTANCE_THRESHOLD_FOR_DAMPENING)
        {
            selectedMagnitude *= distanceToTarget / DISTANCE_THRESHOLD_FOR_DAMPENING;
        }

        //smoothing
        float finalRotation = (1.0f - SMOOTHING_FACTOR) * previousMagnitude + SMOOTHING_FACTOR * selectedMagnitude;
        previousMagnitude = finalRotation;

        // apply final redirection
        if (!isCurvatureSelected)
        {
            // float direction = directionRotation;
            float direction = Mathf.Sign(rotationGain);
            return (GainType.Rotation, finalRotation * direction);
        }
        else
        {
            float direction = Mathf.Sign(curvatureGain);
            // if(Vector2.Angle(user.Body.deltaPosition, user.Body.Forward) > 5f) finalRotation = 0;
            return (GainType.Curvature, finalRotation * direction);
        }
    }

}
