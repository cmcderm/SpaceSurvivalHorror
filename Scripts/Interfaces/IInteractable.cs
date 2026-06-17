using Godot;
using SpaceSurvivalHorror.Scripts.Player;

namespace SpaceSurvivalHorror.Scripts.Interfaces;

public interface IInteractable {
    string GetPrompt();
    void Interact(PlayerController player);
}