using System;
using Godot;

namespace SpaceSurvivalHorror.Scripts.Systems.Gravity;
public partial class GravitySource : Node3D
{
	[Export] public float SurfaceGravity = 9f;
	[Export] public float Radius = 200f;
	[Export] public float AlignmentRadius = 250f;

	private float Mu => SurfaceGravity * Radius * Radius;

	private Node3D _body;
	private RigidBody3D _rb;
	
	public override void _Ready() {
		GravityManager.GravitySources.Add(this);
		_body = GetParent<Node3D>();
		_rb = GetParent<RigidBody3D>();
	}

	public override void _ExitTree() {
		GravityManager.GravitySources.Remove(this);
	}

	public Vector3 GetGravityAt(Vector3 worldPos) {
		Vector3 dir = _body.GlobalPosition - worldPos;
		float distSq = dir.LengthSquared();
		
		if (distSq < 0.001f) { return Vector3.Zero; }
		
		return dir.Normalized() * (Mu / distSq);	
	}

	public bool CanAlignPlayer(Vector3 worldPos) {
		return (_body.GlobalPosition - worldPos).LengthSquared() > AlignmentRadius * AlignmentRadius;
	}

	public Vector3 GetUpDirection(Vector3 worldPos) {
		return -(_body.GlobalPosition - worldPos).Normalized();
	}
}
