using System.Runtime.InteropServices;
using Godot;
using Nebula;

namespace Game;

public partial class PlayerController : NetNode3D
{
    #region Private

    private static readonly StringName Up = "ui_up";
    private static readonly StringName Down = "ui_down";
    private static readonly StringName Left = "ui_left";
    private static readonly StringName Right = "ui_right";

    private const float BaseSpeed = .5f;
    private const float ScoreSlowdown = .05f;

    private Player Player => field ??= (Player)GetParent();

    #endregion

    [NetProperty]
    public Vector3 Direction { get; set; }

    #region Godot

    public sealed override void _Process(double _)
    {
        Network.SetInput(new PlayerInput
        {
            Up = Input.IsActionPressed(Up),
            Down = Input.IsActionPressed(Down),
            Left = Input.IsActionPressed(Left),
            Right = Input.IsActionPressed(Right),
        });
    }

    #endregion

    #region Nebula

    public sealed override void _WorldReady()
    {
        Network.InitializeInput<PlayerInput>();
        GlobalPosition = World.RandPos(Player.GetCollisionRadius());
    }

    public sealed override void _NetworkProcess(int tick)
    {
        UpdatePosition();

        void UpdatePosition()
        {
            ref readonly var input = ref Network.GetInput<PlayerInput>();

            var dir = Vector3.Zero;

            if (input.Up) dir.Z -= 1;
            if (input.Down) dir.Z += 1;
            if (input.Left) dir.X -= 1;
            if (input.Right) dir.X += 1;

            if (dir != Vector3.Zero)
                Direction = dir.Normalized();

            var inset = Player.GetCollisionRadius() * .5f;
            var speed = BaseSpeed / (1f + Player.Score * ScoreSlowdown);
            Position = (Position + Direction * speed).Clamp(World.GetMin(inset), World.GetMax(inset));
        }
    }

    #endregion

    #region Private

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct PlayerInput
    {
        public bool Up;
        public bool Down;
        public bool Left;
        public bool Right;
    }

    #endregion
}
