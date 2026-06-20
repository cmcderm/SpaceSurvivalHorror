using Godot;
using System.Collections.Generic;

namespace SpaceSurvivalHorror.Scripts.Systems.Gravity;

public static class GravityManager
{
	public static List<GravitySource> GravitySources = new();

	public static Vector3 GetGravityAt(Vector3 worldPos) {
		Vector3 gravity = Vector3.Zero;
		
		foreach (GravitySource gs in GravitySources) {
			gravity += gs.GetGravityAt(worldPos);
		}

		return gravity;
	}
}
