using Godot;
using Nebula;
using Nebula.Serialization;

namespace Game;

public partial class Pellets : NetNode
{
    #region Private

    private MultiMesh MultiMesh => field ??= ((MultiMeshInstance3D)GetNode("MultiMesh")).Multimesh;

    #endregion

    #region Positions

    [NetProperty(NotifyOnChange = true)]
    public NetArray<Vector3> PelletPositions { get; private set; } = new(2000, 2000);

    protected virtual void OnNetChangePelletPositions(int _1, Vector3[] _2, int[] changedIndices, Vector3[] _3)
    {
        if (MultiMesh.InstanceCount != PelletPositions.Length)
            MultiMesh.InstanceCount = PelletPositions.Length;

        for (var i = 0; i < changedIndices.Length; ++i)
        {
            var idx = changedIndices[i];
            var pos = PelletPositions[idx];
            MultiMesh.SetInstanceTransform(idx, new Transform3D(Basis.Identity, pos));
            MultiMesh.SetInstanceColor(idx, GetPelletColor(pos));
        }

        static Color GetPelletColor(in Vector3 pos)
        {
            const float period = 20f;
            var tx = Fract(pos.X / period);
            var tz = Fract(pos.Z / period);
            return new Color(tx, .35f, tz);

            static float Fract(float x)
                => x - Mathf.Floor(x);
        }
    }

    #endregion

    #region Respawn

    public void Respawn(int i)
        => PelletPositions[i] = World.RandPos(inset: .5f, y: .1f);

    #endregion

    #region Nebula

    public sealed override void _WorldReady()
    {
        InitServer();

        void InitServer()
        {
            if (Network.IsServer)
                InitPellets();

            void InitPellets()
            {
                for (var i = 0; i < PelletPositions.Capacity; ++i)
                    Respawn(i);
            }
        }
    }

    #endregion
}
