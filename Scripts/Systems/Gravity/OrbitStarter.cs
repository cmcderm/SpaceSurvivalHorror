using Godot;
using System;

public partial class OrbitStarter : RigidBody3D {
	
	[Export] public RigidBody3D OrbitBody;
	[Export] public float OrbitSpeed = 200f;
	[Export] public Vector3 OrbitAxis = Vector3.Up;

	public override void _Ready() {
		
		if (OrbitBody == null) {
			return;
		}
		
		Vector3 toCenter = OrbitBody.GlobalPosition - GlobalPosition;
		Vector3 orbitDirection = OrbitAxis.Cross(toCenter).Normalized();
		
		LinearVelocity = orbitDirection * OrbitSpeed;
	}
}
