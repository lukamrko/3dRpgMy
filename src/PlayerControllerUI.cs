using Godot;
using System;

public class PlayerControllerUI : Control
{
    public Texture LayoutXbox = ResourceLoader.Load<Texture>("res://assets/sprites/labels/controls-ui-xbox.png");
    public Texture LayoutPC = ResourceLoader.Load<Texture>("res://assets/sprites/labels/controls-ui.png");

    private Godot.Collections.Array<Button> Buttons;

    private Button _BtnMove;
    private Button _BtnAttack;
    public bool IsJoyStick = false;

    private TextureRect _ControllerHints;
    private VBoxContainer _Actions;

    public override void _Ready()
    {
        _ControllerHints = GetNode<TextureRect>("HBox/VBox/ControllerHints");
        _Actions = GetNode<VBoxContainer>("HBox/Actions");
        _BtnMove = GetNode<Button>("HBox/Actions/Move");
        _BtnAttack = GetNode<Button>("HBox/Actions/Attack");
        Buttons = _Actions.GetChildren().As<Button>();
    }

    public override void _Process(float delta)
    {
        if (IsJoyStick)
        {
            _ControllerHints.Texture = LayoutXbox;
        }
        else
        {
            _ControllerHints.Texture = LayoutPC;
        }
    }

    public Button GetAct(string action)
    {
        return _Actions.GetNode(action) as Button;
    }

    public bool IsMouseHoverButton()
    {
        if (_Actions.Visible)
        {
            //This all should be buttons
            // Godot.Collections.Array buttons = _Actions.GetChildren();
            foreach (Button action in Buttons)
            {
                Vector2 mousePosition = GetViewport().GetMousePosition();
                if (action.GetGlobalRect().HasPoint(mousePosition))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void SetVisibilityOfActionsMenu(bool visible, PlayerPawn pawn)
    {
        if (!_Actions.Visible)
        {
            _BtnMove.GrabFocus();
        }

        _Actions.Visible = visible;
        if (pawn is null)
        {
            return;
        }

        _BtnMove.Disabled = !pawn.CanMove;
        _BtnAttack.Disabled = !pawn.CanAttack;
    }

}
