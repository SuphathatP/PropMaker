using Godot;
using System;

public partial class DestructibleProp : RigidBody3D
{
    [Export] public PackedScene BrokenPrefab { get; set; }

    public void Break()
    {
        GD.Print($"Breaking: {Name}");

        if (BrokenPrefab == null)
        {
            GD.PrintErr("No broken prefab assigned.");
            return;
        }

        Node3D brokenObject = BrokenPrefab.Instantiate<Node3D>();

        GetParent().AddChild(brokenObject);

        brokenObject.GlobalPosition = GlobalPosition;
        brokenObject.GlobalRotation = GlobalRotation;
        
        GD.Print($"Broken model scale: {brokenObject.Scale}");

        QueueFree();
    }
}
