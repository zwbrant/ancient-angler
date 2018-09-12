using UnityEngine;
using System.Collections;
using VRTK;

public class ZB_AttachPoint : VRTK_InteractableObject {
    Transform initialParent;

    internal ZB_AkimboInteractableObject spawningObj;
    internal bool isRightController;

    GameObject emptyGO;
    GameObject attachPtGO;
    GameObject sliderGO; 

    protected override void Awake()
    {
        base.Awake();
        emptyGO = transform.parent.gameObject;
        attachPtGO = transform.gameObject;
        if (emptyGO.transform.childCount == 2)
        {
            sliderGO = emptyGO.transform.GetChild(1).gameObject;
        }

        
    }

    public override void Ungrabbed(GameObject previousGrabbingObject)
    {
        base.Ungrabbed(previousGrabbingObject);

        VRTK_ControllerActions controlActions = previousGrabbingObject.GetComponent<VRTK_ControllerActions>();
        //controlActions.TriggerHapticPulse((ushort)10000);

        //Destroy(initialParent.gameObject);
        if (sliderGO != null)
        {
            foreach(ZB_PaddleBlade paddle in spawningObj.paddleBlades)
            {
                paddle.mainControlActions = null;
            }
            //spawningObj.currStaticJoint = null;
            spawningObj.attachPtCount--;
        }
        else {
            foreach (ZB_PaddleBlade paddle in spawningObj.paddleBlades)
            {
                if (spawningObj.alwaysUseSlider)
                {
                    paddle.mainControlActions = null;
                }
                else
                {
                    paddle.secondaryControlActions = null;
                    paddle.forcePoint = null;
                }
            }
            spawningObj.staticAttachPtCount--;
        }

        if (isRightController)
            spawningObj.rightRotateTrack.follow = false;
        else
            spawningObj.leftRotateTrack.follow = false;

        Destroy(sliderGO);
        Destroy(emptyGO);
        Destroy(gameObject);
    }

    override protected void OnJointBreak(float breakForce)
    {
        VRTK_ControllerActions controlActions = grabbingObject.GetComponent<VRTK_ControllerActions>();
        controlActions.TriggerHapticPulse((ushort)10000);
        this.Ungrabbed(grabbingObject);
    }
}
