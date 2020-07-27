using UnityEngine;

public class IW3DUtils
{
	static public void CreateGridPlane (GameObject empty, Material mat, int grid, float w, float h, Vector3 offset)
    {
		Vector2 cellSize = new Vector2(w / (grid - 1), h / (grid - 1));
		Vector3[] vertices = new Vector3[grid * grid];
		Vector2[] uvs = new Vector2[grid * grid];
		for (int r = 0; r < grid; r++)
		{
			for (int c = 0; c < grid; c++)
			{
				vertices[r * grid + c].x = cellSize.x * c + offset.x;
				vertices[r * grid + c].y = 0 + offset.y;
				vertices[r * grid + c].z = cellSize.y * r + offset.z;
				uvs[r * grid + c].x = (float)r / (float)(grid - 1);
				uvs[r * grid + c].y = (float)c / (float)(grid - 1);
			}
		}
		int[] indices = new int[(grid - 1) * (grid - 1) * 6];
		int n = 0;
		for (int r = 0; r < grid - 1 ; r++)
		{
			for (int c = 0; c < grid - 1; c++)
			{
				indices[n] = r * grid + c;
				indices[n + 1] = (r + 1) * grid + c;
				indices[n + 2] = r * grid + c + 1;
				indices[n + 3] = r * grid + c + 1;
				indices[n + 4] = (r + 1) * grid + c;
				indices[n + 5] = (r + 1) * grid + c + 1;
				n += 6;
			}
		}
		Mesh mesh = new Mesh();
		mesh.name = "Grid Plane";
		mesh.vertices = vertices;
		mesh.uv = uvs;
		mesh.triangles = indices;
		MeshFilter filter = empty.AddComponent<MeshFilter>();
		filter.mesh = mesh;
		MeshRenderer rd = empty.AddComponent<MeshRenderer>();
		rd.material = mat;
    }
	static public Camera CreateOrthographicCamera (Transform parent, LayerMask lm)
	{
		GameObject obj = new GameObject ("Orthographic Camera");
		obj.transform.parent = parent;
		obj.transform.localPosition = new Vector3 (0, 0, 0);
		obj.transform.localRotation = new Quaternion (0, 0, 0, 1);

		Camera cam = obj.AddComponent<Camera> ();
		cam.backgroundColor = Color.black;
		cam.useOcclusionCulling = false;
		cam.renderingPath = RenderingPath.Forward;
		cam.enabled = true;
		cam.cullingMask = lm;
		cam.clearFlags = CameraClearFlags.Nothing;
		cam.orthographic = true;
		cam.transform.Rotate (new Vector3 (90, 180, 0), Space.Self);  // look down
		// use near and far clip plane to clip objects that under and upside water plane
		cam.nearClipPlane = -10f;
		cam.farClipPlane = 10f;
		Shader sdr = Shader.Find ("Interactive Water 3D/Push");
		cam.SetReplacementShader (sdr, "");
		return cam;
	}
	static public void SyncCameraParameters (Camera src, Camera dst)
	{
		if (dst == null)
			return;

		if (src.clearFlags == CameraClearFlags.Skybox)
		{
			Skybox sky = src.GetComponent (typeof (Skybox)) as Skybox;
			Skybox mysky = dst.GetComponent (typeof (Skybox)) as Skybox;
			if (!sky || !sky.material)
			{
				mysky.enabled = false;
			}
			else
			{
				mysky.enabled = true;
				mysky.material = sky.material;
			}
		}
		dst.clearFlags = src.clearFlags;
		dst.backgroundColor = src.backgroundColor;
		dst.farClipPlane = src.farClipPlane;
		dst.nearClipPlane = src.nearClipPlane;
		dst.orthographic = src.orthographic;
		dst.fieldOfView = src.fieldOfView;
		dst.aspect = src.aspect;
		dst.orthographicSize = src.orthographicSize;
	}
	static public void CalculateReflectionMatrix (ref Matrix4x4 m, Vector4 plane)
	{
		m.m00 = (1f - 2f * plane[0] * plane[0]);
	    m.m01 = (   - 2f * plane[0] * plane[1]);
	    m.m02 = (   - 2f * plane[0] * plane[2]);
	    m.m03 = (   - 2f * plane[3] * plane[0]);

	    m.m10 = (   - 2f * plane[1] * plane[0]);
	    m.m11 = (1f - 2f * plane[1] * plane[1]);
	    m.m12 = (   - 2f * plane[1] * plane[2]);
	    m.m13 = (   - 2f * plane[3] * plane[1]);
	
    	m.m20 = (   - 2f * plane[2] * plane[0]);
    	m.m21 = (   - 2f * plane[2] * plane[1]);
    	m.m22 = (1f - 2f * plane[2] * plane[2]);
    	m.m23 = (   - 2f * plane[3] * plane[2]);

    	m.m30 = 0f;
    	m.m31 = 0f;
    	m.m32 = 0f;
    	m.m33 = 1f;
	}
	static public Vector4 CameraSpacePlane (Camera cam, Vector3 pos, Vector3 normal, float sideSign, float clipPlaneOffset)
	{
		Vector3 offsetPos = pos + normal * clipPlaneOffset;
		Matrix4x4 m = cam.worldToCameraMatrix;
		Vector3 cpos = m.MultiplyPoint (offsetPos);
		Vector3 cnormal = m.MultiplyVector (normal).normalized * sideSign;
		return new Vector4 (cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot (cpos, cnormal));
	}
}
