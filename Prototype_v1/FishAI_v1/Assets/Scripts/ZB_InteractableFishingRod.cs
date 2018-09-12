using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VRTK;
using System.Linq;

public class ZB_InteractableFishingRod : VRTK_InteractableObject
{
    public ZB_Reel reel;

    public override void Grabbed(GameObject grabbingObject)
    {
        base.Grabbed(grabbingObject);
        reel.rodControllerActions = grabbingObject.GetComponent<VRTK_ControllerActions>();
    }

    public override void Ungrabbed(GameObject previousGrabbingObject)
    {
        base.Ungrabbed(previousGrabbingObject);
        reel.rodControllerActions = null;
    }
}
