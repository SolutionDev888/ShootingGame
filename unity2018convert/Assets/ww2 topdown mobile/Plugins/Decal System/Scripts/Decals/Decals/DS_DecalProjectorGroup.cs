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
/// Instances of this class are used for the grouping of <see cref="T:DS_DecalProjector"/> child game objects.
/// Decal groups are only used in the Unity Editor to modify the transforms of all its child
/// <see cref="T:DS_DecalProjector">projectors</see> at once. Those objects have no effect at runtime.
/// </summary>
public class DS_DecalProjectorGroup : DecalProjectorGroup {
}
