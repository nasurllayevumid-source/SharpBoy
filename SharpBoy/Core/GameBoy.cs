using System;
using System.Diagnostics;
using System.Text;

namespace GameBoyEmulator.Core;

public class GameBoy
{
    private readonly Memory _memory;
    private readonly Registers _registers;
    private readonly CPU _cpu;
    private readonly PPU _ppu;
    private readonly Timer _timer;
    private readonly Joypad _joypad;
    private readonly Interrupts _interrupts;

    private bool _running;
    private readonly Stopwatch _stopwatch;
    private int _frames;
    private int _fps;
    private long _nextFpsUpdate;
    private double _nextFrameTime;
    private const int TARGET_FPS = 60;
    private const double FRAME_TIME_MS = 1000.0 / TARGET_FPS;

    private readonly StringBuilder _frameBuffer;
    private readonly char[] _pixelChars = { ' ', '░', '▒', '█' };

    public GameBoy()
    {
        _memory = new Memory();
        _registers = new Registers();
        _cpu = new CPU(_memory);
        _ppu = new PPU(_memory, _registers);
        _timer = new Timer(_memory);
        _joypad = new Joypad(_memory);
        _interrupts = new Interrupts(_memory, _registers, _cpu);

        _running = false;
        _stopwatch = new Stopwatch();
        _frames = 0;
        _fps = 0;
        _nextFpsUpdate = 0;
        _nextFrameTime = 0;
        _frameBuffer = new StringBuilder(160 * 145);
    }

    public void LoadRom(string romPath, string biosPath = null)
    {
        byte[] rom = System.IO.File.ReadAllBytes(romPath);
        _memory.LoadRom(rom);

        byte[] bios = null;

        if (!string.IsNullOrEmpty(biosPath) && System.IO.File.Exists(biosPath))
        {
            bios = System.IO.File.ReadAllBytes(biosPath);
        }
        else if (System.IO.File.Exists("gb_bios.bin"))
        {
            bios = System.IO.File.ReadAllBytes("gb_bios.bin");
        }
        else
        {
            bios = BootRom.GetDefaultBootRom();
        }

        if (bios != null && bios.Length > 0)
        {
            _memory.LoadBootRom(bios);
            _registers.PC = 0x0000;
        }
        else
        {
            _cpu.LoadPostBootState();
        }
    }

    public void Run()
    {
        _running = true;
        _stopwatch.Start();

        double currentTime = GetCurrentTime();
        _nextFpsUpdate = (long)currentTime + 1000;
        _nextFrameTime = currentTime + FRAME_TIME_MS;

        while (_running)
        {
            int cycles = _cpu.Step();
            _timer.Step(cycles);
            _ppu.Step(cycles);
            _interrupts.Step();

            ProcessInput();

            if (_ppu.FrameReady)
            {
                RenderFrame();
                _ppu.ResetFrameReady();

                _frames++;
                long now = (long)GetCurrentTime();
                if (now >= _nextFpsUpdate)
                {
                    _fps = _frames;
                    _frames = 0;
                    _nextFpsUpdate = now + 1000;
                }

                LimitFPS();
            }
        }

        _stopwatch.Stop();
        Console.WriteLine("\nEmulation stopped.");
    }

    private double GetCurrentTime()
    {
        return (double)_stopwatch.ElapsedTicks / Stopwatch.Frequency * 1000.0;
    }

    private void LimitFPS()
    {
        double currentTime = GetCurrentTime();
        double sleepTime = _nextFrameTime - currentTime;

        if (sleepTime > 2.0)
        {
            System.Threading.Thread.Sleep((int)sleepTime - 1);
        }
        else if (sleepTime > 0.0)
        {
            System.Threading.Thread.Sleep(0);
        }

        if (currentTime > _nextFrameTime + 100.0)
        {
            _nextFrameTime = currentTime;
        }

        _nextFrameTime += FRAME_TIME_MS;
    }

    private void ProcessInput()
    {
        while (Console.KeyAvailable)
        {
            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Escape)
            {
                _running = false;
                return;
            }

            var button = KeyToButton(key.Key);
            if (button.HasValue)
            {
                _joypad.KeyDown(button.Value);
            }
        }
    }

    private Core.JoypadButton? KeyToButton(ConsoleKey key)
    {
        return key switch
        {
            ConsoleKey.RightArrow => Core.JoypadButton.Right,
            ConsoleKey.LeftArrow => Core.JoypadButton.Left,
            ConsoleKey.UpArrow => Core.JoypadButton.Up,
            ConsoleKey.DownArrow => Core.JoypadButton.Down,
            ConsoleKey.X or ConsoleKey.A => Core.JoypadButton.A,
            ConsoleKey.Z or ConsoleKey.S => Core.JoypadButton.B,
            ConsoleKey.Enter => Core.JoypadButton.Start,
            ConsoleKey.Space => Core.JoypadButton.Select,
            _ => null
        };
    }

    private void RenderFrame()
    {
        try
        {
            Console.SetCursorPosition(0, 0);

            _frameBuffer.Clear();

            for (int y = 0; y < 144; y++)
            {
                for (int x = 0; x < 160; x++)
                {
                    _frameBuffer.Append(_pixelChars[_ppu.FrameBuffer[y, x]]);
                }
                _frameBuffer.AppendLine();
            }

            _frameBuffer.Append($"FPS: {_fps}  |  Frames: {_frames}");
            _frameBuffer.AppendLine();
            _frameBuffer.Append("Press ESC to stop");

            Console.Write(_frameBuffer.ToString());
        }
        catch (ArgumentOutOfRangeException)
        {
            Console.Clear();
            Console.WriteLine("Console too small! Please resize to at least 160x150.");
            Console.WriteLine($"Current FPS: {_fps}");
        }
    }

    public void Stop()
    {
        _running = false;
    }

    public void Reset()
    {
        _cpu.Reset();
        _ppu.ResetFrameReady();
        _timer.Reset();
        _memory.Reset();
        _joypad.Reset();
        _interrupts.Reset();
        _frames = 0;
        _fps = 0;
        _nextFpsUpdate = 0;
        _nextFrameTime = 0;
    }
}