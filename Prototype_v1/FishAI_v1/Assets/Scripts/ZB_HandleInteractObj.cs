using UnityEngine;
using System.Collections;
using VRTK;

public class ZB_HandleInteractObj : VRTK_InteractableObject {

    [SerializeField]
    public GameObject StaticAttachPoint_PF;
    public ZB_Reel reel;

    Rigidbody rBody;

    protected override void Start()
    {
        base.Start();
        rBody = this.GetComponent<Rigidbody>();
    }

    internal bool held = false;
    public override void Grabbed(GameObject grabbingObject)
    {
        if (held == false) {
            held = true;
            reel.handleInteractObjCallback = this;

            var grabScript = grabbingObject.GetComponent<VRTK_InteractGrab>();
            var touchScript = grabbingObject.GetComponent<VRTK_InteractTouch>();
            grabScript.ForceRelease();

            ToggleHighlight(true);

            Transform spawnPoint = grabScript.controllerAttachPoint.transform;

            VRTK_ControllerActions controllerActions = grabbingObject.GetComponent<VRTK_ControllerActions>();
            reel.reelControllerActions = controllerActions;
            // make the reel handle start following
            reel.handleRotation.followPoint = spawnPoint;
            reel.handleRotation.follow = true;
        }
    }

    public override void Ungrabbed(GameObject previousGrabbingObject)
    {
        base.Ungrabbed(previousGrabbingObject);
       
    }
}
