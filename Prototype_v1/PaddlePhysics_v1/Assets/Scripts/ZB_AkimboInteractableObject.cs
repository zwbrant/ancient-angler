using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VRTK;
using System.Linq;

public class ZB_AkimboInteractableObject : VRTK_InteractableObject
{
    [SerializeField]
    public ZB_FollowRotation rightRotateTrack;
    [SerializeField]
    public ZB_FollowRotation leftRotateTrack;
    [SerializeField]
    public bool alwaysUseStatic = false;
    [SerializeField]
    public bool alwaysUseSlider = false;
    [SerializeField]
    public GameObject AttachPoint_PF;
    [SerializeField]
    public GameObject StaticAttachPoint_PF;
    public const float breakForce = 1850f;

    internal List<ZB_PaddleBlade> paddleBlades = new List<ZB_PaddleBlade>();
    Rigidbody rBody;

    protected override void Start()
    {
        base.Start();
        rBody = this.GetComponent<Rigidbody>();
 
        paddleBlades = transform.GetComponentsInChildren<ZB_PaddleBlade>().OfType<ZB_PaddleBlade>().ToList();
    }

    ZB_AttachPoint currStaticAttachPt;


    internal int attachPtCount = 0;
    internal int staticAttachPtCount = 0;

    internal GameObject currRightControl = null;
    internal GameObject currLeftControl = null;
    internal GameObject currRightAttachPt = null;
    internal GameObject currLeftAttachPt = null;
    internal ConfigurableJoint currStaticJoint = null;
    internal float rightControlLocalY;
    internal float leftControlLocalY;


    VRTK_InteractableObject newInteractableObject;
    public override void Grabbed(GameObject grabbingObject)
    {
        VRTK_InteractGrab grabScript = grabbingObject.GetComponent<VRTK_InteractGrab>();
        VRTK_InteractTouch touchScript = grabbingObject.GetComponent<VRTK_InteractTouch>();
        VRTK_ControllerActions controllerActions = grabbingObject.GetComponent<VRTK_ControllerActions>();
        grabScript.ForceRelease();

        bool isRightController = grabbingObject.name.Contains("right");

        if (!alwaysUseSlider && (alwaysUseStatic || attachPtCount >= staticAttachPtCount))
        {
            //print("making static joint");
            staticAttachPtCount++;


            GameObject zbAttachPoint = Instantiate(StaticAttachPoint_PF);
            GameObject attachPoint = zbAttachPoint.transform.GetChild(0).gameObject;

            foreach (ZB_PaddleBlade paddle in paddleBlades)
            {
                if (alwaysUseSlider)
                {
                    paddle.mainControlActions = controllerActions;
                }
                else
                {
                    paddle.secondaryControlActions = controllerActions;
                    paddle.ForcePoint = attachPoint.gameObject;
                }
            }

            ZB_AttachPoint attachPointScript = attachPoint.GetComponent<ZB_AttachPoint>();

            Transform spawnPoint = (grabScript && grabScript.controllerAttachPoint) ? grabScript.controllerAttachPoint.transform: grabbingObject.transform;
            //Transform spawnPoint = grabbingObject.transform.GetChild(0).transform.GetChild(1).GetChild(0).transform;

            if (isRightController)
                rightSnapHandle = attachPoint.transform.GetChild(0).transform;
            else
                leftSnapHandle = attachPoint.transform.GetChild(0).transform;

            attachPointScript.spawningObj = this;

            touchScript.ForceStopTouching();
            touchScript.ForceTouch(attachPoint.gameObject);

            zbAttachPoint.transform.position = spawnPoint.position;
            zbAttachPoint.transform.rotation = transform.rotation;
            float attachPtLocalY = transform.InverseTransformPoint(zbAttachPoint.transform.position).y;
            Vector3 zeroPos = transform.TransformPoint(new Vector3(0f, attachPtLocalY, 0f));
            zbAttachPoint.transform.position = zeroPos;

            ConfigurableJoint attachPtJoint = attachPoint.GetComponent<ConfigurableJoint>();

            //sliderJoint.connectedBody = rBody;

            attachPtJoint.connectedBody = rBody;
            SetStaticAttachPointJointRestraints(ref attachPtJoint);

            attachPtJoint.connectedBody = rBody;

            grabScript.AttemptGrab();

            if (isRightController)
            {
                rightControlLocalY = attachPtLocalY;
                currRightControl = grabbingObject;
                if (alwaysUseSlider)
                    rightRotateTrack.follow = true;
                attachPointScript.isRightController = true;
                currRightAttachPt = attachPointScript.gameObject;
            }
            else
            {
                leftControlLocalY = attachPtLocalY;
                currLeftControl = grabbingObject;
                if (alwaysUseSlider)
                    leftRotateTrack.follow = true;
                attachPointScript.isRightController = false;
                currLeftAttachPt = attachPointScript.gameObject;
            }

        }
        else {
            //print("making slider joint");
            attachPtCount++;

            foreach (ZB_PaddleBlade paddleBlade in paddleBlades)
            {
                paddleBlade.mainControlActions = controllerActions;
            }

            GameObject zbAttachPoint = Instantiate(AttachPoint_PF);
            GameObject attachPoint = zbAttachPoint.transform.GetChild(0).gameObject;
            GameObject slider = zbAttachPoint.transform.GetChild(1).gameObject;
            ZB_AttachPoint attachPointScript = attachPoint.GetComponent<ZB_AttachPoint>();

            Transform spawnPoint = (grabScript && grabScript.controllerAttachPoint) ? grabScript.controllerAttachPoint.transform : grabbingObject.transform;

            if (isRightController)
                rightSnapHandle = attachPoint.transform.GetChild(0).transform;
            else
                leftSnapHandle = attachPoint.transform.GetChild(0).transform;

            attachPointScript.spawningObj = this;

            touchScript.ForceStopTouching();
            touchScript.ForceTouch(attachPoint.gameObject);

            zbAttachPoint.transform.position = spawnPoint.position;
            zbAttachPoint.transform.rotation = transform.rotation;
            float attachPtLocalY = transform.InverseTransformPoint(zbAttachPoint.transform.position).y;
            Vector3 zeroPos = transform.TransformPoint(new Vector3(0f, attachPtLocalY, 0f));
            zbAttachPoint.transform.position = zeroPos;

            ConfigurableJoint sliderJoint = slider.GetComponent<ConfigurableJoint>();
            ConfigurableJoint attachPtJoint = attachPoint.GetComponent<ConfigurableJoint>();

            attachPtJoint.connectedBody = rBody;
            SetStaticAttachPointJointRestraints(ref attachPtJoint);
            SetSliderJointRestraints(ref sliderJoint);

            attachPtJoint.connectedBody = rBody;
            sliderJoint.connectedBody = rBody;

            grabScript.AttemptGrab();

            if (isRightController)
            {
                rightControlLocalY = attachPtLocalY;
                currRightControl = grabbingObject;

                rightRotateTrack.follow = true;


                attachPointScript.isRightController = true;
                currRightAttachPt = attachPointScript.gameObject;
            }
            else
            {
                leftControlLocalY = attachPtLocalY;
                currLeftControl = grabbingObject;
                leftRotateTrack.follow = false;

                attachPointScript.isRightController = false;
                currLeftAttachPt = attachPointScript.gameObject;
            }

            StartCoroutine(DelayedJointConnect(attachPtJoint, slider.GetComponent<Rigidbody>()));
        }
    }


    IEnumerator DelayedJointConnect(ConfigurableJoint attachPtJoint, Rigidbody rBody)
    {
        yield return new WaitForSeconds(.2f);
        //print("joint connected");

        attachPtJoint.connectedBody = rBody;
        attachPtJoint.breakForce = breakForce;
    }

    public override void Ungrabbed(GameObject previousGrabbingObject)
    {
        currRightControl = null;
        currLeftControl = null;
        currLeftAttachPt = null;
        currRightAttachPt = null;

        base.Ungrabbed(previousGrabbingObject);
    }

    private static Vector3 topOfShaft = new Vector3(0f, 10f, 0f);
    protected override void FixedUpdate()
    {
        float rightYRotation = 0f;

        if (rightRotateTrack.follow)
        {
            //Vector3 localRotatorPos = transform.InverseTransformPoint(rightRotateTrack.transform.position);
            //Vector3 localAttachPtPos = transform.InverseTransformPoint(currRightAttachPt.transform.position);
            //float contollerPoleAngle = Vector3.Angle(topOfShaft, localRotatorPos);

            //if (localAttachPtPos.y > localRotatorPos.y)
                rightYRotation += rightRotateTrack.LastAngleDiff * 1.2f;
            //else
                //rightYRotation += rightRotateTrack.LastAngleDiff * 1.5f;

        }

        float leftYRotation = 0f;

        if (leftRotateTrack.follow)
        {
            //Vector3 localRotatorPos = transform.InverseTransformPoint(leftRotateTrack.transform.position);
            //Vector3 localAttachPtPos = transform.InverseTransformPoint(currLeftAttachPt.transform.position);
            //float contollerPoleAngle = Vector3.Angle(topOfShaft, localRotatorPos);

            //if (localAttachPtPos.y > localRotatorPos.y)
                leftYRotation += leftRotateTrack.LastAngleDiff * 1.2f;
            //else
                //leftYRotation += leftRotateTrack.LastAngleDiff * 1.5f;
        }

        // make sure there's some rotation, then, if the hands are going opposite directions, split the difference,
        // otherwise use the greater value
        if (Mathf.Abs(leftYRotation) > 0f || Mathf.Abs(rightYRotation) > 0f)
        {
            //if ((leftYRotation < 0f && rightYRotation > 0f) || (leftYRotation > 0f && rightYRotation < 0f))
            //    transform.Rotate(new Vector3(0f, (leftYRotation + rightYRotation), 0f));
            if (Mathf.Abs(leftYRotation) > Mathf.Abs(rightYRotation))
                transform.Rotate(new Vector3(0f, (leftYRotation), 0f));
            else
                transform.Rotate(new Vector3(0f, (rightYRotation), 0f));
        }
        else
        {
           // rBody.AddRelativeTorque(new Vector3(0f, 0f, 0f), ForceMode.VelocityChange);
        }
    }


    public static void SetSliderJointRestraints(ref ConfigurableJoint joint)
    {
        joint.angularXMotion = ConfigurableJointMotion.Locked;
        joint.angularYMotion = ConfigurableJointMotion.Free;
        joint.angularZMotion = ConfigurableJointMotion.Locked;
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Limited;
        joint.zMotion = ConfigurableJointMotion.Locked;
    }

    public static void SetAttachPointJointRestraints(ref ConfigurableJoint joint)
    {
        joint.angularXMotion = ConfigurableJointMotion.Free;
        joint.angularYMotion = ConfigurableJointMotion.Free;
        joint.angularZMotion = ConfigurableJointMotion.Free;
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;
    }

    public static void SetStaticAttachPointJointRestraints(ref ConfigurableJoint joint)
    {
        joint.angularXMotion = ConfigurableJointMotion.Free;
        joint.angularYMotion = ConfigurableJointMotion.Free;
        joint.angularZMotion = ConfigurableJointMotion.Free;
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;
    }

    public static void SetJointTotallyFree(ref ConfigurableJoint joint)
    {
        joint.angularXMotion = ConfigurableJointMotion.Free;
        joint.angularYMotion = ConfigurableJointMotion.Free;
        joint.angularZMotion = ConfigurableJointMotion.Free;
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;
    }
}
