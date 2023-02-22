using Godot;
using System;

public class TacticsCamera : KinematicBody
{
    const int MoveSpeed = 16;
    const int RotSpeed = 10;

    public int XRot = -30;
    public int YRot = -45;
    public PhysicsBody Target = null;
    Spatial Pivot;

    public TacticsCamera()
    {
        // _Ready();
    }

    public void MoveCamera(float h, float v, bool joystick)
    {
        if (!joystick && h == 0 && v == 0 || Target != null)
            return;
        float angle = (Godot.Mathf.Atan2(-h, v)) + Pivot.Rotation.y;
        Vector3 direction = Vector3.Forward.Rotated(Vector3.Up, angle);
        Vector3 velocity = direction * MoveSpeed;
        if (joystick)
            velocity = velocity * Godot.Mathf.Sqrt(h * h + v * v);
        velocity = MoveAndSlide(velocity, Vector3.Up);
    }

    public void RotateCamera(float delta)
    {
        Vector3 CurrentRotation = Pivot.Rotation;
        float x = Godot.Mathf.Deg2Rad(XRot);
        float y = Godot.Mathf.Deg2Rad(YRot);
        Vector3 destinationRotation = new Vector3(x, y, 0);
        Pivot.Rotation = CurrentRotation.LinearInterpolate(destinationRotation, RotSpeed * delta);
    }

    public override void _Ready()
    {
        Pivot = GetNode<Spatial>("Pivot");
    }

    public void Follow()
    {
        if (Target == null)
            return;
        Vector3 from = GlobalTransform.origin;
        Vector3 to = Target.GlobalTransform.origin;
        // Vector3 to = IsInstanceValid(Target)
        //     ? Target.GlobalTransform.origin
        //     : Vector3.Zero;
        Vector3 velocity = (to - from) * MoveSpeed / 4;
        velocity=MoveAndSlide(velocity, Vector3.Up);
        if (from.DistanceTo(to) <= 0.25)
            Target = null;
    }

    public override void _Process(float delta)
    {
        RotateCamera(delta);
        Follow();
    }




}
