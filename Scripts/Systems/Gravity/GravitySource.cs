using System;
using Godot;

namespace SpaceSurvivalHorror.Scripts.Systems.Gravity;
public partial class GravitySource : Node3D
{
	[Export] public float Mass = 5e16f; // Mass of default planet
	[Export] public float AlignmentRadius = 100f;

	private const float GravitationalConstant = 6.674e-11f;

	private Node3D body;
	private RigidBody3D _rb;
	
	public override void _Ready() {
		GravityManager.GravitySources.Add(this);
		body = GetParent<Node3D>();
		_rb = GetParent<RigidBody3D>();
	}

	public override void _ExitTree() {
		GravityManager.GravitySources.Remove(this);
	}

	public Vector3 GetGravityAt(Vector3 worldPos) {
		Vector3 dir = body.GlobalPosition - worldPos;
		float dist = dir.Length();
		if (dist < 0.001f) { return Vector3.Zero; }
		
		float gravityStrength = (GravitationalConstant * _rb.Mass) / (dist * dist);
		
		return dir.Normalized() * gravityStrength;	
	}

	public bool CanAlignPlayer(Vector3 worldPos) {
		return (body.GlobalPosition - worldPos).LengthSquared() > AlignmentRadius * AlignmentRadius;
	}

	public Vector3 GetUpDirection(Vector3 worldPos) {
		return -(body.GlobalPosition - worldPos).Normalized();
	}
}
