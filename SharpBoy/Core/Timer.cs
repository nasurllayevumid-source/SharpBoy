public void Reset()
{
    _systemCounter = 0;
    _lastDiv = 0;
    _lastTima = 0;
    _timaOverflowPending = 0;
    _lastTac = 0;
    _memory.WriteIO(0xFF04, 0);
    _memory.WriteIO(0xFF05, 0);
    _memory.WriteIO(0xFF06, 0);
    _memory.WriteIO(0xFF07, 0);
}