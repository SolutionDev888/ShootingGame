using UnityEngine;
using System.Collections;
namespace RadarSystem{
	public class RaderItem : MonoBehaviour {
		public int index;
		public Transform m_trans;
		private void Awake(){
			m_trans = GetComponent<Transform> ();
		}
	}
}