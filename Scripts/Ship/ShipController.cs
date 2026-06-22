using Godot;
using SpaceSurvivalHorror.Scripts.Player;

public partial class ShipController : RigidBody3D {
	[Export] public Marker3D PilotMarker;
	[Export] public Marker3D ExitMarker;

	[Export] public float ThrustStrength = 100f;
	
	private PlayerController _player;
	private Vector3 _thrust;
	
	private bool _waitForInteractionRelease = false;
	
	public override void _Ready() {
		
	}

	public override void _PhysicsProcess(double delta) {
		if (_player == null) {
			return;
		}

		_player.SetGlobalPosition(PilotMarker.GlobalPosition);
		_player.SetGlobalRotation(PilotMarker.GlobalRotation);

		if (Input.IsActionPressed("game_forward")) {
			ApplyCentralForce(-GlobalBasis.Z * ThrustStrength);
		}

		if (Input.IsActionPressed("game_backward")) {
			ApplyCentralForce(GlobalBasis.Z * ThrustStrength);
		}

		if (Input.IsActionPressed("game_jump")) {
			ApplyCentralForce(GlobalBasis.Y * ThrustStrength);
		}

		if (Input.IsActionPressed("game_crouch")) {
			ApplyCentralForce(-GlobalBasis.Y * ThrustStrength);
		}

		if (Input.IsActionPressed("game_left")) {
			ApplyCentralForce(GlobalBasis.X * ThrustStrength);
		}
		
		if (Input.IsActionPressed("game_right")) {
			ApplyCentralForce(-GlobalBasis.X * ThrustStrength);
		}
		
		DebugOverlay.SetValue("Ship Velocity",  LinearVelocity.Length());
	}

	public override void _UnhandledInput(InputEvent @event) {
		if (_player == null) {
			return;
		}
		
		if (!_waitForInteractionRelease && Input.IsActionJustPressed("interact")) {
			Unbuckle();
			return;
		}

		if (Input.IsActionJustReleased("interact")) {
			_waitForInteractionRelease = false;
		}
	}

	public void BuckleIn(PlayerController player) {
		_player = player;
		_player.BuckleIn(this);
		_waitForInteractionRelease = true;
	}

	public void Unbuckle() {
		_player.SetGlobalPosition(ExitMarker.GlobalPosition);
		_player.SetGlobalRotation(ExitMarker.GlobalRotation);
		_player.Unbuckle();
		_player = null;
	}
}
