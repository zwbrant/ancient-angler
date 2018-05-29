using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualizeFishingForces : MonoBehaviour {
    public RbodyVerletLine FishingLine;
    public RbodyFishResistance Fish;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public Material mat;
    void OnPostRender()
    {
        float lineForce = FishingLine.CurrentRbodyForce.magnitude * .1f;
        float fishForce = Fish.CurrentForce.magnitude * .1f;

        //if (lineForce + fishForce > 10)
        //{
        //    FishingLine.DettachRbody();
        //}

        if (!mat)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things. In this case, we just want to use
            // a blend mode that inverts destination colors.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            mat = new Material(shader);
            mat.hideFlags = HideFlags.HideAndDontSave;
            // Set blend mode to invert destination colors.
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusDstColor);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            // Turn off backface culling, depth writes, depth test.
            mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            mat.SetInt("_ZWrite", 0);
            mat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
        }
        GL.PushMatrix();
        GL.LoadIdentity();
        GL.LoadProjectionMatrix(Camera.main.projectionMatrix);
        GL.modelview = Camera.main.worldToCameraMatrix;
        GL.Viewport(Camera.main.pixelRect);

        // activate the first shader pass (in this case we know it is the only pass)
        mat.SetPass(0);
        // draw a quad over whole screen
        GL.Begin(GL.QUADS);
        GL.Color(Color.red);
        GL.Vertex3(-5f, 0, 0);
        GL.Vertex3(lineForce - 5f, 0, 0);
        GL.Vertex3(lineForce - 5f, 1, 0);
        GL.Vertex3(-5f, 1, 0);

        GL.Color(Color.blue);
        GL.Vertex3(-5f, -.5f, 0);
        GL.Vertex3(fishForce - 5f, -.5f, 0);
        GL.Vertex3(fishForce - 5f, -1.5f, 0);
        GL.Vertex3(-5f, -1.5f, 0);

        GL.End();

        GL.PopMatrix();
    }

    
}
