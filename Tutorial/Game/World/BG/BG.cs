using Godot;

namespace Game;

public partial class BG : Node
{
    #region Private

    private MeshInstance3D Floor => field ??= (MeshInstance3D)GetNode("Floor");

    #endregion

    #region Godot

    public sealed override void _Ready()
    {
    }

    #endregion
}
