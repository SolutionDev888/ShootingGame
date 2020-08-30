using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class vp_DeleteAStar : Editor {

	[MenuItem("Assets/Ultimate FPS AI/Remove A*Pathfinding Project")]
	public static void RemoveAStar()
	{
	
		if(EditorUtility.DisplayDialog("Delete A*Pathfinding Project", "This will remove all the files that pertain to the A*Pathfinding project and cannot be done. Are you sure this is what you want to do?", "Yes", "No"))
		{
		
			List<string> aStarFiles = new List<string>(){
											"Assets/UltimateFPS/Scripts/Gameplay/AI/Core/vp_AI.cs",
											"Assets/UltimateFPS/Scripts/Gameplay/AI/Extras/vp_AIAStarGraphsManager.cs",
											"Assets/UltimateFPS/Scripts/Gameplay/Editor/AI/Plugins/vp_AIAStarEditor.cs",
											"Assets/UltimateFPS/Scripts/Gameplay/AI/Plugins/vp_AIAStarPathfinding.cs",
											"Assets/UltimateFPS/Scripts/Gameplay/AI/AstarPathfindingProject",
											"Assets/UltimateFPS/Scripts/Gameplay/Editor/AI/vp_DeleteAStar.cs"
										};
		
			foreach(string f in aStarFiles)
				AssetDatabase.DeleteAsset(f);
				
			AssetDatabase.MoveAsset("Assets/UltimateFPS/Scripts/Gameplay/AI/Core/vp_AIWithoutAStar.txt", "Assets/UltimateFPS/Scripts/Gameplay/AI/Core/vp_AI.cs");
			
			AssetDatabase.Refresh();
		
		}
	
	}
	
}
