using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class PoleHandle_InteractObj : VRTK_InteractableObject {
    public Transform ChildOnGrabbed;

    private VRTK_ControllerReference _controllerReference;
    private Transform _oldParent;

    public override void Grabbed(VRTK_InteractGrab grabbingObject)
    {
        base.Grabbed(grabbingObject);

        grabbingObject.GetComponent<VRTK_ControllerEvents>();

        _controllerReference = VRTK_ControllerReference.GetControllerReference(grabbingObject.controllerEvents.gameObject);
        _oldParent = ChildOnGrabbed.parent;
        ChildOnGrabbed.SetParent(transform);
    }

    public override void Ungrabbed(VRTK_InteractGrab previousGrabbingObject)
    {
        base.Ungrabbed(previousGrabbingObject);
        _controllerReference = null;
        ChildOnGrabbed.parent = _oldParent;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        _controllerReference = null;
       // interactableRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    public void HapticPulse(float strength)
    {
        VRTK_ControllerHaptics.TriggerHapticPulse(_controllerReference, strength);

    }

    private void FixedUpdate()
    {
        
    }
}
