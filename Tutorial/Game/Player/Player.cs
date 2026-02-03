using System;
using Godot;
using Nebula;

namespace Game;

public partial class Player : NetNode
{
    #region Private

    private MeshInstance3D Model => field ??= (MeshInstance3D)GetNode("Model");
    private PlayerController Controller => field ??= (PlayerController)GetNode("Controller");
    private StandardMaterial3D Material => field ??= (StandardMaterial3D)Model.GetActiveMaterial(0)?.Duplicate() ?? new();

    private const float ScoreScaleDivisor = 50f;
    private const float ScaleLerpSpeed = 8f;
    private const float BaseScale = 1f;

    private const float MercyTimer = 3f;

    private Vector3 TargetScale { get; set; } = Vector3.One * BaseScale;
    private float GetScoreScale() => BaseScale + Score / ScoreScaleDivisor;

    #endregion

    public event Action PlayerDead;
    public event Action<int> ScoreChanged;
    public static event Action<Player> PlayerReady;

    public float GetCollisionRadius() => GetScoreScale();
    public Vector3 GetWorldPosition() => Controller.GlobalPosition;

    #region Score

    [NetProperty(NotifyOnChange = true)]
    public int Score { get; set; }

    protected virtual void OnNetChangeScore(int _1, int _2, int _3)
    {
        if (Network.IsCurrentOwner)
            ScoreChanged?.Invoke(Score);
        TargetScale = Vector3.One * GetScoreScale();
    }

    #endregion

    #region Color

    public Color Color { get; private set; }

    [NetProperty(NotifyOnChange = true)]
    private ulong ColorSeed { get; set; }

    protected virtual void OnNetChangeColorSeed(int _1, ulong _2, ulong _3)
    {
        var rng = new RandomNumberGenerator { Seed = ColorSeed };
        Color = new Color(rng.Randf(), rng.Randf(), rng.Randf());
        Material.AlbedoColor = Color;
        Material.Emission = Color;
    }

    #endregion

    #region Mercy

    [NetProperty]
    public float MercyTime { get; private set; }

    #endregion

    #region Godot

    public sealed override void _Ready()
    {
        MercyTime = MercyTimer;
        Model.MaterialOverlay = Material;

        Material.EmissionEnabled = true;
    }

    public sealed override void _Process(double _delta)
    {
        var delta = (float)_delta;

        var t = 1f - Mathf.Exp(-ScaleLerpSpeed * delta);
        Model.Scale = Model.Scale.Lerp(TargetScale, t);

        if (MercyTime > 0)
        {
            if ((MercyTime = Math.Max(MercyTime - delta, 0)) > 0)
            {
                var flashRate = MercyTime / MercyTimer;
                var pulseRate = Mathf.Abs(Mathf.Sin(flashRate * 20f));
                Material.EmissionEnergyMultiplier = pulseRate * 2f;
            }
            else
            {
                Material.EmissionEnabled = false;
                Material.EmissionEnergyMultiplier = 0;
            }
        }
    }

    #endregion

    #region Nebula

    public sealed override void _WorldReady()
    {
        InitClient();
        InitServer();

        void InitClient()
        {
            if (!Network.IsClient) return;
            if (!Network.IsCurrentOwner) return;

            var camera = new Camera3D();
            Model.AddChild(camera);
            camera.Position = Vector3.Up * 10;
            camera.LookAt(Model.GlobalPosition, up: Vector3.Forward);

            PlayerReady?.Invoke(this);
        }

        void InitServer()
        {
            if (!Network.IsServer) return;

            ColorSeed = GD.Randi();
            PlayerReady?.Invoke(this);
        }
    }

    public override void _Despawn()
    {
        if (!Network.IsClient) return;
        if (!Network.IsCurrentOwner) return;

        PlayerDead?.Invoke();
    }

    #endregion
}
