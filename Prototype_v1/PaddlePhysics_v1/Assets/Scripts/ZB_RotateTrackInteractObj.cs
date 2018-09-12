using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VRTK;

public class ZB_RotateTrackInteractObj : VRTK_InteractableObject
{
    public ZB_FollowRotation rightRotateTrack;
    public ZB_FollowRotation leftRotateTrack;

    public List<ZB_PaddleBlade> paddleBlades;


    [SerializeField]
    public GameObject AttachPoint_PF;
    [SerializeField]
    public GameObject StaticAttachPoint_PF;

    Rigidbody rBody;

    protected override void Start()
    {
        base.Start();
        rBody = this.GetComponent<Rigidbody>();

        paddleBlades = transform.parent.GetComponentsInChildren<ZB_PaddleBlade>().OfType<ZB_PaddleBlade>().ToList();
    }

    ZB_AttachPoint currStaticAttachPt;


    internal int attachPtCount = 0;
    internal int staticAttachPtCount = 0;

    //VRTK_InteractableObject newInteractableObject;
    //public override void Grabbed(GameObject grabbingObject)
    //{
    //    foreach (ZB_PaddleBlade paddleBlade in paddleBlades)
    //    {
    //        paddleBlade.controllerActions = grabbingObject.GetComponent<VRTK_ControllerActions>();
    //        paddleBlade.controllerActions.TriggerHapticPulse((ushort)10000);
    //    }

    //    var grabScript = grabbingObject.GetComponent<VRTK_InteractGrab>();
    //    var touchScript = grabbingObject.GetComponent<VRTK_InteractTouch>();
    //    grabScript.ForceRelease();

    //    if (attachPtCount >= staticAttachPtCount)
    //    {
    //        staticAttachPtCount++;
    //        GameObject zbAttachPoint = Instantiate(StaticAttachPoint_PF);
    //        GameObject attachPoint = zbAttachPoint.transform.GetChild(0).gameObject;
    //        ZB_AttachPoint attachPointScript = attachPoint.GetComponent<ZB_AttachPoint>();
    //        attachPointScript.spawningObj = this;

    //        Transform spawnPoint = (grabScript && grabScript.controllerAttachPoint) ? grabScript.controllerAttachPoint.transform : grabbingObject.transform;


    //        zbAttachPoint.transform.position = spawnPoint.position;
    //        zbAttachPoint.transform.rotation = transform.rotation;
    //        Vector3 zeroPos = transform.TransformPoint(new Vector3(0f, transform.InverseTransformPoint(zbAttachPoint.transform.position).y, 0f));
    //        zbAttachPoint.transform.position = zeroPos;

    //        touchScript.ForceStopTouching();
    //        touchScript.ForceTouch(attachPoint.gameObject);

    //        grabScript.AttemptGrab();

    //        ConfigurableJoint attachPointJoint = attachPoint.GetComponent<ConfigurableJoint>();

    //        attachPointJoint.connectedBody = rBody;
    //        SetStaticAttachPointJointRestraints(ref attachPointJoint);
    //    }
    //    else {
    //        attachPtCount++;
    //        GameObject zbAttachPoint = Instantiate(AttachPoint_PF);
    //        GameObject attachPoint = zbAttachPoint.transform.GetChild(0).gameObject;
    //        GameObject slider = zbAttachPoint.transform.GetChild(1).gameObject;
    //        ZB_AttachPoint attachPointScript = attachPoint.GetComponent<ZB_AttachPoint>();
    //        attachPointScript.spawningObj = this;

    //        Transform spawnPoint = (grabScript && grabScript.controllerAttachPoint) ? grabScript.controllerAttachPoint.transform : grabbingObject.transform;

    //        //zbAttachPoint.transform.SetParent(transform, true);

    //        zbAttachPoint.transform.position = spawnPoint.position;
    //        zbAttachPoint.transform.rotation = transform.rotation;
    //        Vector3 zeroPos = transform.TransformPoint(new Vector3(0f, transform.InverseTransformPoint(zbAttachPoint.transform.position).y, 0f));
    //        zbAttachPoint.transform.position = zeroPos;

    //        touchScript.ForceStopTouching();
    //        touchScript.ForceTouch(attachPoint.gameObject);

    //        grabScript.AttemptGrab();

    //        ConfigurableJoint sliderJoint = slider.GetComponent<ConfigurableJoint>();
    //        ConfigurableJoint attachPointJoint = attachPoint.GetComponent<ConfigurableJoint>();

    //        //attachPointJoint.connectedBody = rBody;
    //        //if (handsOnPaddle > 0)
    //        //attachPointJoint.angularYMotion = ConfigurableJointMotion.Free;

    //        sliderJoint.connectedBody = rBody;
    //        SetAttachPointJointRestraints(ref attachPointJoint);
    //        SetSliderJointRestraints(ref sliderJoint);
    //    }
    //}

    //public override void Ungrabbed(GameObject previousGrabbingObject)
    //{
    //    //handsOnPaddle--;
    //    base.Ungrabbed(previousGrabbingObject);
    //    //foreach (ZB_PaddleBlade paddleBlade in paddleBlades)
    //    //{
    //    //    paddleBlade.controllerActions.TriggerHapticPulse((ushort)10000);
    //    //    paddleBlade.controllerActions = null;
    //    //}
    //}
}
