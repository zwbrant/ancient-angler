using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class FishingPole_InteractObj : VRTK_InteractableObject {
    [Header("Component Dependencies")]
    public FishingReel Reel;
    public float ReelingMultiplier = .5f;


    public VRTK_ControllerEvents CntrlEvents { get { return _cntrlEvents; } }

    private VRTK_ControllerReference _cntrlReference;
    private VRTK_ControllerEvents _cntrlEvents;

    protected override void Awake()
    {
        base.Awake();

        Reel = UnityUtilities.TryResolveDependency(Reel, gameObject);
    }

    #region Controller events

    public override void Grabbed(VRTK_InteractGrab grabbingObject)
    {
        base.Grabbed(grabbingObject);

        _cntrlEvents =  grabbingObject.GetComponent<VRTK_ControllerEvents>();
        _cntrlReference = VRTK_ControllerReference.GetControllerReference(grabbingObject.controllerEvents.gameObject);
    }

    public override void Ungrabbed(VRTK_InteractGrab previousGrabbingObject)
    {
        base.Ungrabbed(previousGrabbingObject);
        _cntrlReference = null;
    }

    #endregion

    protected override void OnEnable()
    {
        base.OnEnable();
        _cntrlReference = null;
    }

    public void HapticPulse(float strength)
    {
        VRTK_ControllerHaptics.TriggerHapticPulse(_cntrlReference, strength);

    }

    public void HapticPulse(float strength, float duration)
    {
        VRTK_ControllerHaptics.TriggerHapticPulse(_cntrlReference, strength, duration, .01f);

    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();




        if (CntrlEvents != null)
        {
            if (CntrlEvents.triggerTouched)
            {
                if (!Reel.IsBailOpen)
                {
                    Reel.OpenBail();
                }
            } else if (Reel.IsBailOpen)
            {
                Reel.CloseBail();
            }

            var touchpadY = CntrlEvents.GetTouchpadAxis().y;
            if (touchpadY < 0)
                Reel.ReelIn(Mathf.Abs(touchpadY * .5f));
            else if (touchpadY > 0)
                Reel.ReelOut(touchpadY * .5f);

            
        }
    }

}
