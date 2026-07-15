using System;

namespace GameBoyEmulator.Core;

public enum JoypadButton
{
    Right = 0x01,
    Left = 0x02,
    Up = 0x04,
    Down = 0x08,
    A = 0x10,
    B = 0x20,
    Select = 0x40,
    Start = 0x80
}

public class Joypad
{
    private readonly Memory _memory;
    private byte _directionState;
    private byte _buttonState;
    private byte _lastJoyp;
    private byte _currentJoyp;

    public Joypad(Memory memory)
    {
        _memory = memory;
        _directionState = 0x0F;
        _buttonState = 0x0F;
        _lastJoyp = 0xFF;
        _currentJoyp = 0xFF;
        Update();
    }

    public void KeyDown(JoypadButton button)
    {
        byte bit = (byte)button;
        if ((bit & 0x0F) != 0)
        {
            _directionState = (byte)(_directionState & ~bit);
        }
        else
        {
            _buttonState = (byte)(_buttonState & ~bit);
        }
        Update();
    }

    public void KeyUp(JoypadButton button)
    {
        byte bit = (byte)button;
        if ((bit & 0x0F) != 0)
        {
            _directionState = (byte)(_directionState | bit);
        }
        else
        {
            _buttonState = (byte)(_buttonState | bit);
        }
        Update();
    }

    public void Refresh()
    {
        Update();
    }

    private void Update()
    {
        byte joyp = _memory.ReadIO(0xFF00);
        byte selected = (byte)((~joyp & 0x30) >> 4);

        byte pressed = 0x0F;

        if ((selected & 0x01) != 0)
        {
            pressed = (byte)(pressed & _directionState);
        }

        if ((selected & 0x02) != 0)
        {
            pressed = (byte)(pressed & _buttonState);
        }

        _currentJoyp = (byte)((joyp & 0xF0) | pressed);

        if (_currentJoyp != _lastJoyp)
        {
            _lastJoyp = _currentJoyp;
            _memory.WriteIO(0xFF00, _currentJoyp);

            byte newPressing = (byte)(_currentJoyp & 0x0F);
            byte oldPressing = (byte)(_lastJoyp & 0x0F);

            if ((newPressing & ~oldPressing) != 0)
            {
                _memory.SetInterruptFlag((byte)(_memory.GetInterruptFlag() | 0x10));
            }
        }
    }
}