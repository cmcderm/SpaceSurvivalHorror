using Godot;
using System.Collections.Generic;
using System.Text;

public partial class DebugOverlay : CanvasLayer
{
	public static DebugOverlay Instance { get; private set; }
	
	private readonly Dictionary<string, string> _values = new();
	private readonly List<string> _order = new();

	private Label _label;
	
	public override void _Ready() {
		Instance = this;
		_label = GetNode<Label>("PanelContainer/VBoxContainer/Label");

		Visible = false;
	}

	public override void _Process(double delta) {
		var sb = new StringBuilder();

		foreach (string key in _order) {
			sb.Append(key);
			sb.Append(": ");
			sb.AppendLine(_values[key]);
		}

		_label.Text = sb.ToString();
	}

	public override void _Input(InputEvent @event) {
		if (@event is InputEventKey eventKey) {
			if (eventKey.Pressed && eventKey.Keycode == Key.Quoteleft) {
				Visible = !Visible;
			}
		}
	}

	public static void SetValue(string key, object value) {
		if (Instance == null) {
			return;
		}	
		
		if (!Instance._values.ContainsKey(key)) {
			Instance._order.Add(key);	
		}

		Instance._values[key] = value?.ToString();
	}

	public static void RemoveValue(string key) {
		if (Instance == null) {
			return;
		}

		if (Instance._values.Remove(key)) {
			Instance._order.Remove(key);
		}
	}

	public static void ClearValues() {
		if (Instance == null) {
			return;
		}

		Instance._values.Clear();
		Instance._order.Clear();
	}
}
