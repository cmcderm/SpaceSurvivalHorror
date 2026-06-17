using Godot;
using SpaceSurvivalHorror.Scripts.Player;

public partial class ShipController : Node3D {
	[Export] public Marker3D PilotMarker;
		
	[Export] private RigidBody3D _rb;

	[Export] public float ThrustStrength;	
	
	private PlayerController _player;
	private Vector3 _thrust;
	
	
	
	public override void _Ready() {
		
	}

	public override void _PhysicsProcess(double delta) {
		if (_player == null) {
			return;
		}
		
		_player.SetGlobalPosition(PilotMarker.GlobalPosition);
		
		Vector2 input = Input.GetVector(
			"game_left",
			"game_right",
			"game_forward",
			"game_backward"
		);

		float verticalAxis = Input.GetAxis(
			"game_crouch", 
			"game_jump"
		);

		_thrust = new Vector3(input.X, verticalAxis, input.Y);
		
		_rb.ApplyCentralForce(_thrust * ThrustStrength * (float)delta);
	}

	public override void _UnhandledInput(InputEvent @event) {
		if (_player == null) {
			return;
		}

		if (@event.IsActionPressed("interact")) {
			// Get out of the ship
		}
	}

	public void BuckleIn(PlayerController player) {
		_player = player;
	}
}
