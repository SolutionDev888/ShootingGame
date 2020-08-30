//----------------------------------------------
//            NJG MiniMap (NGUI)
// Copyright © 2014 Ninjutsu Games LTD.
//----------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// A game mini map that display icons and scroll UITexture when target moves.
/// </summary>

namespace NJG
{
	public abstract class UIMiniMapBase : UIMapBase
	{
		public enum ZoomType
		{
			ZoomIn,
			ZoomOut
		}

		static public bool initialized { get { return mInst != null; } }

		static UIMiniMapBase mInst;
		static public UIMiniMapBase inst
		{
			get
			{
				if (mInst == null) 
					mInst = GameObject.FindObjectOfType(typeof(UIMiniMapBase)) as UIMiniMapBase; 				
				
				return mInst;
			}
		}

		public Transform arrowRoot
		{
			get
			{
				if (mArrowRoot == null && Application.isPlaying)
				{
					mArrowRoot = NJGTools.AddChild(gameObject).transform;
					mArrowRoot.parent = iconRoot;
					mArrowRoot.name = "_MapArrows";
					mArrowRoot.localEulerAngles = Vector3.zero;
					mArrowRoot.localScale = Vector3.one;
					mArrowRoot.localPosition = Vector3.zero;
				}
				return mArrowRoot;
			}
		}		

		/// <summary>
		/// Key to toggle the minimap lock.
		/// </summary>

		public KeyCode lockKey = KeyCode.L;

		/// <summary>
		/// Optional north icon. Will be automatically placed if its assigned.
		/// </summary>

		public GameObject northIcon;

		/// <summary>
		/// North icon offset.
		/// </summary>

		//public int northIconOffset = 10;

		/// <summary>
		/// Key to toggle the world map.
		/// </summary>

		public KeyCode mapKey = KeyCode.M;

		/// <summary>
		/// Offset applied to the overlay border's size. Used to fine-tune the borders, making the overlay envelop the map properly.
		/// </summary>

		public int overlayBorderOffset = 0;	
		
		bool worldMapVisible;
		GameObject northRoot;		
		
		protected Vector2 mArrowScale;
		protected Transform mArrowRoot;
		protected List<NJGMapItem> mPingList = new List<NJGMapItem>();
		protected List<NJGMapItem> mPingUnused = new List<NJGMapItem>();
		protected List<UIMapArrowBase> mListArrow = new List<UIMapArrowBase>();
		protected List<UIMapArrowBase> mUnusedArrow = new List<UIMapArrowBase>();

		NJGMapItem pingMarker;
		protected int mArrowCount = 0;
		Pivot mPivot;
		Vector2 mMargin;

		protected override void OnStart()
		{
			base.OnStart();

			if (Application.isPlaying)
			{
				// Create north root and place icon north properly
				northRoot = NJGTools.AddChild(iconRoot.gameObject);
				northRoot.name = "North";
				northRoot.transform.localPosition = Vector3.zero;

				if (northIcon != null)
				{
					northIcon.transform.parent = northRoot.transform;
					northIcon.transform.localRotation = Quaternion.identity;
				}

				// Calculate the border radius for icon culling.
				if (calculateBorder)					
					mapBorderRadius = (rendererTransform.localScale.x / 2f) / 4f;				
			}

			UpdateAlignment();

			// Create a gameObject for the ping marker
			/*GameObject go = NJGTools.AddChild(iconRoot);
			go.name = "_Ping";

			// Assign Ping type to this marker, 'Ping' type most be created on the editor
			pingMarker = go.AddComponent<NJGMapItem>();
			pingMarker.type = "Ping";*/
		}

		#region Ping (This feature is not fullly working yet)

		static public System.Action<Vector3> onMapClick;

		public void OnClick()
		{
			/*Vector3 worldTouchPos = UICamera.lastHit.point;
			Vector3 localTouchPos = cachedTransform.InverseTransformPoint(worldTouchPos);
			localTouchPos -= cachedTransform.position;

			Vector3 pos = MapToWorld(localTouchPos);

			//Debug.Log("worldTouchPos:" + worldTouchPos + " / localTouchPos:" + localTouchPos + " / pos:" + pos + " / textureCoord " + UICamera.lastHit.textureCoord + " - textureCoord2 " + UICamera.lastHit.textureCoord1);

			if (onMapClick != null) onMapClick(pos);*/

			/*Debug.Log("CLick " + UICamera.lastHit.textureCoord + " - " + UICamera.lastHit.textureCoord2 + " / " + UICamera.lastHit.point);
			NJGMap.MapItemType mi = NJG.NJGMapBase.instance.GetItem("Ping");
			if (mi != null)
			{
				UIMapIcon icon = GetEntry(pingMarker);
				//Debug.Log("CLick " + UICamera.lastHit.textureCoord + " - " + UICamera.lastHit.textureCoord2 + " / " + UICamera.lastHit.point);
				icon.cachedTransform.localPosition = new Vector3(UICamera.lastHit.point.x, UICamera.lastHit.point.y, 0);
			}
			else
			{
				Debug.LogWarning("There is no 'Ping' icon type defined");
			}*/
		}

		#endregion

		
		/// <summary>
		/// Updates MiniMap alignment.
		/// </summary>

		public virtual void UpdateAlignment()
		{
			Vector3 pos = Vector3.zero;
			pos.z = rendererTransform.localPosition.z;

			// If the map doesn't have a central pivot point, we need to offset it by half of the texture's size
			if (pivot != Pivot.Center)
			{
				switch (pivot)
				{
					case Pivot.TopRight:
						pos.x = Mathf.Round(-0.5f * mapScale.x) - margin.x;
						pos.y = Mathf.Round(-0.5f * mapScale.y) - margin.y;
						break;
					case Pivot.Right:
						pos.x = Mathf.Round(-0.5f * mapScale.x) - margin.x;
						break;
					case Pivot.BottomRight:
						pos.x = Mathf.Round(-0.5f * mapScale.x) - margin.x;
						pos.y = Mathf.Round(0.5f * mapScale.y) + margin.y;
						break;
					case Pivot.Bottom:
						pos.y = Mathf.Round(0.5f * mapScale.y) + margin.y;
						break;
					case Pivot.Top:
						pos.y = Mathf.Round(-0.5f * mapScale.y) - margin.y;
						break;
					case Pivot.TopLeft:
						pos.x = Mathf.Round(0.5f * mapScale.x) + margin.x;
						pos.y = Mathf.Round(-0.5f * mapScale.y) - margin.y;
						break;
					case Pivot.Left:
						pos.x = Mathf.Round(0.5f * mapScale.x) + margin.x;
						break;
					case Pivot.BottomLeft:
						pos.x = Mathf.Round(0.5f * mapScale.x) + margin.x;
						pos.y = Mathf.Round(0.5f * mapScale.y) + margin.y;
						break;
				}
			}

			rendererTransform.localPosition = pos;

			if(iconRoot != null) iconRoot.localPosition = new Vector3(rendererTransform.localPosition.x, rendererTransform.localPosition.y, 1);
		}

		/// <summary>
		/// Update the icon icon for the specified unit, assuming it's visible.
		/// </summary>		

		protected override void UpdateIcon(NJGMapItem item, float x, float y)
		{
			// If the unit is not visible, don't consider it
			bool isVisible = false;

			if (map.fow.enabled)
			{
				isVisible = (((x - mapBorderRadius) >= -mapHalfScale.x) &&
					 ((x + mapBorderRadius) <= mapHalfScale.x)) &&
					 (((y - mapBorderRadius) >= -mapHalfScale.y) &&
					 ((y + mapBorderRadius) <= mapHalfScale.y));

				if (!item.isRevealed) isVisible = false;
			}
			else
			{
				isVisible = (((x - mapBorderRadius) >= -mapHalfScale.x) &&
					 ((x + mapBorderRadius) <= mapHalfScale.x)) &&
					 (((y - mapBorderRadius) >= -mapHalfScale.y) &&
					 ((y + mapBorderRadius) <= mapHalfScale.y));
			}

			if (!isPanning)
			{
				if (!isVisible && item.haveArrow)
				{
					if (item.arrow == null) item.arrow = (UIMapArrowBase)GetArrow(item);

					if (item.arrow != null)
					{
						if (!NJGTools.GetActive(item.arrow.gameObject)) NJGTools.SetActive(item.arrow.gameObject, true);
						item.arrow.UpdateRotation(target.position);
					}
				}
				else if (isVisible && item.haveArrow)
				{
					if (item.arrow != null)
					{
						if (NJGTools.GetActive(item.arrow.gameObject))
						{
							NJGTools.SetActive(item.arrow.gameObject, false);
						}
					}
				}
			}

			if (!isVisible) return;

			UIMapIconBase icon = GetEntry(item);			

			if (icon != null && !icon.isValid)
			{
				icon.isMapIcon = false;
				icon.isValid = true;
				Transform t = icon.cachedTransform;
				Vector3 newPos = new Vector3(x, y, 0f);
				if (item.updatePosition) if (t.localPosition != newPos) t.localPosition = newPos;

				if (item.rotate)
				{
					float angle = ((Vector3.Dot(item.cachedTransform.forward, Vector3.Cross(Vector3.up, Vector3.forward)) <= 0f) ? 1f : -1f) * Vector3.Angle(item.cachedTransform.forward, Vector3.forward);
					t.localEulerAngles = new Vector3(t.localEulerAngles.x, t.localEulerAngles.y, angle);
				}
				else if (!item.rotate && rotateWithPlayer)
				{
					Vector3 eu = new Vector3(0, 0, -iconRoot.localEulerAngles.z);
					if (!t.localEulerAngles.Equals(eu)) t.localEulerAngles = eu;
				}
				else
					if (!t.localEulerAngles.Equals(Vector3.zero)) t.localEulerAngles = Vector3.zero;
			}
		}		

		/// <summary>
		/// Get the map icon entry associated with the specified unit.
		/// </summary>

		protected virtual UIMapArrowBase GetArrow(Object o) { return (UIMapArrowBase)o; }

		/// <summary>
		/// Update what's necessary.
		/// </summary>

		protected override void Update()
		{
			if (mPivot != pivot || mMargin != margin || mMapScale != mapScale)
			{
				mMapScale = mapScale;
				mPivot = pivot;
				mMargin = margin;
				UpdateAlignment();
			}

			if (arrowRoot != null)
			{
				if (isPanning && arrowRoot.localScale != new Vector3(0.001f, 0.001f, 0.001f)) arrowRoot.localScale = new Vector3(0.001f, 0.001f, 0.001f);
				else if (!isPanning && arrowRoot.localScale != Vector3.one) arrowRoot.localScale = Vector3.one;
			}

			UpdateKeys();

			base.Update();
		}

		protected virtual void UpdateKeys()
		{
			if (Input.GetKeyDown(mapKey))
				ToggleWorldMap();

			if (Input.GetKeyDown(lockKey))
				rotateWithPlayer = !rotateWithPlayer;
		}

		#region Public Methods

		/// <summary>
		/// Delete the specified entry, adding it to the unused list.
		/// </summary>

		public virtual void DeleteArrow(UIMapArrowBase ent)
		{
			if (ent != null)
			{
				mListArrow.Remove(ent);
				mUnusedArrow.Add(ent);
				NJGTools.SetActive(ent.gameObject, false);
			}
		}		

		/// <summary>
		/// Toggle Wold map if the instance its found
		/// </summary>

		public void ToggleWorldMap()
		{
			if (UIWorldMapBase.inst != null)
			{
				UIWorldMapBase.inst.Toggle();
			}
		}
		#endregion
	}
}