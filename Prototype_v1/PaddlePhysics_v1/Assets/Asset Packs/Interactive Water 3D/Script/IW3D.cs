using UnityEngine;

[RequireComponent(typeof(Camera))]
public class IW3D : MonoBehaviour
{
	[Header("Water Plane")]
	public GameObject m_WaterPlane;
	public int m_Grid = 64;
    public float m_Width = 10f;
	public int m_Resolution = 128;
	public LayerMask m_WaterForceLayer;
	[Header("Water Material")]
	public Material m_MatShading;
	public Material m_MatSimulation;
	public Material m_MatHeightToNormal;
	public bool m_UseVTF = true;
	public Color m_Diffuse = new Color (0f, 0.5f, 1f, 1f);
	public Color m_Specular = Color.white;
	[Range(0f, 1f)] public float m_Damping = 0.99f;
	[Range(0.01f, 0.5f)] public float m_HeightScale = 0.3f;
	[Header("Internal Maps")]
	public RenderTexture[] m_RTHeightmaps = { null, null, null };
    public RenderTexture m_RTNormal;
	public RenderTexture m_RTReflection;
	public RenderTexture m_RTRefraction;
	public bool m_ShowInternalMaps = false;
	
	private Camera m_CamOrth;
	private Camera m_CamReflection;
	private Camera m_CamRefraction;
	private float m_ClipPlaneOffset = 0.07f;
	
	void Start ()
	{
		m_WaterForceLayer = 1 << LayerMask.NameToLayer ("Ignore Raycast");
		IW3DUtils.CreateGridPlane (m_WaterPlane, m_MatShading, m_Grid, m_Width, m_Width, new Vector3(-m_Width / 2f, 0, -m_Width / 2f));
		m_CamOrth = IW3DUtils.CreateOrthographicCamera (m_WaterPlane.transform, m_WaterForceLayer);
		CreateInternalMaps ();
		CreateInternalCameras (gameObject.transform);
		m_CamOrth.targetTexture = m_RTHeightmaps[1];
	}
	void Awake ()
	{
	}
	void Update ()
	{
		if (!enabled)
			return;
		
		// reflection and refraction rendering
		Camera cam = GetComponent<Camera>();
		if (cam)
		{
			IW3DUtils.SyncCameraParameters (cam, m_CamReflection);
			IW3DUtils.SyncCameraParameters (cam, m_CamRefraction);
			RenderToReflectionMap ();
			RenderToRefractionMap ();
		}
		
		// ripple
		UpdateCore ();
		UpdateGPUParameters ();
	}
	void RenderToReflectionMap ()
	{
		// reflection plane's position and normal in world space
		Vector3 pos = m_WaterPlane.transform.position;
		Vector3 normal = m_WaterPlane.transform.up;
			
		// Reflect camera around reflection plane
		float d = -Vector3.Dot (normal, pos) - m_ClipPlaneOffset;
		Vector4 reflectionPlane = new Vector4 (normal.x, normal.y, normal.z, d);
		
		Matrix4x4 reflection = Matrix4x4.zero;
		IW3DUtils.CalculateReflectionMatrix (ref reflection, reflectionPlane);
		Camera cam = GetComponent<Camera> ();
		Vector3 oldpos = cam.transform.position;
		Vector3 newpos = reflection.MultiplyPoint (oldpos);
		m_CamReflection.worldToCameraMatrix = cam.worldToCameraMatrix * reflection;
		
		// render objects up side of water plane
		Vector4 clipPlane = IW3DUtils.CameraSpacePlane (m_CamReflection, pos, normal, 1f, m_ClipPlaneOffset);
		m_CamReflection.projectionMatrix = cam.CalculateObliqueMatrix (clipPlane);
		GL.SetRevertBackfacing (true);
		m_CamReflection.transform.position = newpos;
		Vector3 euler = cam.transform.eulerAngles;
		m_CamReflection.transform.eulerAngles = new Vector3 (-euler.x, euler.y, euler.z);
		m_CamReflection.Render ();
		m_CamReflection.transform.position = oldpos;
		GL.SetRevertBackfacing (false);
	}
	void RenderToRefractionMap ()
	{
		Vector3 pos = m_WaterPlane.transform.position;
		Vector3 normal = m_WaterPlane.transform.up;
		
		Camera cam = GetComponent<Camera> ();
		m_CamRefraction.worldToCameraMatrix = cam.worldToCameraMatrix;
		
		// render objects bottom side of water plane
		Vector4 clipPlane = IW3DUtils.CameraSpacePlane (m_CamRefraction, pos, normal, -1f, m_ClipPlaneOffset);
		m_CamRefraction.projectionMatrix = cam.CalculateObliqueMatrix (clipPlane);
		m_CamRefraction.transform.position = cam.transform.position;
		m_CamRefraction.transform.rotation = cam.transform.rotation;
		m_CamRefraction.Render ();
	}
	void OnGUI ()
	{
		GUI.Box (new Rect (10, 10, 240, 25), "Interactive Water 3D Demo");
		if (m_ShowInternalMaps)
		{
			GUI.DrawTextureWithTexCoords (new Rect (10, 10, 128, 128), m_RTHeightmaps[0], new Rect (0, 0, 1, 1));
			GUI.DrawTextureWithTexCoords (new Rect (148, 10, 128, 128), m_RTHeightmaps[1], new Rect (0, 0, 1, 1));
			GUI.DrawTextureWithTexCoords (new Rect (286, 10, 128, 128), m_RTHeightmaps[2], new Rect (0, 0, 1, 1));
			GUI.DrawTextureWithTexCoords (new Rect (424, 10, 128, 128), m_RTNormal, new Rect (0, 0, 1, 1));
			GUI.DrawTextureWithTexCoords (new Rect (10, 148, 128, 128), m_RTReflection, new Rect (0, 0, 1, 1));
			GUI.DrawTextureWithTexCoords (new Rect (148, 148, 128, 128), m_RTRefraction, new Rect (0, 0, 1, 1));
		}
	}
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	void CreateInternalMaps ()
	{
		m_RTHeightmaps[0] = new RenderTexture (m_Resolution, m_Resolution, 0, RenderTextureFormat.ARGB32);
		m_RTHeightmaps[0].wrapMode = TextureWrapMode.Clamp;
		m_RTHeightmaps[0].name = "Pre Frame Height";
		m_RTHeightmaps[0].autoGenerateMips = false;
		m_RTHeightmaps[0].isPowerOfTwo = true;

		m_RTHeightmaps[1] = new RenderTexture (m_Resolution, m_Resolution, 0, RenderTextureFormat.ARGB32);
		m_RTHeightmaps[1].wrapMode = TextureWrapMode.Clamp;
		m_RTHeightmaps[1].name = "Curr Frame Height";
		m_RTHeightmaps[1].autoGenerateMips = false;
		m_RTHeightmaps[1].isPowerOfTwo = true;

		m_RTHeightmaps[2] = new RenderTexture (m_Resolution, m_Resolution, 0, RenderTextureFormat.ARGB32);
		m_RTHeightmaps[2].wrapMode = TextureWrapMode.Clamp;
		m_RTHeightmaps[2].name = "Next Frame Height";
		m_RTHeightmaps[2].autoGenerateMips = false;
		m_RTHeightmaps[2].isPowerOfTwo = true;

		m_RTNormal = new RenderTexture (m_Resolution, m_Resolution, 0, RenderTextureFormat.ARGB32);
		m_RTNormal.wrapMode = TextureWrapMode.Clamp;
		m_RTNormal.name = "Normal Map";
		m_RTNormal.autoGenerateMips = false;
		m_RTNormal.isPowerOfTwo = true;

		// initialize height maps
		RenderTexture mainRTT = RenderTexture.active;
		RenderTexture.active = m_RTHeightmaps[0];
		GL.Clear (false, true, new Color (0, 0, 0, 0));
		RenderTexture.active = m_RTHeightmaps[1];
		GL.Clear (false, true, new Color (0, 0, 0, 0));
		RenderTexture.active = m_RTHeightmaps[2];
		GL.Clear (false, true, new Color (0, 0, 0, 0));
		RenderTexture.active = mainRTT;

		m_RTReflection = new RenderTexture (256, 256, 16);
		m_RTReflection.name = "Reflection";
		m_RTReflection.autoGenerateMips = false;
		m_RTReflection.isPowerOfTwo = true;

		m_RTRefraction = new RenderTexture (256, 256, 16);
		m_RTRefraction.name = "Refraction";
		m_RTRefraction.autoGenerateMips = false;
		m_RTRefraction.isPowerOfTwo = true;
	}
	void CreateInternalCameras (Transform parent)
	{
		// reflection camera
		GameObject obj = new GameObject ("Reflection", typeof (Camera), typeof (Skybox));
		obj.transform.parent = parent;
		m_CamReflection = obj.GetComponent<Camera>();
		m_CamReflection.enabled = false;
		m_CamReflection.transform.position = transform.position;
		m_CamReflection.transform.rotation = transform.rotation;
		m_CamReflection.targetTexture = m_RTReflection;
		m_CamReflection.cullingMask = ~(1 << LayerMask.NameToLayer("Water"));  // not render water self
		
		// refraction camera
		obj = new GameObject ("Refraction", typeof (Camera), typeof (Skybox));
		obj.transform.parent = parent;
		m_CamRefraction = obj.GetComponent<Camera>();
		m_CamRefraction.enabled = false;
		m_CamRefraction.transform.position = transform.position;
		m_CamRefraction.transform.rotation = transform.rotation;
		m_CamRefraction.targetTexture = m_RTRefraction;
		m_CamRefraction.cullingMask = ~(1 << LayerMask.NameToLayer("Water"));  // not render water self
	}
	Vector4 UpdateWaveEquation (float u, float c, float d, float t)
	{
		float k = c * c * t * t / (d * d);
		float k1 = (4 - 8 * k) / (u * t + 2);
		float k2 = (u * t - 2) / (u * t + 2);
		float k3 = (2 * k) / (u * t + 2);
		return new Vector4 (k1, k2, k3, 0);
	}
	void UpdateGPUParameters ()
	{
		m_MatSimulation.SetTexture ("_HeightPrevTex", m_RTHeightmaps[0]);
		m_MatSimulation.SetTexture ("_HeightCurrTex", m_RTHeightmaps[1]);
		m_MatSimulation.SetFloat ("_Damping", m_Damping);
		m_MatSimulation.SetVector ("_Parameters", UpdateWaveEquation (0.15f, 0.15f, 1f / m_Resolution, 0.03f));
		
		Matrix4x4 matProj = GL.GetGPUProjectionMatrix (m_CamOrth.projectionMatrix, true);
		Matrix4x4 matSVP = matProj * m_CamOrth.worldToCameraMatrix;
		m_MatShading.SetMatrix ("_OrthCamViewProj", matSVP);
		m_MatShading.SetTexture ("_NormalMap", m_RTNormal);
		m_MatShading.SetTexture ("_HeightMap", m_RTHeightmaps[1]);
		m_MatShading.SetFloat ("_HeightScale", m_HeightScale);
		m_MatShading.SetTexture ("_ReflectionMap", m_RTReflection);
		m_MatShading.SetTexture ("_RefractionMap", m_RTRefraction);
		m_MatShading.SetColor ("_Diffuse", m_Diffuse);
		m_MatShading.SetColor ("_Specular", m_Specular);
		if (m_UseVTF)
			m_MatShading.EnableKeyword ("IW3D_USE_VTF");
		else
			m_MatShading.DisableKeyword ("IW3D_USE_VTF");
	}
	void UpdateCore ()
	{
		// wave equation
		m_RTHeightmaps[2].DiscardContents();
		Graphics.Blit (m_RTHeightmaps[0], m_RTHeightmaps[2], m_MatSimulation);
		
		// generate normal map based on height map
		m_RTNormal.DiscardContents();
		Graphics.Blit (m_RTHeightmaps[1], m_RTNormal, m_MatHeightToNormal);

		// swap height map
		m_CamOrth.targetTexture = m_RTHeightmaps[1];
		RenderTexture temp = m_RTHeightmaps[0];
		m_RTHeightmaps[0] = m_RTHeightmaps[1];
		m_RTHeightmaps[1] = m_RTHeightmaps[2];
		m_RTHeightmaps[2] = temp;
	}
}
