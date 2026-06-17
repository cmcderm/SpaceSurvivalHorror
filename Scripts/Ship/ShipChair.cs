using Godot;
using System;
using SpaceSurvivalHorror.Scripts.Interfaces;
using SpaceSurvivalHorror.Scripts.Player;

public partial class ShipChair : Node3D, IInteractable
{
	[Export] private ShipController _shipController;
	
	public string GetPrompt() {
		return "Press 'e' to Buckle In";
	}

	public void Interact(PlayerController player) {
		_shipController.BuckleIn(player);
	}
}
