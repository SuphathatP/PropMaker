#if TOOLS
using Godot;

[Tool]
public partial class PropMakerPlugin : EditorPlugin
{
	private EditorDock dock;
	public override void _EnterTree()
	{
		GD.Print("Prop Maker plugin loaded successfully!");

		PropMakerDock dockControl = new PropMakerDock();
		dockControl.Name = "PropMakerDock";

		dock = new EditorDock
		{
			Title = "Prop Maker",
			DefaultSlot = EditorDock.DockSlot.RightUl
		};

		dock.AddChild(dockControl);

		AddDock(dock);
	}

	public override void _ExitTree()
	{
		GD.Print("Prop Maker plugin unloaded!");

		RemoveDock(dock);
		dock.QueueFree();
	}
}
#endif
