namespace GameBoyEmulator.Core.APU;

public class FrameSequencer
{
    private int _step;
    private int _timer;
    private const int FREQUENCY = 8192;

    public FrameSequencer()
    {
        Reset();
    }

    public void Reset()
    {
        _step = 0;
        _timer = 0;
    }

    public void Step(int cycles, Action<int> onStep)
    {
        _timer += cycles;

        while (_timer >= FREQUENCY)
        {
            _timer -= FREQUENCY;
            _step = (_step + 1) & 0x07;
            onStep?.Invoke(_step);
        }
    }
}