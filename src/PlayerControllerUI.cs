using Godot;
using System;

public class PlayerControllerUI : Control
{
    public Texture LayoutXbox = ResourceLoader.Load<Texture>("res://assets/sprites/labels/controls-ui-xbox.png");
    public Texture LayoutPC = ResourceLoader.Load<Texture>("res://assets/sprites/labels/controls-ui.png");
    public bool IsJoyStick = false;

    private TextureRect _ControllerHints;
    private VBoxContainer _Actions;

    public PlayerControllerUI()
    {
        // _Ready();
    }

    public override void _Ready()
    {
        _ControllerHints = GetNode<TextureRect>("HBox/VBox/ControllerHints");
        _Actions = GetNode<VBoxContainer>("HBox/Actions");
    }

    public override void _Process(float delta)
    {
        if (IsJoyStick)
            _ControllerHints.Texture = LayoutXbox;
        else
            _ControllerHints.Texture = LayoutPC;
    }

    public Button GetAct(string action)
    {
        return _Actions.GetNode(action) as Button;
    }

    public bool IsMouseHoverButton()
    {
        if(_Actions.Visible)
        {
            //This all should be buttons
            Godot.Collections.Array buttons = _Actions.GetChildren();
            foreach(Button action in buttons)
            {
                Vector2 mousePosition = GetViewport().GetMousePosition();
                if(action.GetGlobalRect().HasPoint(mousePosition))
                    return true;
            }
        }
        return false;
    }

    public void SetVisibilityOfActionsMenu(bool visible, Pawn pawn)
    {
        Button btnMove = GetNode<Button>("HBox/Actions/Move");
        Button btnAttack = GetNode<Button>("HBox/Actions/Attack");

        if(!_Actions.Visible)
            btnMove.GrabFocus();
        _Actions.Visible = visible;
        if(pawn==null)
            return;

        btnMove.Disabled = !pawn.CanMove;
        btnAttack.Disabled = !pawn.CanAttack;
    }



}
