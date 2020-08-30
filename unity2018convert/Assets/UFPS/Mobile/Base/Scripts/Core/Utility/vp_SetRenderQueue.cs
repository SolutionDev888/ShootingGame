/*
	SetRenderQueue.cs
	
	Sets the RenderQueue of an object's materials on Awake. This will instance
	the materials, so the script won't interfere with other renderers that
	reference the same materials.
*/

using UnityEngine;

public class vp_SetRenderQueue : MonoBehaviour {
	
	[SerializeField]
	protected int[] m_Queues = new int[]{3000};
	
	protected void Awake() {
		Material[] materials = GetComponent<Renderer>().materials;
		for (int i = 0; i < materials.Length && i < m_Queues.Length; ++i) {
			materials[i].renderQueue = m_Queues[i];
		}
	}
}