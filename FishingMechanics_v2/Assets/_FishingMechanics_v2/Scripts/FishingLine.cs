using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(LineRenderer), typeof(RbodyVerletLine))]
public class FishingLine : MonoBehaviour
{
    public GameObject StartingAnchorPoint;
    public LureInteractions Lure;
    public GameObject StartingEndpoint;

    public GameObject CurrentEndpoint { get; private set; }
    public RbodyVerletLine BaseLine { get; private set; }
    public LineRenderer LineRenderer { get; private set; }
    public Vector3 CurrentEndpointAttachmentForce { get { return BaseLine.CurrentRbodyForce; }}

    private bool _lureEnabled = false;

    private void Awake()
    {
        if (LineRenderer == null) { LineRenderer = GetComponent<LineRenderer>(); }
        if (BaseLine == null) { BaseLine = GetComponent<RbodyVerletLine>(); }
    }

    private void Start()
    {
        LineRenderer.positionCount = BaseLine.LinePoints.Count();

        if (StartingAnchorPoint == null) { StartingAnchorPoint = this.gameObject; }
        BaseLine.AnchorPoint = StartingAnchorPoint.transform;

        if (StartingEndpoint != null)
        {
            var rbody = StartingEndpoint.GetComponent<Rigidbody>();
            if (rbody != null)
                AttachEndpoint(rbody);
            else
                AttachEndpoint(StartingEndpoint.transform);
        } else if (Lure != null)
        {
            EnableLure();
        }
    }

    public void EnableLure()
    {
        Lure.transform.position = BaseLine.LastParticle.Position;
        Lure.gameObject.SetActive(true);
        BaseLine.AttachRbody(Lure.Rbody);
        _lureEnabled = true;
    }

    public void DisableLure()
    {
        Lure.gameObject.SetActive(false);
        DetachEndpoint();
        _lureEnabled = false;
    }

    private void Update()
    {
        LineRenderer.positionCount = BaseLine.LinePoints.Count();
        LineRenderer.SetPositions(BaseLine.LinePoints.ToArray());
    }

    public void DetachEndpoint()
    {
        BaseLine.DetachTransform();
        BaseLine.DetachRbody();
    }

    public void AttachEndpoint(Transform endpoint) {
        if (Lure != null)
            DisableLure();
        CurrentEndpoint = endpoint.gameObject;
        BaseLine.AttachTransform(endpoint);
    }

    public void AttachEndpoint(Rigidbody endpoint)
    {
        if (Lure != null)
            DisableLure();
        CurrentEndpoint = endpoint.gameObject;
        BaseLine.AttachRbody(endpoint);
    }

    public void AttachEndpoint(Fish fish, Vector3 localAttachPoint)
    {
        if (Lure != null)
            DisableLure();
        if (fish.RbodyResistance != null)
            AttachEndpoint(fish.RbodyResistance.Rbody);
        else
            AttachEndpoint(fish.transform);

        CurrentEndpoint = fish.gameObject;

        BaseLine.AttachPointOnConnection = localAttachPoint;
    }
    
}

