using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using VRTK;
using Vec3 = UnityEngine.Vector3;
using System.Collections.Generic;
using UnityStandardAssets.ImageEffects;


/* Author: Zane Brant
    
    Provides information about the paddle it's attached to (represented by four points). 
    Should be used by a PaddleForceManager to calculate forces from Paddles and distribute them. 

*/

public class ZB_PaddleBlade : MonoBehaviour
{
    const float WaterHeight = 0f;
    // how many frames of data should be averaged for the velocity vector (if any)?
    const int VelocityFrameBuffer = 4;

    public GameObject[] CornerGOs = new GameObject[4];
    public GameObject ForcePoint;

    public bool ShowSubmersionLines = false;
    public Color SubmergLineColor = Color.red;

    [HideInInspector]
    public List<Vec3> SubmergedPoints = new List<Vec3>();
    private Vec3[] cornerVectors = new Vec3[4];
    private Vec3 velocityVector = new Vec3(0, 0, 0);

    internal VRTK_ControllerActions mainControlActions = null;
    internal VRTK_ControllerActions secondaryControlActions = null;
    internal ZB_PaddleForceManager associatedBoat = null;



    #region Properties
    static Material LineMaterial
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
    public float Velocity { get { return velocityVector.magnitude; } }
    public float SubmergedArea
    {
        get
        {
            int cornersSubmerged = CornerVectors.Count(v => v.y < WaterHeight);

            switch (cornersSubmerged)
            {
                case 1:
                    //print(SubmergedAreaTest());
                    //return GetAreaOneSubmergCorner_fast();
                    return GetAreaOneSubmergCorner();
                case 2:
                    //return GetAreaTwoSubmergCorner_fast();
                    return GetAreaTwoSubmergCorner();
                case 3:
                    return GetAreaThreeSubmergCorner();
                case 4:
                    return GetAreaFourSubmergCorner();
                default:
                    SubmergedPoints.Clear();
                    return 0f;
            }

        }
    }

    public Vec3[] CornerVectors
    {
        get
        {
            cornerVectors[0] = CornerGOs[0].transform.position;
            cornerVectors[1] = CornerGOs[1].transform.position;
            cornerVectors[2] = CornerGOs[2].transform.position;
            cornerVectors[3] = CornerGOs[3].transform.position;
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
            float thrust = Velocity * SubmergedArea;
            float angleOfImpact = Vec3.Angle(transform.up, Direction);


            float dot = Vec3.Dot(transform.InverseTransformVector( Direction),BladeNormal);
            float obliqueness = Mathf.Abs(dot);
            //print(obliqueness);

            // normalize between the sides of the paddle
            if (angleOfImpact > 90)
            {
                angleOfImpact = 180 - angleOfImpact;
            }
            // normalize the angle. the greater the angle of impact (obliqueness), the less the intensity 
            float intensityOfImpact = 1f - (angleOfImpact * 0.0111111111111111f);
            thrust *= obliqueness;

            Vec3 globalVector = transform.TransformDirection(BladeNormal);

            //return -Direction * thrust;

            if (dot > 0f)
                return -globalVector * thrust;
            else
                return globalVector * thrust;
        }
    }



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
            return Vec3.Normalize(
                Vec3.Cross(CornerGOs[0].transform.localPosition, CornerGOs[1].transform.localPosition));
        }
    }

    #endregion

    void Start()
    {
        previousPos = transform.position;
    }

    Vec3 previousPos;
    void Update()
    {
        SetVelocityVector();
        previousPos = transform.position;
    }

    private void OnRenderObject()
    {
        if (!ShowSubmersionLines)
            return;

        LineMaterial.SetPass(0);
        GL.PushMatrix();
        GL.Begin(GL.LINES);

        GL.Color(SubmergLineColor);

        if (SubmergedPoints.Count > 0)
        {
            for (int i = 0; i < SubmergedPoints.Count; i++)
            {
                if (i != 0)
                    GL.Vertex(SubmergedPoints[i]);

                GL.Vertex(SubmergedPoints[i]);
            }
            GL.Vertex(SubmergedPoints[0]);
        }
        GL.Color(Color.green);

        GL.Vertex(transform.position);
        GL.Vertex(transform.position - RawForce);

        GL.End();
        GL.PopMatrix();
    }

    int frameCount = 0;
    Vec3 cumulativeVelocity = new Vec3();
    void SetVelocityVector()
    {
        if (frameCount < VelocityFrameBuffer)
        {
            cumulativeVelocity += (transform.position - previousPos) / Time.deltaTime;
            frameCount++;
        }
        else
        {
            velocityVector = cumulativeVelocity / VelocityFrameBuffer;
            frameCount = 0;
            cumulativeVelocity = new Vec3();
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

        if (ShowSubmersionLines)
        {
            SubmergedPoints.Clear();
            SubmergedPoints.Add(dHit);
            SubmergedPoints.Add(corners[0]);
            SubmergedPoints.Add(sHit);
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

        if (ShowSubmersionLines)
        {
            SubmergedPoints.Clear();
            SubmergedPoints.Add(dHit);
            SubmergedPoints.Add(corners[0]);
            SubmergedPoints.Add(corners[1]);
            SubmergedPoints.Add(sHit);
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

        if (ShowSubmersionLines)
        {
            SubmergedPoints.Clear();
            SubmergedPoints.Add(dHit);
            SubmergedPoints.Add(corners[1]);
            SubmergedPoints.Add(corners[0]);
            SubmergedPoints.Add(corners[2]);
            SubmergedPoints.Add(sHit);
        }

        return obliqueArea + dRightTriArea + sRightTriArea;
    }

    public float GetAreaFourSubmergCorner()
    {
        if (ShowSubmersionLines)
        {
            SubmergedPoints.Clear();
            SubmergedPoints.Add(CornerVectors[0]);
            SubmergedPoints.Add(CornerVectors[1]);
            SubmergedPoints.Add(CornerVectors[2]);
            SubmergedPoints.Add(CornerVectors[3]);
        }
        return SurfaceArea;
    }
}
