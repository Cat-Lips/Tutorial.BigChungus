using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
using Nebula.Utility.Tools;

namespace Game;

public partial class Utils : Node
{
    public sealed override void _Ready()
    {
        ParseDebugCmdLine(this);

        [Conditional("DEBUG")]
        static void ParseDebugCmdLine(Node self)
        {
            if (Engine.IsEditorHint()) return;
            if (IsLocal()) InitLocalInstance();
            else InitChildInstance();

            bool IsLocal()
                => !Env.Instance.StartArgs.ContainsKey("title");

            void InitLocalInstance()
            {
                if (TryGetLaunchList(out var items))
                {
                    if (TryGetScreenLayout(out var grid, out var idx, out var windowSize, out var posOffset, out var sizeOffset))
                    {
                        LaunchItems(grid, idx, windowSize, posOffset, sizeOffset);
                        SetFocus();
                        SetTitle();
                    }
                    else
                    {
                        foreach (var item in items)
                            Launch(item);
                    }
                }

                bool TryGetLaunchList(out string[] items)
                {
                    items = [.. LaunchList()];
                    return items.Length is not 0;

                    IEnumerable<string> LaunchList()
                    {
                        if (Env.Instance.StartArgs.TryGetValue("with-server", out var type))
                        {
                            if (type is "headless")
                                Launch("--server --headless --title=server:headless --log-file godot.server.headless.log");
                            else yield return "--server --title=server --log-file godot.server.log";
                        }

                        if (Env.Instance.StartArgs.TryGetValue("with-clients", out var _count) && int.TryParse(_count, out var count))
                        {
                            for (var i = 0; i < count; ++i)
                                yield return $"--client --title=client.{i + 1} --log-file godot.client.{i + 1}.log";
                        }
                    }
                }

                bool TryGetScreenLayout(out int grid, out int idx, out Vector2I windowSize, out Vector2I posOffset, out Vector2I sizeOffset)
                {
                    if (DisplayServer.WindowCanDraw())
                    {
                        posOffset = -(DisplayServer.WindowGetPositionWithDecorations() - DisplayServer.WindowGetPosition());
                        sizeOffset = DisplayServer.WindowGetSizeWithDecorations() - DisplayServer.WindowGetSize();
                        var screenRect = DisplayServer.ScreenGetUsableRect();

                        grid = Mathf.CeilToInt(Mathf.Sqrt(items.Length + (idx = 1)));
                        windowSize = new Vector2I(
                            screenRect.Size.X / grid - sizeOffset.X,
                            screenRect.Size.Y / grid - sizeOffset.Y);

                        DisplayServer.WindowSetPosition(posOffset);
                        DisplayServer.WindowSetSize(windowSize);

                        GD.Print($"ScreenRect: {screenRect}, WindowPosOffset: {posOffset}, WindowSizeOffset: {sizeOffset}");
                        GD.Print($"[LOCAL] WindowPos: {DisplayServer.WindowGetPosition()}, WindowSize: {DisplayServer.WindowGetSize()}");

                        return true;
                    }
                    else
                    {
                        windowSize = posOffset = sizeOffset = default;
                        grid = idx = default;
                        return false;
                    }
                }

                void LaunchItems(int grid, int idx, in Vector2I windowSize, in Vector2I posOffset, in Vector2I sizeOffset)
                {
                    for (var i = 0; i < items.Length; ++i, ++idx)
                    {
                        var row = idx / grid;
                        var col = idx % grid;

                        var x = col * windowSize.X + col * sizeOffset.X + posOffset.X;
                        var y = row * windowSize.Y + row * sizeOffset.Y + posOffset.Y;
                        var w = windowSize.X;
                        var h = windowSize.Y;

                        Launch(items[i],
                            $"--position {x},{y}",
                            $"--resolution {w}x{h}");
                    }
                }

                void Launch(params string[] args)
                {
                    args = args.SelectMany(x => x.Split()).Concat(PassArgs()).ToArray();
                    GD.Print($"[LAUNCH] Godot {string.Join(" ", args)}");
                    KillOnExit(OS.CreateInstance(args));

                    void KillOnExit(int pid)
                        => self.TreeExiting += () => OS.Kill(pid);

                    IEnumerable<string> PassArgs()
                    {
                        foreach (var kvp in Env.Instance.StartArgs)
                        {
                            if (kvp.Key is
                                "title" or
                                "server" or
                                "client" or
                                "log-file" or
                                "headless" or
                                "position" or
                                "resolution" or
                                "with-server" or
                                "with-clients") continue;

                            yield return kvp.Value is null or ""
                                ? $"--{kvp.Key}"
                                : $"--{kvp.Key}={kvp.Value}";
                        }
                    }
                }

                void SetFocus()
                {
                    if (DisplayServer.WindowCanDraw())
                    {
                        var curRunTime = Time.GetTicksMsec();
                        var delayInSeconds = curRunTime * .0012f;
                        GD.Print($"[FOCUS] Waiting {delayInSeconds}s (runtime: {curRunTime}ms + 20%)");
                        self.GetTree().CreateTimer(delayInSeconds).Timeout += SetFocus;

                        void SetFocus()
                        {
                            DisplayServer.WindowMoveToForeground();
                            GD.Print($"[FOCUS] Done");
                        }
                    }
                }

                void SetTitle()
                {
                    self.GetTree().Root.Title = string.Join(":", Parts());

                    IEnumerable<string> Parts()
                    {
                        if (Env.Instance.StartArgs.ContainsKey("server")) yield return "Server";
                        if (Env.Instance.StartArgs.ContainsKey("client")) yield return "Client";
                    }
                }
            }

            void InitChildInstance()
            {
                SetTitle();

                void SetTitle()
                {
                    if (Env.Instance.StartArgs.TryGetValue("title", out var title) && title is not null or "")
                        self.GetTree().Root.Title = title;
                }
            }
        }
    }
}
