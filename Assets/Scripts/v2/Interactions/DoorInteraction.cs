using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class DoorInteraction : MonoBehaviour
{
    private Hand.AttachmentFlags attachmentFlags = Hand.defaultAttachmentFlags & (~Hand.AttachmentFlags.SnapOnAttach) & (~Hand.AttachmentFlags.DetachOthers) & (~Hand.AttachmentFlags.VelocityMovement);
    private Interactable interactable;
    private GrabTypes grabType;
    // private Users users;

    private void Start()
    {
        interactable = this.GetComponent<Interactable>();
        // users = GameObject.FindGameObjectWithTag("Users").GetComponent<Users>();
    }

    private void HandHoverUpdate(Hand hand)
    {
        GrabTypes startingGrabType = hand.GetGrabStarting();
        GrabTypes endGingGrabType = hand.GetGrabEnding();
        bool isGrabEnding = hand.IsGrabEnding(this.gameObject);

        // Debug.Log($"startingGrabType: {startingGrabType}");
        // Debug.Log($"endGingGrabType: {endGingGrabType}");

        if (startingGrabType != GrabTypes.None)
        {
            grabType = startingGrabType;
            // Debug.Log("Grabbing!");

            // Call this to continue receiving HandHoverUpdate messages,
            // and prevent the hand from hovering over anything else
            hand.HoverLock(interactable);

            this.transform.Rotate(new Vector3(0, 0, -45), Space.Self);
            this.transform.parent.parent.GetComponent<Door>().OpenDoor();

            // // Attach this object to the hand
            // hand.AttachObject(gameObject, startingGrabType, attachmentFlags);
        }
        else if (endGingGrabType == grabType)
        {
            // Debug.Log("Releasing!");
            // Detach this object from the hand
            // hand.DetachObject(gameObject);

            // // Call this to undo HoverLock
            hand.HoverUnlock(interactable);
        }
    }

    private void OnAttachedToHand(Hand hand) {
        // Debug.Log("OnAttachedToHand");
        User user =  hand.transform.parent.GetComponent<User>();
        UserEventArgs caller = new UserEventArgs(Behaviour.Grab, this.gameObject);
        user.ProcessingEvent(caller);
    }

    private void OnDetachedFromHand(Hand hand)
    {
        // Debug.Log("OnDetachedFromHand");
        User user = hand.transform.parent.GetComponent<User>();
        UserEventArgs caller = new UserEventArgs(Behaviour.Release, this.gameObject);
        user.ProcessingEvent(caller);
    }
}
