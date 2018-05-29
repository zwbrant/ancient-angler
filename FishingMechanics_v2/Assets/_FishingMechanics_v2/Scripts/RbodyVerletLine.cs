using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RbodyVerletLine : VerletLine
{
    [Header("Rigidbody Options")]
    public float ForceMultiplier = 1.9f;
    public ForceMode ForceMode = ForceMode.Acceleration;
    public int RbodyUpdatesPerFrame = 1;

    public Rigidbody ConnectedRbody { get; protected set; }
    public override bool IsConnected { get { return ConnectedRbody != null || ConnectedTransform != null; } }

    public override Vector3 CalculatedGlobalAttachPoint {
        get {
            if (ConnectedRbody != null)
                return ConnectedRbody.transform.TransformPoint(AttachPointOnConnection);
            else
                return base.CalculatedGlobalAttachPoint;
        }
    }
    public Vector3 CurrentRbodyForce { get; private set; }

    public override IEnumerable<Vector3> LinePoints
    {
        get
        {
            if (ConnectedRbody != null)
            {
                foreach (Vector3 point in base.LinePoints)
                {
                    yield return point;
                }
                yield return CalculatedGlobalAttachPoint;
            } else
            {
                foreach (Vector3 point in base.LinePoints)
                {
                    yield return point;
                }
            }
        }
    }

    protected override void Awake()
    {
        // resolve Rbody/Transform attachment conflicts
        if (ConnectedRbody != null && ConnectedTransform != null)
        {
            ConnectedTransform = null;
        }

        base.Awake();

        if (ConnectedRbody != null) { AttachRbody(ConnectedRbody); }       
    }

    protected override void UpdateConstraints()
    {
        // TODO this is a hack :/
        if (ConnectedRbody != null && !_allowLinePullSpawn)
        {
            for (int i = 0; i < RbodyUpdatesPerFrame; i++)
                ConstrainRigidBody();
        }
        else
        {
            CurrentRbodyForce = Vector3.zero;
        }

        base.UpdateConstraints();


        // TODO this is a hack :/
        if (ConnectedRbody != null && !_allowLinePullSpawn)
        {
            for (int i = 0; i < RbodyUpdatesPerFrame; i++)
                ConstrainRigidBody();
        } else
        {
            CurrentRbodyForce = Vector3.zero;
        }
    }

    protected virtual void ConstrainRigidBody()
    {
        Vector3 distanceVector = LastParticle.Position - CalculatedGlobalAttachPoint;
        float distance = distanceVector.magnitude;

        if (distance > SegmentLength)
        {
            Vector3 constraintVector = (distanceVector * distance.RatioOfExcess(SegmentLength)) / 2;

            var forceVector = constraintVector * Mathf.Pow(ForceMultiplier, 10);
            CurrentRbodyForce = forceVector;
            //print(Mathf.Round(CurrentRbodyForce.magnitude));

            ConnectedRbody.AddForceAtPosition(CurrentRbodyForce, CalculatedGlobalAttachPoint, ForceMode);
        }
    }

    public virtual void AttachRbody(Rigidbody rbody)
    {
        if (ConnectedTransform != null) { DetachTransform(); }
        if (ConnectedRbody != null) { DetachRbody(); }

        AlignLineWithPoint(rbody.position);
        ConnectedRbody = rbody;
    }

    public override void AttachTransform(Transform transform)
    {
        if (ConnectedRbody != null) { DetachRbody(); }
        base.AttachTransform(transform);
    }

    public virtual void DetachRbody()
    {
        ConnectedRbody = null;
    }
}
