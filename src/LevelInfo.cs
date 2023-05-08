public class LevelInfo
{
    public int RoundsToWin { get; set; }
    public Godot.Collections.Array<PawnClass> AllowedEnemies { get; set; }

    public string LevelPath { get; set; }
}