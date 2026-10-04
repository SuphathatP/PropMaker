public class PropSettings
{
    public string ModelPath { get; set; }

    public double Mass { get; set; } = 1.0;

    public double Scale { get; set; } = 1.0;

    public string CollisionType { get; set; } = "Box";

    public bool IsDestructible { get; set; } = false;

    public string BrokenModelPath { get; set; } = "";

    public bool FollowBrokenModelScale { get; set; } = true;
    
    public double BrokenModelScale { get; set; } = 1.0;

    public PropSettings(string modelPath)
    {
        ModelPath = modelPath;
    }
}