using Godot;
using SpaceSurvivalHorror.Scripts.Interfaces;
using SpaceSurvivalHorror.Scripts.Systems.Gravity;

namespace SpaceSurvivalHorror.Scripts.Player;

public enum PlayerMode {
	Walking,
	Piloting
}

public partial class PlayerController : CharacterBody3D {
	[Export] public bool Enabled = true;
	[Export] public float Acceleration = 25f;
	[Export] public float MoveSpeed = 5.0f;
	[Export] public float JumpVelocity = 50f;
	[Export] public float MouseSensitivity = 0.001f;
	[Export] public float VerticalLookMax = Mathf.DegToRad(89f);	
	
	public PlayerMode playerMode = PlayerMode.Walking;

	private Node3D _head = null!;
	private Camera3D _camera = null!;
	private RayCast3D _interactionRay = null!;

	[Export] private PlayerUI _playerUi;

	IInteractable _currentInteractable;	
	
	private float _pitch;
	private uint _savedLayer;
	private uint _savedMask;
	private bool _waitForInteractionRelease = false;

	public override void _Ready() {
		_head = GetNode<Node3D>("Head");
		_camera = GetNode<Camera3D>("Head/Camera3D");
		_interactionRay = GetNode<RayCast3D>("InteractionRay");

		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _UnhandledInput(InputEvent @event) {
		if (@event.IsActionReleased("escape")) {
			Input.MouseMode = Input.MouseModeEnum.Visible;
		} else if (@event is InputEventMouseButton mouseButton) {
			if (mouseButton.Pressed) {
				Input.MouseMode = Input.MouseModeEnum.Captured;
			}	
		}
		
		if (playerMode != PlayerMode.Walking) {
			_pitch = 0f;
			_head.Rotation = Vector3.Zero;
			return;
		}
		
		if (@event is InputEventMouseMotion mouseMotion) {
			MouseLook(mouseMotion);	
		} else if (@event.IsActionPressed("interact")) {
			TryInteract();
		} else if (@event.IsActionReleased("interact")) {
			_waitForInteractionRelease = false;
		}
	}
	
	private void MouseLook(InputEventMouseMotion mouseMotion)
	{
		if (Input.MouseMode != Input.MouseModeEnum.Captured)
			return;

		float yaw = -mouseMotion.Relative.X * MouseSensitivity;
		float pitchDelta = -mouseMotion.Relative.Y * MouseSensitivity;

		GlobalBasis = new Basis(UpDirection.Normalized(), yaw) * GlobalBasis;

		_pitch += pitchDelta;
		_pitch = Mathf.Clamp(_pitch, -VerticalLookMax, VerticalLookMax);

		_head.Rotation = new Vector3(_pitch, 0.0f, 0.0f);
	}
	
	public override void _PhysicsProcess(double delta) {
		DebugOverlay.SetValue("Player Velocity", $"{Velocity.Length():0.00}  m/s");
		if (playerMode != PlayerMode.Walking || !Enabled) {
			return;
		}
		
		Vector3 velocity = Velocity;
		bool isGrounded = IsOnFloor();

		Vector3 localGravity = GetLocalGravity();	
		
		DebugOverlay.SetValue("Player Gravity", localGravity);
		
		velocity += localGravity;

		AlignWithGravity(delta);
		
		DebugOverlay.SetValue("Player Up Direction", UpDirection);

		if (isGrounded && Input.IsActionJustPressed("game_jump")) {
			velocity += UpDirection * JumpVelocity;
		}

		Vector2 input = Input.GetVector(
			"game_left",
			"game_right",
			"game_forward",
			"game_backward"
		);
		
		Vector3 forward = -GlobalBasis.Z;
		Vector3 right = GlobalBasis.X;

		forward = forward.Slide(UpDirection).Normalized();
		right = right.Slide(UpDirection).Normalized();
		
		forward = forward.Normalized();
		right = right.Normalized();

		Vector3 direction = (right * input.X + forward * -input.Y).Normalized();

		Vector3 surfaceVelocity = velocity.Slide(UpDirection);
		Vector3 verticalVelocity = velocity - surfaceVelocity;

		Vector3 targetSurfaceVelocity = direction * MoveSpeed;

		surfaceVelocity = surfaceVelocity.MoveToward(
			targetSurfaceVelocity,
			Acceleration * (float)delta
		);
		
		velocity = surfaceVelocity + verticalVelocity;
		
		Velocity = velocity;

		MoveAndSlide();
		
		UpdateInteractionPrompt();
	}

	private Vector3 GetLocalGravity() {
		Vector3 gravity = Vector3.Zero;
		foreach (GravitySource gs in GravityManager.GravitySources) {
			gravity += gs.GetGravityAt(GlobalPosition);
		}

		return gravity;
	}

	private void AlignWithGravity(double delta) {
		bool foundAlignment = false;
		foreach (GravitySource gs in GravityManager.GravitySources) {
			if (!foundAlignment && gs.CanAlignPlayer(GlobalPosition)) {
				foundAlignment = true;
				Vector3 newUp = gs.GetUpDirection(GlobalPosition);
				UpDirection = newUp;
				AlignToUpSmooth(newUp, delta);
			}
		}
	}

	private void AlignToUpSmooth(Vector3 targetUp, double delta) {
		targetUp = targetUp.Normalized();

		Vector3 currentForward = -GlobalBasis.Z;

		Vector3 newForward = currentForward.Slide(targetUp).Normalized();

		if (newForward.LengthSquared() < 0.001f) {
			return;
		}

		Vector3 newRight = newForward.Cross(targetUp).Normalized();

		Basis targetBasis = new Basis(
			newRight,
			targetUp,
			-newForward
		).Orthonormalized();

		Quaternion current = GlobalBasis.GetRotationQuaternion();
		Quaternion target = targetBasis.GetRotationQuaternion();

		GlobalBasis = new Basis(current.Slerp(target, (float)delta * 8f));
	}

	private void UpdateInteractionPrompt() {
		_currentInteractable = null;

		if (playerMode == PlayerMode.Piloting) {
			return;
		}

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

	public void BuckleIn(ShipController ship) {
		if (_waitForInteractionRelease) {
			return;
		}
		
		playerMode = PlayerMode.Piloting;

		_savedLayer = CollisionLayer;
		_savedMask = CollisionMask;

		CollisionLayer = 0;
		CollisionMask = 0;
	}

	public void Unbuckle() {
		playerMode = PlayerMode.Walking;
		
		CollisionLayer = _savedLayer;
		CollisionMask = _savedMask;

		_waitForInteractionRelease = true;
	}
}
