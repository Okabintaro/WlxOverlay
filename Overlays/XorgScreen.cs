using WlxOverlay.Desktop;
using WlxOverlay.GFX;
using WlxOverlay.Numerics;
using WlxOverlay.Overlays.Simple;
using WlxOverlay.Types;

namespace WlxOverlay.Overlays;

public class XorgScreen : BaseScreen<BaseOutput>
{
    private static Vector2Int _mousePos;
    private static bool _mousePosSet;

    private static Rect2 _outputRect;

    protected override Rect2 OutputRect => _outputRect;

    public static int NumScreens()
    {
        return wlxshm_num_screens();
    }

    private readonly IntPtr _handle;
    private readonly uint _bufSize;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public XorgScreen(int screen) : base(new BaseOutput(screen.ToString()))
    {
        Vector2Int size = new(), pos = new();

        _handle = wlxshm_create(screen, ref size, ref pos);

        if (_handle == IntPtr.Zero)
            throw new ApplicationException("Could not initialize Xorg screen capture!");

        _bufSize = (uint)(size.X * size.Y * 4U);

        Screen.Name = screen.ToString();
        Screen.Size = size;
        Screen.Position = pos;

        _outputRect = _outputRect.Merge(new Rect2((Vector2)pos, (Vector2)size));
    }

    public override void Show()
    {
        base.Show();
        _semaphore.Wait();
        wlxshm_capture_start(_handle);
        _semaphore.Release();
    }

    public override void Hide()
    {
        base.Hide();
        _semaphore.Wait();
        wlxshm_capture_end(_handle);
        _semaphore.Release();
    }

    protected internal override void AfterInput(bool batteryStateUpdated)
    {
        _mousePosSet = false;
        base.AfterInput(batteryStateUpdated);
    }
    
    private Task<IntPtr> _captureTask = Task.Run(() => IntPtr.Zero);
    private async Task<IntPtr> CaptureNextFrame()
    {
        await _semaphore.WaitAsync();
        var ptr = wlxshm_capture_frame(_handle);
        _semaphore.Release();
        return ptr;
    }

    protected internal override unsafe void Render()
    {
        if (!_mousePosSet)
        {
            _semaphore.Wait();
            wlxshm_mouse_pos_global(_handle, ref _mousePos);
            _semaphore.Release();
            _mousePosSet = true;
        }
        
        if (_captureTask.IsCompleted)
        {
            var buf = (buf_t*) _captureTask.Result.ToPointer();
            if (buf != null && buf->length == _bufSize)
                Texture!.LoadRawImage(buf->buffer, GraphicsFormat.BGRA8);

            _captureTask.Dispose();
            _captureTask = Task.Run(CaptureNextFrame);
        }

        var mouse = new Vector2Int(_mousePos.X - Screen.Position.X, _mousePos.Y - Screen.Position.Y);

        if (mouse.X >= 0 && mouse.X < Screen.Size.X && mouse.Y >= 0 && mouse.Y < Screen.Size.Y)
        {
            if (Config.Instance.FallbackCursors)
            {
                DrawFallbackCross(mouse.X, mouse.Y, Vector3.One, 8);
                DrawFallbackCross(mouse.X + 1, mouse.Y + 1, Vector3.Zero, 8);
            }
            else
            {
                var uv = new Vector2(mouse.X / (float)Screen.Size.X, mouse.Y / (float)Screen.Size.Y);
                var moveToTransform = CurvedSurfaceTransformFromUv(uv);
                DesktopCursor.Instance.MoveTo(moveToTransform);
            }
        }

        base.Render();
    }

    public override string ToString()
    {
        return $"Scr {Screen}";
    }

    public override void Dispose()
    {
        _semaphore.Wait();
        wlxshm_destroy(_handle);
        _semaphore.Release();
        _semaphore.Dispose();
        base.Dispose();
    }

    [DllImport("libwlxshm.so", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr wlxshm_create(int screen, ref Vector2Int size, ref Vector2Int pos);

    [DllImport("libwlxshm.so", CallingConvention = CallingConvention.Cdecl)]
    private static extern void wlxshm_destroy(IntPtr handle);

    [DllImport("libwlxshm.so", CallingConvention = CallingConvention.Cdecl)]
    private static extern int wlxshm_capture_start(IntPtr handle);

    [DllImport("libwlxshm.so", CallingConvention = CallingConvention.Cdecl)]
    private static extern void wlxshm_capture_end(IntPtr handle);

    [DllImport("libwlxshm.so", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr wlxshm_capture_frame(IntPtr handle);

    [DllImport("libwlxshm.so", CallingConvention = CallingConvention.Cdecl)]
    private static extern void wlxshm_mouse_pos_global(IntPtr handle, ref Vector2Int pos);

    [DllImport("libwlxshm.so", CallingConvention = CallingConvention.Cdecl)]
    private static extern int wlxshm_num_screens();

    [StructLayout(LayoutKind.Sequential)]
    private struct buf_t
    {
        public int length;
        public IntPtr buffer;
    }
}