using UnityEditor;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class vp_AIAudioStatesEditor : vp_AIEditor
{

	public static void AudioStates()
	{
	
		List<vp_AIState> states = m_AI.States.Where(s => s.Name != "Default" && s.IsAudioState).ToList();
		if(states.Count == 0)
		{
			GUILayout.Space(10);
			GUILayout.BeginHorizontal();
			GUILayout.Space(20);
			if(m_AI.States.Count == 1)
				EditorGUILayout.HelpBox("No states have been added. Please add some states in the States and Presets section below.", MessageType.Info);
			else
				EditorGUILayout.HelpBox("No states have been marked for audio. Please click the Audio button next to a state to add it here.", MessageType.Info);
			GUILayout.Space(20);
			GUILayout.EndHorizontal();
		}
		
		for (int i = 0; i < states.Count; ++i)
		{
			vp_AIState state = states[i];
			
			state.AudioClipsFoldout = EditorGUILayout.Foldout(state.AudioClipsFoldout, state.Name, state.AudioClipsFoldout ? SubFoldoutStyle : new GUIStyle("Foldout"));
			if(state.AudioClipsFoldout)
			{
				EditorGUI.indentLevel++;
			
				for(int n = 0; n < state.AudioClips.Count; n++)
				{	
					GUILayout.BeginHorizontal();
					state.AudioClips[n] = (AudioClip)EditorGUILayout.ObjectField(state.AudioClips[n], typeof(AudioClip), false);
					if(state.AudioClips.Count > 1)
					{
						if (GUILayout.Button("Remove", vp_EditorGUIUtility.SmallButtonStyle, GUILayout.MinWidth(50), GUILayout.MaxWidth(50), GUILayout.MinHeight(15)))
						{
							state.AudioClips.RemoveAt(n);
							--n;
							return;
						}
					}
					GUILayout.EndHorizontal();
				}
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button(new GUIContent("Add Audio Clip", "Add a new audio clip for this state"), GUILayout.MinWidth(150), GUILayout.MaxWidth(150)))
					state.AudioClips.Add(null);
				GUILayout.EndHorizontal();
				
				EditorGUILayout.LabelField(new GUIContent("Pitch (Min:Max)", "random pitch range the sound will play at"));
				GUILayout.Space(-20);
				state.AudioPitch = EditorGUILayout.Vector2Field("", state.AudioPitch);
				state.AudioProbability = EditorGUILayout.Slider(new GUIContent("Probability "+(state.AudioProbability == 1 ? "(always)" : ""), "probability at which the sound will play. 1 will always play the sound."), state.AudioProbability, 0, 1);
				state.AudioPlayInterval = EditorGUILayout.Slider(new GUIContent("Play Interval", "Interval in seconds at which this state plays sounds"), state.AudioPlayInterval, 0, 10);
				
				GUILayout.Space(5);
				
				GUI.backgroundColor = Color.white;
				
				EditorGUI.indentLevel--;
			}
			
			states[i] = state;
		}
	
	}
	
	
	public static void Reset()
	{
	
		foreach(vp_AIState state in m_AI.States)
		{
			state.AudioProbability = 1;
			state.AudioClipsFoldout = false;
			state.AudioClips = new List<AudioClip>(){ null };
			state.LastAudioClip = null;
			state.LastTimeAudioPlayed = 0;
			state.AudioPlayInterval = 0;
			state.AudioPitch = new Vector2(1.0f, 1.0f);
		}
	
	}
	
}
