//
// Author:
//   Andreas Suter (andy@edelweissinteractive.com)
//
// Copyright (C) 2012 Edelweiss Interactive (http://edelweissinteractive.com)
//

using UnityEngine;
using System.Collections;
using Edelweiss.DecalSystem;

/// <summary>
/// Instances of this class are used to manage the visualization of
/// <see cref="T:Edelweiss.DecalSystem.DecalProjector">decal projections</see> onto meshes and terrains.
/// While the actual cutting operations are performed in <see cref="T:Edelweiss.DecalSystem.DecalsMesh">decals meshes</see>
/// and <see cref="T:Edelweiss.DecalSystem.DecalsMeshCutter">decals mesh cutters</see>, objects of this type provide the required
/// rendering infrastructure. <see cref="T:Edelweiss.DecalSystem.DecalsMesh">Decals meshes</see> can be
/// <see cref="M:Edelweiss.DecalSystem.Decals.UpdateDecalsMeshes(Edelweiss.DecalSystem.DecalsMesh)">applied</see> to
/// instances of this class and both the creation and destruction of the
/// <see cref="T:DS_DecalsMeshRenderer"/> objects is handled automatically.
/// For the visualization of projections onto skinned meshes, there is <see cref="T:DS_SkinnedDecals"/>.
/// <seealso cref="T:DS_SkinnedDecals"/>
/// </summary>
public class DS_Decals : Decals {
	
	/// <inheritdoc />
	protected override DecalsMeshRenderer AddDecalsMeshRendererComponentToGameObject (GameObject a_GameObject) {
		DecalsMeshRenderer l_Result = a_GameObject.AddComponent <DS_DecalsMeshRenderer> ();
		return (l_Result);
	}
}
