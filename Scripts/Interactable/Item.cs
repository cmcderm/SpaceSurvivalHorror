using Godot;
using SpaceSurvivalHorror.Scripts.Interfaces;
using SpaceSurvivalHorror.Scripts.Player;

namespace SpaceSurvivalHorror.Scripts.Interactable;

public partial class Item : Node3D, IInteractable
{
	public string GetPrompt() {
		return "Press 'e' to Interact";
	}
	
	public void Interact(PlayerController player) {
		GD.Print("Item Interacted");
	}
}
