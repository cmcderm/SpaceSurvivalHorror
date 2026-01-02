using Godot;
using System;

public partial class PlayerController : CharacterBody3D
{
	private Node3D CameraMount;
	private Node3D CameraRig;
	private Camera3D camera3d;
	
	private const float Speed = 5.0f;
	private const float JumpVelocity = 4.5f;

	private const float SensHorizontal = 0.15f;
	private const float SensVertical = 0.15f;

	private const float VerticalLookMax = 85f;

	public override void _Ready() {
		CameraRig = GetNode<Node3D>("CameraRig");
		CameraMount = GetNode<Node3D>("CameraRig/ShoulderOffset/CameraMount");
		camera3d = GetNode<Camera3D>("CameraRig/ShoulderOffset/CameraMount/Camera3d");
		
		GD.Print(CameraRig);
		GD.Print(CameraMount);
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _Input(InputEvent @event) {
		switch (@event) {
			case InputEventMouseMotion e: {
				ProcessMouseMotionEvent(e);
				break;
			}
			case InputEventKey eventKey: {
				if (eventKey.Keycode == Key.Escape && eventKey.Pressed) {
					Input.MouseMode = Input.MouseModeEnum.Visible;
				}
				break;
			}
			case InputEventMouseButton mouseButton when Input.MouseMode != Input.MouseModeEnum.Captured: {
				if (mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed) {
					Input.MouseMode = Input.MouseModeEnum.Captured;
				}
				break;
			}
		}
	}
	
	private void ProcessMouseMotionEvent(InputEventMouseMotion e) {
		GD.Print("Mouse Motion: " + e.Relative);

		CameraRig.RotateY(Mathf.DegToRad(-e.Relative.X * SensHorizontal));
		
		// Clamp vertical rotation
		Basis cameraBasis = CameraMount.GlobalTransform.Basis;

		Vector3 forward = -cameraBasis.Z;

		float pitch = Mathf.Asin(forward.Y);

		pitch -= Mathf.DegToRad(e.Relative.Y * SensVertical);
		
		pitch = Mathf.Clamp(
			pitch, 
			Mathf.DegToRad(-VerticalLookMax),
			Mathf.DegToRad(VerticalLookMax)
		);

		CameraMount.Rotation = new Vector3(pitch, 0, 0);
	}

	public override void _PhysicsProcess(double delta) {
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor()) {
			velocity += GetGravity() * (float)delta;
		}

		// Handle Jump.
		if (Input.IsActionJustPressed("game_jump") && IsOnFloor()) {
			velocity.Y = JumpVelocity;
		}

		// Get the input direction and handle the movement/deceleration.
		Vector2 inputDir = Input.GetVector("game_left", "game_right", "game_forward", "game_backward");
		
		Basis camBasis = camera3d.GlobalTransform.Basis;

		Vector3 camForward = -camBasis.Z;
		camForward.Y = 0;
		camForward = camForward.Normalized();

		Vector3 camRight = camBasis.X;
		camRight.Y = 0;
		camRight = camRight.Normalized();
		
		Vector3 desiredMove = camForward * inputDir.Y + camRight * inputDir.X;
		
		if (desiredMove != Vector3.Zero) {
			velocity.X = desiredMove.X * Speed;
			velocity.Z = desiredMove.Z * Speed;
		}
		else {
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}
