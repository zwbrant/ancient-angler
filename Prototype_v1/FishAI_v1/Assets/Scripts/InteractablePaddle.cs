using UnityEngine;
using System.Collections;
using VRTK;
using System.Collections.Generic;
using System.Linq;

public class InteractablePaddle : VRTK_InteractableObject {
    [SerializeField]
    public List<ZB_PaddleBlade> paddleBlades;

    protected override void Start()
    {
        base.Start();
        if (paddleBlades == null || paddleBlades.Count < 1)
            paddleBlades = transform.parent.GetComponentsInChildren<ZB_PaddleBlade>().OfType<ZB_PaddleBlade>().ToList();
    }

    public override void Grabbed(GameObject grabbingObject)
    {
        base.Grabbed(grabbingObject);
        foreach (ZB_PaddleBlade paddleBlade in paddleBlades)
        {
            paddleBlade.mainControlActions = grabbingObject.GetComponent<VRTK_ControllerActions>();
            paddleBlade.mainControlActions.TriggerHapticPulse((ushort)10000);
        }
    }

    public override void Ungrabbed(GameObject previousGrabbingObject)
    {
        base.Ungrabbed(previousGrabbingObject);
        foreach (ZB_PaddleBlade paddleBlade in paddleBlades)
        {
            paddleBlade.mainControlActions.TriggerHapticPulse((ushort)10000);
            paddleBlade.mainControlActions = null;
        }
    }
}
