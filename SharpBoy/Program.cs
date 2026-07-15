using GameBoyEmulator.Core;

namespace GameBoyEmulator;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("╔══════════════════════════════════╗");
        Console.WriteLine("║     Game Boy Emulator v0.2      ║");
        Console.WriteLine("╚══════════════════════════════════╝");
        Console.WriteLine();

        var memory = new Memory();
        var cpu = new CPU(memory);

        if (args.Length > 0)
        {
            string romPath = args[0];
            if (File.Exists(romPath))
            {
                byte[] rom = File.ReadAllBytes(romPath);
                memory.LoadRom(rom);
                Console.WriteLine($"ROM loaded: {romPath} ({rom.Length} bytes)");
            }
            else
            {
                Console.WriteLine($"ROM not found: {romPath}");
                return;
            }
        }
        else
        {
            Console.WriteLine("No ROM provided. Starting with empty memory.");
        }

        Console.WriteLine("CPU starting at 0x0100...");
        Console.WriteLine("Press any key to execute next instruction.");
        Console.WriteLine();

        cpu.Running = true;
        while (cpu.Running)
        {
            cpu.Step();
            Console.ReadKey();
        }
    }
}