using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class ReelHandle_InteractObj : VRTK_InteractableObject
{
    [Header("Reeling")]
    public float LineReeledPerRotation = .5f;

    [Header("Haptics")]
    public int PulsesPerRotation = 6;
    public float BaseHapticStrength = .25f;
    public float LineResistanceHapicMulti = .01f;

    [Header("Component Dependencies")]
    public FishingReel Reel;
    public FollowRotation FollowRotation;

    public VRTK_ControllerReference CntrlReference { get; private set; }
    public VRTK_ControllerEvents CntrlEvents { get; private set; }

    private float _degreesPerPulse;
    private float _handleRotationSum = 0f;

    protected override void Awake()
    {
        base.Awake();

        Reel = UnityUtilities.TryResolveDependency(Reel, transform.parent.parent.gameObject);
        FollowRotation = UnityUtilities.TryResolveDependency(FollowRotation, transform.parent.gameObject);

        _degreesPerPulse = 360f / PulsesPerRotation;
    }

    protected override void Update()
    {
        base.Update();


    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (IsGrabbed())
        {
            if (FollowRotation.LastAngleDiff > 0) {
                _handleRotationSum += FollowRotation.LastAngleDiff;
                if (_handleRotationSum >= _degreesPerPulse)
                {
                    _handleRotationSum = _handleRotationSum % _degreesPerPulse;
                    VRTK_ControllerHaptics.TriggerHapticPulse(CntrlReference, 
                        BaseHapticStrength + (Reel.Pole.Line.CurrentEndpointAttachmentForce.magnitude * LineResistanceHapicMulti));
                }

                Reel.ReelIn((FollowRotation.LastAngleDiff / 360f) * LineReeledPerRotation);

            } else if (FollowRotation.LastAngleDiff < 0)
            {
                _handleRotationSum = 0f;
            }
 
            //print(string.Format("Last AngleDiff: {0} | Sum: {1}", FollowRotation.LastAngleDiff, _handleRotationSum));
        }
    }

    public override void Grabbed(VRTK_InteractGrab grabbingObject)
    {
        base.Grabbed(grabbingObject);

        CntrlEvents = grabbingObject.GetComponent<VRTK_ControllerEvents>();
        CntrlReference = VRTK_ControllerReference.GetControllerReference(CntrlEvents.gameObject);

        // FollowRotation stuff
        var interactGrab = grabbingObject.GetComponent<VRTK_InteractGrab>();
        FollowRotation.FollowTarget = interactGrab.controllerAttachPoint.transform;
        FollowRotation.Follow = true;


    }

    public override void Ungrabbed(VRTK_InteractGrab grabbingObject)
    {
        base.Ungrabbed(grabbingObject);

        CntrlEvents = null;

        // FollowRotation stuff
        FollowRotation.Follow = false;

    }


}
