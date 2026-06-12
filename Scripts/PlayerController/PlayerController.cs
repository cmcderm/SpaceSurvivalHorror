using Godot;
using SpaceSurvivalHorror.Interfaces;

namespace SpaceSurvivalHorror.Scripts.PlayerController {
	public partial class PlayerController : CharacterBody3D {
		[Export] public bool Enabled = true;
		[Export] public float MoveSpeed = 5.0f;
		[Export] public float JumpVelocity = 4.5f;
		[Export] public float MouseSensitivity = 0.15f;
		[Export] public float VerticalLookMax = 89f;	

		[Export] public Node3D Head = null!;
		[Export] public Camera3D Camera = null!;
		[Export] public RayCast3D InteractionRay = null!;

		private float _pitch;

		public override void _Ready() {
			Head = GetNode<Node3D>("Head");
			Camera = GetNode<Camera3D>("Head/Camera3d");
			InteractionRay = GetNode<RayCast3D>("InteractionRay");

			Input.MouseMode = Input.MouseModeEnum.Captured;
		}

		public override void _UnhandledInput(InputEvent @event) {
			if (@event is InputEventMouseMotion mouseMotion) {
				RotateY(-mouseMotion.Relative.X * MouseSensitivity);	
				
				_pitch -= mouseMotion.Relative.Y * MouseSensitivity;
				_pitch = Mathf.Clamp(_pitch, -MouseSensitivity, MouseSensitivity);

				Head.Rotation = new Vector3(_pitch, 0, 0);
			} else if (@event.IsActionPressed("interact")) {
				TryInteract();
			} else if (@event.IsActionReleased("escape")) {
				Input.MouseMode = Input.MouseModeEnum.Visible;
			} else if (@event is InputEventMouseButton mouseButton) {
				if (mouseButton.Pressed) {
					Input.MouseMode = Input.MouseModeEnum.Captured;	
				}
			}
		}

		public override void _PhysicsProcess(double delta) {
			if (!Enabled) {
				return;
			}
			
			Vector3 velocity = Velocity;
			bool isGrounded = IsOnFloor();

			if (!IsOnFloor()) {
				velocity += GetGravity() * (float)delta;
			}

			if (isGrounded && Input.IsActionJustPressed("game_jump")) {
				velocity.Y = JumpVelocity;
			}

			Vector2 movement = Input.GetVector(
				"game_left",
				"game_right",
				"game_forward",
				"game_backward"
			);
			
			Vector3 direction = (GlobalTransform.Basis * new Vector3(movement.X, 0, movement.Y)).Normalized();
			
			velocity.X = direction.X * MoveSpeed;
			velocity.Y = direction.Y * MoveSpeed;
			
			Velocity = velocity;

			MoveAndSlide();
		}

		private void TryInteract() {
			if (!InteractionRay.IsColliding()) {
				return;
			}

			var collider = InteractionRay.GetCollider();

			if (collider is IInteractable interactable) {
				interactable.Interact(this);
			}
		}
	}
}
