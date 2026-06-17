using Godot;
using SpaceSurvivalHorror.Scripts.Interfaces;

namespace SpaceSurvivalHorror.Scripts.Player;

public partial class PlayerController : CharacterBody3D {
	[Export] public bool Enabled = true;
	[Export] public float MoveSpeed = 5.0f;
	[Export] public float JumpVelocity = 4.5f;
	[Export] public float MouseSensitivity = 0.001f;
	[Export] public float VerticalLookMax = Mathf.DegToRad(89f);	

	private Node3D _head = null!;
	private Camera3D _camera = null!;
	private RayCast3D _interactionRay = null!;

	[Export] private PlayerUI _playerUi;

	IInteractable _currentInteractable;	
	
	private float _pitch;

	public override void _Ready() {
		_head = GetNode<Node3D>("Head");
		_camera = GetNode<Camera3D>("Head/Camera3D");
		_interactionRay = GetNode<RayCast3D>("InteractionRay");

		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _UnhandledInput(InputEvent @event) {
		if (@event is InputEventMouseMotion mouseMotion) {
			RotateY(-mouseMotion.Relative.X * MouseSensitivity);	
			
			_pitch -= mouseMotion.Relative.Y * MouseSensitivity;
			_pitch = Mathf.Clamp(_pitch, -VerticalLookMax, VerticalLookMax);

			_head.Rotation = new Vector3(_pitch, 0.0f, 0.0f);
			_interactionRay.Rotation = _head.Rotation;
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
		
		UpdateInteractionPrompt();
	}

	private void UpdateInteractionPrompt() {
		_currentInteractable = null;

		if (!_interactionRay.IsColliding()) {
			_playerUi.HideInteractionPrompt();
			return;
		}
		
		var collider = _interactionRay.GetCollider();

		if (collider is not Node) {
			_playerUi.HideInteractionPrompt();
			return;
		}

		IInteractable interactable = FindInteractable(collider as Node);

		if (interactable == null) {
			_playerUi.HideInteractionPrompt();
			return;
		}
		
		_currentInteractable = interactable;
		_playerUi.ShowInteractionPrompt(_currentInteractable.GetPrompt());

	}
	
	private IInteractable FindInteractable(Node node)
	{
		while (node != null)
		{
			if (node is IInteractable interactable)
				return interactable;

			node = node.GetParent();
		}

		return null;
	}

	private void TryInteract() {
		_currentInteractable?.Interact(this);
	}
}
