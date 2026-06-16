using Godot;
using SpaceSurvivalHorror.Interfaces;

namespace SpaceSurvivalHorror.Scripts.PlayerController {
	public partial class PlayerController : CharacterBody3D {
		[Export] public bool Enabled = true;
		[Export] public float MoveSpeed = 5.0f;
		[Export] public float JumpVelocity = 4.5f;
		[Export] public float MouseSensitivity = 0.001f;
		[Export] public float VerticalLookMax = Mathf.DegToRad(89f);	

		[Export] public Node3D Head = null!;
		[Export] public Camera3D Camera = null!;
		[Export] public RayCast3D InteractionRay = null!;

		private float _pitch;

		public override void _Ready() {
			Head = GetNode<Node3D>("Head");
			Camera = GetNode<Camera3D>("Head/Camera3D");
			InteractionRay = GetNode<RayCast3D>("InteractionRay");

			Input.MouseMode = Input.MouseModeEnum.Captured;
		}

		public override void _UnhandledInput(InputEvent @event) {
			if (@event is InputEventMouseMotion mouseMotion) {
				RotateY(-mouseMotion.Relative.X * MouseSensitivity);	
				
				_pitch -= mouseMotion.Relative.Y * MouseSensitivity;
				_pitch = Mathf.Clamp(_pitch, -VerticalLookMax, VerticalLookMax);

				Head.Rotation = new Vector3(_pitch, 0.0f, 0.0f);
			} else if (@event.IsActionPressed("interact")) {
				TryInteract();
			} else if (@event.IsActionReleased("escape")) {
				GetTree().Quit();
			}
		}

		public override void _PhysicsProcess(double delta) {
			if (!Enabled) {
				return;
			}
			
			Vector3 velocity = Velocity;
			bool isGrounded = IsOnFloor();

			if (!isGrounded) {
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
			
			Vector3 forward = -GlobalTransform.Basis.Z;
			Vector3 right = GlobalTransform.Basis.X;

			forward.Y = 0;
			right.Y = 0;

			forward = forward.Normalized();
			right = right.Normalized();

			Vector3 direction = (right * movement.X + forward * -movement.Y).Normalized();
			
			velocity.X = direction.X * MoveSpeed;
			velocity.Z = direction.Z * MoveSpeed;
			
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
