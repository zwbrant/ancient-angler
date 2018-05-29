using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerletLine : MonoBehaviour
{
    [Header("Line Options")]
    public float InitialLineLength = 5f;
    public float SegmentLength = .1f;
    public Transform AnchorPoint;
    [Header("Physics Options")]
    public Vector3 AttachPointOnConnection = Vector3.zero;
    public float Gravity = -10f;
    //public float ConstraintMultiplier = 1f;
    public int VerletUpdatesPerFrame = 6;
    public int ConstraintUpdatesPerFrame = 50;

    public Transform ConnectedTransform { get; protected set; }
    public virtual bool IsConnected { get { return ConnectedTransform != null; } }
    public virtual float LineLength { get { return _firstSegmentLength + (_particles.Count * SegmentLength); } }

    public virtual Vector3 CalculatedGlobalAttachPoint
    {
        get
        {
            if (ConnectedTransform != null)
                return ConnectedTransform.TransformPoint(AttachPointOnConnection);
            else
                return LastParticle.Position;
        }
    }

    public virtual IEnumerable<Vector3> LinePoints
    {
        get
        {
            yield return AnchorPoint.position;
            for (int i = 0; i < _particles.Count; i++)
            {
                yield return _particles[i].Position;
            }
            if (ConnectedTransform != null)
            {
                yield return CalculatedGlobalAttachPoint;
            }
        }
    }
    public virtual VerletParticle LastParticle { get { return _particles[_particles.Count - 1]; } }

    protected List<VerletParticle> _particles;
    protected float _firstSegmentLength = 0f;
    protected bool _allowLinePullSpawn = false;
    protected float _lineSpawnPullThreshold = 0f;



    protected virtual void Awake()
    {
        if (AnchorPoint == null) { AnchorPoint = transform; }
        _particles = new List<VerletParticle>();

        // calculate minimum line length based on segment length
        if (InitialLineLength < SegmentLength) { InitialLineLength = SegmentLength; }

        // add initial particles
        AddLine(InitialLineLength);

        if (ConnectedTransform != null) { AttachTransform(ConnectedTransform); }
    }

    protected virtual void Update()
    {
        if (_isAddingLine)
        {
            UpdateAddLine();
        }
    }

    protected virtual void FixedUpdate()
    {
        UpdateParticleVerlets();
        UpdateConstraints();
    }

    private void OnGUI()
    {
        float amountToAdd = 0f;
        amountToAdd = EzGui.FloatField(amountToAdd, 0, 0);
        if(EzGui.Button("ADD LINE", 0, 1))
        {
            AddLine(amountToAdd);
        }

        float spawnThreshold = 0f;
        spawnThreshold = EzGui.FloatField(spawnThreshold, 1, 0);
        if (EzGui.Button("TOGGLE LINE RELEASE", 1, 1))
        {
            SetLinePullSpawn(!_allowLinePullSpawn, spawnThreshold);
        }

        float addRate = 0f;
        addRate = EzGui.FloatField(addRate, 2, 0);
        if (EzGui.Button("START ADDING LINE", 2, 1))
        {
            StartAddingLine(addRate);
        }

        float amountToRemove = 0f;
        amountToRemove = EzGui.FloatField(amountToRemove, 3, 0);
        if (EzGui.Button("REMOVE LINE", 3, 1))
        {
            RemoveLine(amountToRemove);
        }


    }

    protected virtual void UpdateParticleVerlets()
    {
        for (int i = 0; i < VerletUpdatesPerFrame; i++)
        {
            for (int j = 0; j < _particles.Count; j++)
            {
                UpdateVerlet(_particles[j]);
            }
        }
    }

    protected virtual void UpdateConstraints()
    {
        for (int i = 0; i < ConstraintUpdatesPerFrame; i++)
        {
            // first segment
            ConstrainParticleToPoint(_particles[0], AnchorPoint.position, 0, _firstSegmentLength);

            for (int j = 0; j < _particles.Count - 1; j++)
            {
                ConstrainParticles(_particles[j], _particles[j + 1], j + 1);
            }

            // attachment at end
            if (IsConnected)
            {
                ConstrainParticleToPoint(_particles[_particles.Count - 1], CalculatedGlobalAttachPoint, _particles.Count);
            }
        }
    }

    protected virtual void UpdateVerlet(VerletParticle particle)
    {
        Vector3 positionDelta = particle.Position - particle.OldPosition;
        particle.OldPosition = particle.Position;

        // gravity?
        if (particle.Position.y > 0)
            positionDelta.y += Gravity;

        particle.Position += positionDelta * Mathf.Pow(Time.deltaTime, 2f);
    }

    protected virtual void ConstrainParticles(VerletParticle p1, VerletParticle p2, int insertionIndex)
    {
        ConstrainParticles(p1, p2, insertionIndex, SegmentLength);
    }
    protected virtual void ConstrainParticles(VerletParticle p1, VerletParticle p2, int insertionIndex, float constraintLength)
    {
        Vector3 distanceVector = p1.Position - p2.Position;
        float distance = distanceVector.magnitude;

        if (distance > SegmentLength)
        {
            float difference = distance - constraintLength;
            //do we add a new particle to handle the delta or move the old one ?
            if (_allowLinePullSpawn && difference > _lineSpawnPullThreshold)
            {
                AddNewParticle(insertionIndex, (p1.Position + p2.Position) / 2);
            }
            else
            {
                // reduce the distance vector to the length that is excess and divide by two since we're applying to both particles
                Vector3 constraintVector = (distanceVector * distance.RatioOfExcess(SegmentLength)) / 2;

                p1.Position -= constraintVector;
                p2.Position += constraintVector;
            }
        }
    }



    protected virtual void ConstrainParticleToPoint(VerletParticle p1, Vector3 point, int insertionIndex)
    {
        ConstrainParticleToPoint(p1, point, insertionIndex, SegmentLength);
    }
    protected virtual void ConstrainParticleToPoint(VerletParticle p1, Vector3 point, int insertionIndex, float constraintLength)
    {
        Vector3 distanceVector = p1.Position - point;
        float distance = distanceVector.magnitude;

        if (distance > constraintLength)
        {
            float difference = distance - constraintLength;

            // do we add a new particle to handle the delta or move the old one?
            if (_allowLinePullSpawn && difference > _lineSpawnPullThreshold)
            {
                AddNewParticle(insertionIndex, (p1.Position + point) / 2);
            } else
            {
                Vector3 constraintVector = (distanceVector * distance.RatioOfExcess(constraintLength)) / 2;

                p1.Position -= constraintVector;
            }
        }
    }

    public virtual void AttachTransform(Transform transform)
    {
        AlignLineWithPoint(CalculatedGlobalAttachPoint);
        ConnectedTransform = transform;
    }

    public virtual void DetachTransform()
    {
        ConnectedTransform = null;
    }

    public virtual void AlignLineWithPoint(Vector3 point)
    {
        var distanceVector = AnchorPoint.position - point;
        for (int i = 0; i < _particles.Count; i++)
        {
            var newPos = AnchorPoint.position - (distanceVector / (_particles.Count + 1)) * (i + 1);
            _particles[i].OldPosition = newPos;
            _particles[i].Position = newPos;
        }
    }

    #region Line Modification functions

    // flag to add line via UpdateAddLine() in Update loop
    protected bool _isAddingLine = false;
    protected float _lineAddRate = 0f;

    public virtual void StartAddingLine(float metersPerSecond)
    {
        _isAddingLine = true;
        _lineAddRate = metersPerSecond;
    }

    protected virtual void UpdateAddLine()
    {
        float lineToAddThisFrame = _lineAddRate * Time.deltaTime;
        AddLine(lineToAddThisFrame);
    }

    public virtual void StopAddingLine()
    {
        _isAddingLine = false;
    }

    public virtual void AddLine(float meters)
    {
        int newParticleCount = Mathf.FloorToInt(meters / SegmentLength);
        float remainder = (meters % SegmentLength) + _firstSegmentLength;

        // if the remainder is greater than a whole segment, remove that much and add one to the new segment count
        if (remainder >= SegmentLength)
        {
            newParticleCount++;
            remainder = remainder - SegmentLength;
        } 
        _firstSegmentLength = remainder;
        
        // add new particles
        for (int i = 0; i < newParticleCount; i++)
        {
            Vector3 particleSpawnPosition;
            if (_particles.Count < 1)
            {
                if (ConnectedTransform != null)
                    particleSpawnPosition = (ConnectedTransform.position + AnchorPoint.position) / 2;
                else
                    particleSpawnPosition = AnchorPoint.position;
            } else
            {
                particleSpawnPosition = (_particles[0].Position + AnchorPoint.position) / 2;
            }

            AddNewParticle(0, particleSpawnPosition);
        }
    }

    public virtual void RemoveLine(float meters)
    {

        int segmentRemovalCount = Mathf.FloorToInt(meters / SegmentLength);
        float remainder = meters % SegmentLength;

        if (segmentRemovalCount > _particles.Count - 1)
        {
            segmentRemovalCount = _particles.Count - 1;
            remainder = 0f;
        } else
        {
            if (_firstSegmentLength - remainder < 0)
            {
                segmentRemovalCount++;
                remainder = SegmentLength + remainder;
            } else
            {
                remainder = _firstSegmentLength - remainder;
            }
        }


        if (segmentRemovalCount > _particles.Count - 1)
        {
            segmentRemovalCount = _particles.Count - 1;
            remainder = 0f;
        }

        _firstSegmentLength = remainder;

        // remove particles
        for (int i = 0; i < segmentRemovalCount; i++)
        {
            RemoveParticle(0);
        }
    }



    protected virtual void AddNewParticle(int insertIndex, Vector3 position)
    {
        _particles.Insert(insertIndex, new VerletParticle()
        {
            Position = position,
            OldPosition = position
        });
    }

    protected virtual void RemoveParticle(int removalIndex)
    {
        if (_particles.Count > 1)
        {
            _particles.Remove(_particles[removalIndex]);
            Debug.DrawLine(Vector3.zero, _particles[removalIndex].Position, Color.cyan);
        }
    }


    public virtual void SetLinePullSpawn(bool allowLinePullSpawn, float pullThreshold = 1f)
    {
        _allowLinePullSpawn = allowLinePullSpawn;
        _lineSpawnPullThreshold = pullThreshold;
    }

    #endregion

    public class VerletParticle
    {
        public Vector3 Position { get; set; }
        public Vector3 OldPosition { get; set; }
    }

}

