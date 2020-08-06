using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using VRTK;
using Vec3 = UnityEngine.Vector3;
using System.Collections.Generic;
using UnityStandardAssets.ImageEffects;
using UnityEngine.UI;
using LostPolygon.DynamicWaterSystem;


/* Author: Zane Brant
    
    Provides information about the paddle it's attached to (represented by four points). 
    Should be used by a PaddleForceManager to calculate forces from Paddles and distribute them. 

*/

public class ZB_PaddleBlade : MonoBehaviour
{
    const float WaterHeight = 0f;
    // how many frames of data should be averaged for the velocity vector?
    public int VelocityFrameBuffer = 4;

    public GameObject[] CornerGOs = new GameObject[4];
    public GameObject ForcePoint;
    public Rigidbody BladeRbody;
    public BuoyancyForce BForce;
    [Range(0f, 10f)]
    public float MinBForce;
    [Range(0f, 50f)]
    public float MaxBForce;


    [Header("Debugging")]
    public bool DebugSubmersion = false;
    public Color SubmergLineColor = Color.red;
    public Color OutlineColor = Color.cyan;
    public Color VelocityLineColor = Color.blue;
    public TextMesh DebugText;

    [HideInInspector]
    public List<Vec3> SubmergedPoints = new List<Vec3>();
    private Vec3[] cornerVectors = new Vec3[4];
    private Vec3 velocityVector = new Vec3(0, 0, 0);

    private Material _meshMaterial;

    internal VRTK_ControllerActions mainControlActions = null;
    internal VRTK_ControllerActions secondaryControlActions = null;
    internal ZB_PaddleForceManager associatedBoat = null;



    #region Properties
    public static Material LineMaterial
    {
        get
        {
            if (_lineMaterial == null)
            {
                // Unity has a built-in shader that is useful for drawing
                // simple colored things.
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                _lineMaterial = new Material(shader);
                _lineMaterial.hideFlags = HideFlags.HideAndDontSave;
                // Turn on alpha blending
                _lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                _lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                // Turn backface culling off
                _lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                // Turn off depth writes
                _lineMaterial.SetInt("_ZWrite", 0);
            }
            return _lineMaterial;
        }
    }
    private static Material _lineMaterial;
    public Vec3 Direction { get { return velocityVector.normalized; } }
    public float Speed { get { return velocityVector.magnitude; } }
    public float SubmergedArea
    {
        get
        {
            int cornersSubmerged = CornerVectors.Count(v => v.y < WaterHeight);

            switch (cornersSubmerged)
            {
                case 1:
                    return GetAreaOneSubmergCorner();
                case 2:
                    return GetAreaTwoSubmergCorner();
                case 3:
                    return GetAreaThreeSubmergCorner();
                case 4:
                    return GetAreaFourSubmergCorner();
                default:
                    SubmergedPoints.Clear();
                    if (DebugSubmersion)
                    {
                        BladeMesh.triangles = null;
                        BladeMesh.vertices = null;

                    }
                    return 0f;
            }

        }
    }

    public Vec3[] CornerVectors
    {
        get
        {
            for (int i = 0; i < CornerGOs.Length; i++)
                cornerVectors[i] = CornerGOs[i].transform.position; 
            return cornerVectors;
        }
    }


    public Vec3[] CornersByDepth
    {
        get
        {
            return CornerVectors.OrderBy(v => v.y).ToArray<Vec3>();
        }
    }

    public Vec3 RawForce
    {
        get
        {
            float thrust = Speed * SubmergedArea;
            float angleOfImpact = Vec3.Angle(transform.up, Direction);


            float dot = Vec3.Dot(transform.InverseTransformVector(velocityVector).normalized,BladeNormal);
            float obliqueness = Mathf.Abs(dot);
            //print(obliqueness);

            thrust *= obliqueness;

            Vec3 globalVector = transform.TransformVector(BladeNormal).normalized;

            //return -Direction * thrust;

            BForce.SplashForceFactor = Mathf.Lerp(MinBForce, MaxBForce, obliqueness);
            BForce.RecalculateCache();

            if (DebugSubmersion)
            {
                var speedFactor = Mathf.Clamp(Speed / 3f, 0f, 1f);
                var newColor = Color.Lerp(Color.green, Color.red, obliqueness * speedFactor);
                _currMeshColor = Color.Lerp(_currMeshColor, newColor, .1f);

                _meshMaterial.SetColor("_Color", _currMeshColor);

                if (DebugText)
                    DebugText.text = Math.Round(BForce.SplashForceFactor, 2, MidpointRounding.AwayFromZero).ToString();
            }

            if (dot > 0f)
                return -globalVector * thrust;
            else
                return globalVector * thrust;
        }
    }

    private Color _currMeshColor = Color.green;

    public float Height
    {
        get {
            if (_height < 0f)
                _height = Vec3.Distance(CornerVectors[0], CornerVectors[2]);
            return _height;
        }

    }
    private float _height = -1f;

    public float Width
    {
        get
        {
            if (_width < 0f)
                _width = Vec3.Distance(CornerVectors[0], CornerVectors[1]);
            return _width;
        }

    }
    private float _width = -1f;

    public float SurfaceArea { 
        get
        {
            if (_surfaceArea < 0f)
                _surfaceArea = Width * Height;
            return _surfaceArea;
        }
    }
    private float _surfaceArea = -1f;

    public Vec3 BladeNormal
    {
        get
        {
            //return Vec3.Normalize(
            //    Vec3.Cross(CornerGOs[0].transform.localPosition, CornerGOs[1].transform.localPosition));
            return Vec3.right;
        }
    }

    public Mesh BladeMesh
    {
        get
        {
            if (_bladeMesh == null)
            {
                GetComponent<MeshFilter>().mesh = _bladeMesh = new Mesh();
                _bladeMesh.name = "BladeMesh";
                _meshMaterial = GetComponent<MeshRenderer>().material;

            }
            return _bladeMesh;
        }

    }
    private Mesh _bladeMesh;

    #endregion

    private float _initSplashForce;

    void Start()
    {
        previousPos = transform.position;
        //print(name + " surf area: " + SurfaceArea);
        _meshMaterial = GetComponent<MeshRenderer>().material;
        //_meshMaterial.SetColor("_Color", Color.blue);
        if (BForce)
            _initSplashForce = BForce.SplashForceFactor;
    }

    Vec3 previousPos;

    void FixedUpdate()
    {
        SetVelocityVector();

        if (DebugSubmersion)
        {
            var cat = RawForce;

        }
    }

    private void OnRenderObject()
    {
        if (!DebugSubmersion)
            return;

        LineMaterial.SetPass(0);
        GL.PushMatrix();
        GL.Begin(GL.LINES);

        //GL.Color(SubmergLineColor);
        //if (SubmergedPoints.Count > 0)
        //{
        //    for (int i = 0; i < SubmergedPoints.Count; i++)
        //    {
        //        if (i != 0)
        //            GL.Vertex(SubmergedPoints[i]);

        //        GL.Vertex(SubmergedPoints[i]);
        //    }
        //    GL.Vertex(SubmergedPoints[0]);
        //}

        GL.Color(OutlineColor);
        GL.Vertex(CornerVectors[0]);
        GL.Vertex(CornerVectors[1]);
        GL.Vertex(CornerVectors[1]);
        GL.Vertex(CornerVectors[2]);
        GL.Vertex(CornerVectors[2]);
        GL.Vertex(CornerVectors[3]);
        GL.Vertex(CornerVectors[3]);
        GL.Vertex(CornerVectors[0]);



        //GL.Vertex(transform.position);
        //GL.Vertex(transform.position - RawForce * .3f);

        GL.Color(VelocityLineColor);

        GL.Vertex(transform.position);

        var newLine = Vec3.Lerp(_prevVelLine, velocityVector, .1f);

        GL.Vertex(transform.position - newLine * .3f);
        _prevVelLine = velocityVector;
        //GL.Vertex(transform.position);
        //GL.Vertex(transform.TransformPoint(BladeNormal));

        GL.End();
        GL.PopMatrix();
    }

    int frameCount = 0;
    Vec3 cumulativeVelocity = new Vec3();
    private Vec3 _prevVelLine;
    void SetVelocityVector()
    {

        if (frameCount < VelocityFrameBuffer)
        {
            if (BladeRbody != null)
            {
                cumulativeVelocity += BladeRbody.velocity;

            } else
            {
                cumulativeVelocity += (transform.position - previousPos) / Time.deltaTime;
                previousPos = transform.position;
            }
            frameCount++;
        }
        else
        {
            velocityVector = cumulativeVelocity / VelocityFrameBuffer;

            frameCount = 0;
            cumulativeVelocity = Vec3.zero;
        }
        
    }

    public static Vec3 CrossProduct(Vec3 a, Vec3 b)
    {
        Vec3 c = new Vec3(0, 0, 0);
        c.x = (a.y * b.z) - (a.z - b.y);
        c.y = (a.z * b.x) - (a.x - b.z);
        c.z = (a.x * b.y) - (a.y * b.x);

        return c;
    }

    public static Vec3 GetWaterIntersectPoint(Vec3 p0, Vec3 p1)
    {
        Vec3 tSlope = p1 - p0;

        float tProd = tSlope.x + tSlope.y + tSlope.z;
        float coEfProd = p0.x + p0.y + p0.z;

        float t = 0f - p0.y;
        t = (t / tSlope.y);

        Vec3 solution = new Vec3();

        solution.x = (tSlope.x * t) + p0.x;
        solution.y = (tSlope.y * t) + p0.y;
        solution.z = (tSlope.z * t) + p0.z;


        return solution;
    }

    public float GetAreaOneSubmergCorner()
    {
        Vec3[] corners = CornersByDepth;

        Vec3 dHit = GetWaterIntersectPoint(corners[0], corners[2]);
        Vec3 sHit = GetWaterIntersectPoint(corners[0], corners[1]);

        float submergBase = Math.Abs(Vec3.Distance(corners[0], dHit));
        float submergHeight = Math.Abs(Vec3.Distance(corners[0], sHit));
        float submergArea = (submergBase * submergHeight) / 2f;

        if (DebugSubmersion)
        {
            SubmergedPoints.Clear();
            SubmergedPoints.Add(dHit);
            SubmergedPoints.Add(corners[0]);
            SubmergedPoints.Add(sHit);

            RedrawMesh(1);

        }

        return submergArea;
    }

    public float GetAreaTwoSubmergCorner()
    {
        Vec3[] corners = CornersByDepth;

        Vec3 dHit = GetWaterIntersectPoint(corners[0], corners[2]);
        Vec3 sHit = GetWaterIntersectPoint(corners[1], corners[3]);

        float rightTriHeight = Vec3.Distance(corners[1], sHit);
        float rightTriArea = (Width * rightTriHeight) / 2f;

        float obliqueA = Vec3.Distance(corners[0], dHit);
        float obliqueB = Vec3.Distance(corners[0], sHit);
        float obliqueC = Vec3.Distance(sHit, dHit);
        float obliqueArea = ZB_Math.GetTriArea(obliqueA, obliqueB, obliqueC);

        if (DebugSubmersion)
        {
            SubmergedPoints.Clear();
            SubmergedPoints.Add(dHit);
            SubmergedPoints.Add(corners[0]);
            SubmergedPoints.Add(corners[1]);
            SubmergedPoints.Add(sHit);

            RedrawMesh(2);

        }

        return rightTriArea + obliqueArea;
    }

    public float GetAreaThreeSubmergCorner()
    {
        Vec3[] corners = CornersByDepth;

        Vec3 dHit = GetWaterIntersectPoint(corners[1], corners[3]);
        Vec3 sHit = GetWaterIntersectPoint(corners[2], corners[3]);

        float sRightTriHeight = Vec3.Distance(corners[2], sHit);
        float sRightTriArea = (sRightTriHeight * Height) / 2f;

        float dRightTriHeight = Vec3.Distance(corners[1], dHit);
        float dRightTriArea = (dRightTriHeight * Width) / 2f;

        float obliqueA = Vec3.Distance(corners[0], sHit);
        float obliqueB = Vec3.Distance(corners[0], dHit);
        float obliqueC = Vec3.Distance(sHit, dHit);
        float obliqueArea = ZB_Math.GetTriArea(obliqueA, obliqueB, obliqueC);

        if (DebugSubmersion)
        {
            SubmergedPoints.Clear();
            SubmergedPoints.Add(dHit);
            SubmergedPoints.Add(corners[1]);
            SubmergedPoints.Add(corners[0]);
            SubmergedPoints.Add(corners[2]);
            SubmergedPoints.Add(sHit);

            RedrawMesh(3);

        }

        return obliqueArea + dRightTriArea + sRightTriArea;
    }

    public float GetAreaFourSubmergCorner()
    {
        if (DebugSubmersion)
        {
            SubmergedPoints.Clear();
            SubmergedPoints.Add(CornerVectors[0]);
            SubmergedPoints.Add(CornerVectors[1]);
            SubmergedPoints.Add(CornerVectors[2]);
            SubmergedPoints.Add(CornerVectors[3]);

            RedrawMesh(4);

        }
        return SurfaceArea;
    }

    public void RedrawMesh(int submergedCorners)
    {
        BladeMesh.triangles = null;

        if (submergedCorners < 1)
        {
            var verts = new Vec3[4];

            for (int i = 0; i < CornerGOs.Length; i++)
                verts[i] = CornerGOs[i].transform.localPosition;

            BladeMesh.vertices = verts;

            if (BladeMesh.triangles.Length != 6)
                BladeMesh.triangles = new int[6] { 0, 1, 3, 1, 2, 3 };

        }
        else if (submergedCorners == 1)
        {
            var verts = new Vec3[SubmergedPoints.Count];

            for (int i = 0; i < SubmergedPoints.Count; i++)
                verts[i] = transform.InverseTransformPoint(SubmergedPoints[i]);

            BladeMesh.vertices = verts;

            BladeMesh.triangles = new int[3] { 0, 1, 2};
        } 
        else if (submergedCorners == 2)
        {
            var verts = new Vec3[SubmergedPoints.Count];

            for (int i = 0; i < SubmergedPoints.Count; i++)
                verts[i] = transform.InverseTransformPoint(SubmergedPoints[i]);

            BladeMesh.vertices = verts;

            BladeMesh.triangles = new int[6] { 0, 3, 1, 3, 2, 1 };
        }
        else if (submergedCorners == 3)
        {
            var verts = new Vec3[SubmergedPoints.Count];

            for (int i = 0; i < SubmergedPoints.Count; i++)
                verts[i] = transform.InverseTransformPoint(SubmergedPoints[i]);

            BladeMesh.vertices = verts;

            BladeMesh.triangles = new int[9] { 0, 2, 1, 0, 4, 2, 4, 3, 2 };
        }

        else if (submergedCorners == 4)
        {
            var verts = new Vec3[SubmergedPoints.Count];

            for (int i = 0; i < SubmergedPoints.Count; i++)
                verts[i] = transform.InverseTransformPoint(SubmergedPoints[i]);

            BladeMesh.vertices = verts;

            BladeMesh.triangles = new int[6] { 0, 1, 2, 0, 2, 3 };
        }

    }
}
