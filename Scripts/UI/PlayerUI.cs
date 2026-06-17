using Godot;
using System;
using SpaceSurvivalHorror.Scripts.Player;

public partial class PlayerUI : CanvasLayer {
	private Label _interactionPrompt;
	
	public override void _Ready()
    {
        _interactionPrompt = GetNode<Label>("InteractionPrompt");
        _interactionPrompt.Visible = false;
    }

	public void ShowInteractionPrompt(string text) {
		_interactionPrompt.Text = text;
		_interactionPrompt.Visible = true;
	}

	public void HideInteractionPrompt() {
		_interactionPrompt.Visible = false;
	}
}
