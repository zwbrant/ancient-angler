using UnityEngine;

public class ColorObject : MonoBehaviour
{
	public Color m_Color = Color.white;

	void Update ()
	{
		Renderer rd = GetComponent<Renderer> ();
		rd.material.SetColor ("_Color", m_Color);
	}
}
