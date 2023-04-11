using Godot;
using System;

public partial class TacticsCamera : CharacterBody3D
{
	const int MoveSpeed = 16;
	const int RotSpeed = 10;
	public int XRot = -30;
	public int YRot = -45;
	public PhysicsBody3D Target = null;
	Node3D Pivot;

	public void MoveCamera(float h, float v, bool joystick)
	{
		if (!joystick
			&& h == 0
			&& v == 0
			|| Target != null)
			return;
		float angle = (Godot.Mathf.Atan2(-h, v)) + Pivot.Rotation.Y;
		Vector3 direction = Vector3.Forward.Rotated(Vector3.Up, angle);
		Vector3 velocity = direction * MoveSpeed;
		if (joystick)
		{
			velocity = velocity * Godot.Mathf.Sqrt(h * h + v * v);
			velocity = velocity * Vector3.Up;
		}
		MoveAndSlide();
	}

	public void RotateCamera(double delta)
	{
		Vector3 CurrentRotation = Pivot.Rotation;
		float x = Godot.Mathf.DegToRad(XRot);
		float y = Godot.Mathf.DegToRad(YRot);
		Vector3 destinationRotation = new Vector3(x, y, 0);
		Pivot.Rotation = CurrentRotation.Lerp(destinationRotation, RotSpeed * (float)delta);
	}

	public override void _Ready()
	{
		Pivot = GetNode<Node3D>("Pivot");
	}

	public void Follow()
	{
		if (Target == null)
		{
			return;
		}
		Vector3 from = GlobalTransform.Origin;
		Vector3 to = Target.GlobalTransform.Origin;
		//TODO velocity work
		Vector3 velocity = ((to - from) * MoveSpeed / 4); //* Vector3.Up;
		MoveAndSlide();
		if (from.DistanceTo(to) <= 0.25)
		{
			Target = null;
		}
	}

	public override void _Process(double delta)
	{
		RotateCamera(delta);
		Follow();
	}

}
