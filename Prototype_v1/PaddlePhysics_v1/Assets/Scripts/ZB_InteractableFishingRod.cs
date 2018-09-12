using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VRTK;
using System.Linq;

public class ZB_InteractableFishingRod : VRTK_InteractableObject
{
    public ZB_Reel reel;
    public static VRTK_ControllerActions rodController;

    public override void Grabbed(GameObject grabbingObject)
    {
        base.Grabbed(grabbingObject);
        VRTK_ControllerActions cntrlActions = grabbingObject.GetComponent<VRTK_ControllerActions>();
        reel.rodControllerActions = cntrlActions;
        rodController = cntrlActions;
    }

    public override void Ungrabbed(GameObject previousGrabbingObject)
    {
        base.Ungrabbed(previousGrabbingObject);
        reel.rodControllerActions = null;
        rodController = null;
    }
}
