using System.IO;
using System.Reflection;
using Godot;

namespace Game;

public static class World
{
    private const int Extent = 50;

    public static Vector3 GetMax(float inset) => new(Extent - inset, 0, Extent - inset);
    public static Vector3 GetMin(float inset) => new(-Extent + inset, 0, -Extent + inset);

    private static float RandF(float inset) => (float)GD.RandRange(-Extent + inset, Extent - inset);
    public static Vector3 RandPos(float inset, float y = 0) => new(RandF(inset), y, RandF(inset));

    private static readonly PackedScene PlayerScene = LoadScene<Player>();
    public static Player NewPlayer() => (Player)PlayerScene.Instantiate();

    private static PackedScene LoadScene<T>() where T : Node
    {
        var csPath = typeof(T).GetCustomAttribute<ScriptPathAttribute>(false).Path;
        var resPath = Path.ChangeExtension(csPath, "tscn");
        return GD.Load<PackedScene>(resPath);
    }
}
