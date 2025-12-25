using Godot;
using System;

public partial class PlayerController : CharacterBody3D
{
	private Node3D CameraMount;
	
	private const float Speed = 5.0f;
	private const float JumpVelocity = 4.5f;

	private const float SensHorizontal = 0.15f;
	private const float SensVertical = 0.15f;

	public override void _Ready()
	{
		CameraMount = GetNode<Node3D>("CameraMount");
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _Input(InputEvent @event)
	{
		switch (@event)
		{
			case InputEventMouseMotion e:
			{
				GD.Print("Mouse Motion: " + e.Relative);
				RotateY(Mathf.DegToRad(-e.Relative.X * SensHorizontal));
				CameraMount.RotateX(Mathf.DegToRad(-e.Relative.Y + SensVertical));
				break;
			}
			case InputEventKey eventKey:
			{
				if (eventKey.Keycode == Key.Escape && eventKey.Pressed)
				{
					Input.MouseMode = Input.MouseModeEnum.Visible;
				}

				break;
			}
			case InputEventMouseButton mouseButton when Input.MouseMode != Input.MouseModeEnum.Captured:
			{
				if (mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed)
				{
					Input.MouseMode = Input.MouseModeEnum.Captured;
				}

				break;
			}
		}
	}

	public override void _PhysicsProcess(double delta) {
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		// Handle Jump.
		if (Input.IsActionJustPressed("game_jump") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("game_left", "game_right", "game_forward", "game_backward");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}
