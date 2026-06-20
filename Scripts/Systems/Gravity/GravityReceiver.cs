using System.Collections.Generic;
using Godot;

namespace SpaceSurvivalHorror.Scripts.Systems.Gravity;

public partial class GravityReceiver : Node3D {

	[Export] private string name = "GravityReceiver";

	private RigidBody3D _rb;
	
	public override void _Ready()
	{
		_rb = GetParent<RigidBody3D>();
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 totalGravity = GravityManager.GetGravityAt(_rb.GlobalPosition);
		GD.Print($"{name} {totalGravity.Length()} pos: {_rb.GlobalPosition.Length()}");
		_rb.ApplyCentralForce(totalGravity * _rb.Mass);
	}
}
