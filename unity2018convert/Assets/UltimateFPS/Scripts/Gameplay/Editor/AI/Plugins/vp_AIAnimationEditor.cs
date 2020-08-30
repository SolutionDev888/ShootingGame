using UnityEditor;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class vp_AIAnimationEditor : vp_AIEditor
{

	public static void Animation()
	{
	
		Animation oldAnimation = m_AI.Animation.Animation;
		if(m_AI.Animation.Animation == null)
			foreach(Animation anim in m_AI.gameObject.GetComponentsInChildren<Animation>(true))
				m_AI.Animation.Animation = (Animation)EditorGUILayout.ObjectField(new GUIContent("Animation Object", "The object with an Animation component that has animations for this AI"), anim, typeof(Animation), true);	
		else
			m_AI.Animation.Animation = (Animation)EditorGUILayout.ObjectField(new GUIContent("Animation Object", "The object with an Animation component that has animations for this AI"), m_AI.Animation.Animation, typeof(Animation), true);
			
		if(m_AI.Animation.Animation != oldAnimation)
			Reset();
			
		if(m_AI.Animation.Animation == null)
		{
			GUILayout.Space(10);
			m_AI.Animation.Animation = (Animation)EditorGUILayout.ObjectField(new GUIContent("Animation Object", "The object with an Animation component that has animations for this AI"), m_AI.Animation.Animation, typeof(Animation), true);
			GUILayout.BeginHorizontal();
			GUILayout.Space(20);
			EditorGUILayout.HelpBox("An Animation component could not be found. Please add an animation component above.", MessageType.Info);
			GUILayout.Space(20);
			GUILayout.EndHorizontal();
		}
		else
		{
			List<vp_AIState> states = m_AI.States.Where(s => s.Name != "Default" && s.IsAnimationState).ToList();
			if(states.Count == 0)
			{
				GUILayout.Space(10);
				GUILayout.BeginHorizontal();
				GUILayout.Space(20);
				if(m_AI.States.Count == 1)
					EditorGUILayout.HelpBox("No states have been added. Please add some states in the States and Presets section below.", MessageType.Info);
				else
					EditorGUILayout.HelpBox("No states have been marked for animation. Please click the Animation button next to a state to add it here.", MessageType.Info);
				GUILayout.Space(20);
				GUILayout.EndHorizontal();
			}
			
			for (int i = 0; i < states.Count; ++i)
			{
				vp_AIState state = states[i];
				
				state.AnimationFoldout = EditorGUILayout.Foldout(state.AnimationFoldout, state.Name, state.AnimationFoldout ? SubFoldoutStyle : new GUIStyle("Foldout"));
				if(state.AnimationFoldout)
				{
					EditorGUI.indentLevel++;
					state.AutoPlayAnimations = EditorGUILayout.Toggle(new GUIContent("Auto Play Animations", "Whether or not animations should play automatically while this state is active. If disabled, animations can be played with code."), state.AutoPlayAnimations);
					
					if(state.AnimationClips.Count == 0)
						state.AnimationClips.Add(new vp_AIState.vp_AnimationState());
				
					for(int n = 0; n < state.AnimationClips.Count; n++)
					{
						GUILayout.BeginHorizontal();
						if(n > state.AnimationClips.Count-1)
						{
							state.AnimationClips.RemoveAt(n);
							return;
						}
						
						state.AnimationClips[n].Foldout = EditorGUILayout.Foldout(state.AnimationClips[n].Foldout, state.AnimationClips[n].Clip == null ? state.Name+" Animation Clip "+(n+1).ToString() : state.AnimationClips[n].Clip.name);
						if(state.AnimationClips.Count > 1)
						{
							if (GUILayout.Button("Remove", vp_EditorGUIUtility.SmallButtonStyle, GUILayout.MinWidth(50), GUILayout.MaxWidth(50), GUILayout.MinHeight(15)))
							{
								state.AnimationClips.RemoveAt(n);
								--n;
								return;
							}
						}
						GUILayout.EndHorizontal();
						
						
						if(state.AnimationClips[n].Foldout)
						{
							EditorGUI.indentLevel++;
							GUILayout.BeginHorizontal();
							List<AnimationClip> animationClips = new List<AnimationClip>();
							animationClips.Add(null);
							foreach(AnimationState s in m_AI.Animation.Animation)
								animationClips.Add(s.clip);
								
							if(!animationClips.Contains(state.AnimationClips[n].Clip))
								state.AnimationClips[n].Clip = null;
								
							int index = 0;
							if(state.AnimationClips[n].Clip != null)
								index = animationClips.IndexOf(state.AnimationClips[n].Clip);
							
							string[] animationNames = new string[animationClips.Count];
							for(int z = 0;z<animationNames.Length;z++)
								animationNames[z] = z == 0 ? "None" : animationClips[z].name;
							
							index = EditorGUILayout.Popup(index, animationNames);
							
							state.AnimationClips[n].Clip = animationClips[index];
							
							GUI.enabled = false;
							state.AnimationClips[n].Clip = (AnimationClip)EditorGUILayout.ObjectField(state.AnimationClips[n].Clip, typeof(AnimationClip), false);
							GUI.enabled = true;
							GUILayout.Space(5);
							GUILayout.EndHorizontal();
							
							state.AnimationClips[n].WrapMode = (WrapMode)EditorGUILayout.EnumPopup(new GUIContent("Wrap Mode", "WrapMode for this animation"), state.AnimationClips[n].WrapMode);
							state.AnimationClips[n].AnimationSpeed = EditorGUILayout.Slider(new GUIContent("Animation Speed", "Speed at which to play this animation. 1 is normal speed."), state.AnimationClips[n].AnimationSpeed, 0, 10);
							state.AnimationClips[n].CrossfadeLength = EditorGUILayout.Slider(new GUIContent("Crossfade Length", "Speed at which to crossfade to this animation. 0.3 is default."), state.AnimationClips[n].CrossfadeLength, 0, 3);
							state.AnimationClips[n].Layer = EditorGUILayout.IntSlider(new GUIContent("Layer", "Layer for this animation. 0 is default."), state.AnimationClips[n].Layer, -100, 100);
							state.AnimationClips[n].PlayMode = (PlayMode)EditorGUILayout.EnumPopup(new GUIContent("Play Mode", "Play Mode for this animation. Stop Same Layer is default."), state.AnimationClips[n].PlayMode);
							EditorGUI.indentLevel--;
						}
					}
					
					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					if (GUILayout.Button(new GUIContent("Add Animation", "Add a new animation for this state"), GUILayout.MinWidth(100), GUILayout.MaxWidth(100)))
						state.AnimationClips.Add(new vp_AIState.vp_AnimationState());
					GUILayout.EndHorizontal();
					
					GUILayout.Space(5);
					
					GUI.backgroundColor = Color.white;
					
					EditorGUI.indentLevel--;
				}
				
				states[i] = state;
			}
		}
	
	}
	
	
	public static void Reset()
	{
	
		foreach(vp_AIState state in m_AI.States)
		{
			state.AnimationClips = new List<vp_AIState.vp_AnimationState>();
			state.AutoPlayAnimations = false;
			state.LastAnimationClipLength = 0;
			state.LastAnimationClip = null;
		}
	
	}
	
}
