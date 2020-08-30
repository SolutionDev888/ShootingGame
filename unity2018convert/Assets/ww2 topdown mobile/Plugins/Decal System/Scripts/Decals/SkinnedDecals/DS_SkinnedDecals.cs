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
/// <see cref="T:Edelweiss.DecalSystem.SkinnedDecalProjector">skinned decal projections</see> onto skinned meshes.
/// While the actual cutting operations are performed in <see cref="T:Edelweiss.DecalSystem.SkinnedDecalsMesh">skinned decals meshes</see>
/// and <see cref="T:Edelweiss.DecalSystem.SkinnedDecalsMeshCutter">skinned decals mesh cutters</see>, objects of this type provide a rendering
/// infrastructure. <see cref="T:Edelweiss.DecalSystem.SkinnedDecalsMesh">Skinned decals meshes</see> can be
/// <see cref="M:Edelweiss.DecalSystem.SkinnedDecals.UpdateSkinnedDecalsMeshes(Edelweiss.DecalSystem.SkinnedDecalsMesh)">applied</see> to
/// instances of this class and both the creation and destruction of the
/// <see cref="T:DS_SkinnedDecalsMeshRenderer"/> objects is handled automatically.
/// For the visualization of projections onto meshes and terrains, there is <see cref="T:DS_Decals"/>.
/// <seealso cref="T:DS_Decals"/>
/// </summary>
public class DS_SkinnedDecals : SkinnedDecals {
	
	/// <inheritdoc />
	protected override SkinnedDecalsMeshRenderer AddSkinnedDecalsMeshRendererComponentToGameObject (GameObject a_GameObject) {
		SkinnedDecalsMeshRenderer l_Result = a_GameObject.AddComponent <DS_SkinnedDecalsMeshRenderer> ();
		return (l_Result);
	}
}
