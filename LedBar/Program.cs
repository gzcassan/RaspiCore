using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RaspiCore;

namespace LedBar
{
    enum LedMode : Byte
    {
        LTR,
        RTL,
        SuperCar,
        Syren
    }

    static class Program
    {
        static Boolean ShuttingDown = false;
        static UInt16[] Speeds = new UInt16[] { 1000, 500, 250, 100, 50 };

        static Byte SpeedIndex = 0;
        static UInt16 Speed = 1000;
        static LedMode Mode = LedMode.LTR;

        static void Main(string[] args)
        {
            Console.WriteLine("GPIO FUN!");
            var ledPins = new GPIOPin[] {
                Raspberry.Pins.Pin2,
                Raspberry.Pins.Pin3,
                Raspberry.Pins.Pin4,
                Raspberry.Pins.Pin17,
                Raspberry.Pins.Pin27,
                Raspberry.Pins.Pin22
            };

            var mainTask = Task.Factory.StartNew(() =>
            {
                // reset GPIO to default state
                foreach (var pin in ledPins)
                {
                    Console.WriteLine($"Setting pin {pin.GpioId} mode to output");
                    pin.ChangeMode(GPIOMode.Out);
                    Console.WriteLine($"Writing 0 on pin {pin.GpioId}");
                    pin.Write(false);
                }

                Console.WriteLine($"Setting pin 20 and 21 mode to input");
                Raspberry.Pins.Pin20.ChangeMode(GPIOMode.In);
                Raspberry.Pins.Pin21.ChangeMode(GPIOMode.In);

                Raspberry.Pins.Pin20.WaitForEdge(Edge.Falling, HandleSpeedButtonRelease);
                Raspberry.Pins.Pin21.WaitForEdge(Edge.Falling, HandleModeButtonRelease);

                Mode = LedMode.LTR;
                while (!ShuttingDown)
                {
                    switch (Mode)
                    {
                        case LedMode.LTR:
                            foreach (var pin in ledPins)
                            {
                                pin.Toggle();
                                System.Threading.Thread.Sleep(Speed);
                                pin.Toggle();
                            }
                            break;
                        case LedMode.RTL:
                            for (var i = ledPins.Length - 1; i >= 0; i--)
                            {
                                var pin = ledPins[i];
                                pin.Toggle();
                                System.Threading.Thread.Sleep(Speed);
                                pin.Toggle();
                            }
                            break;
                        case LedMode.SuperCar:
                            foreach (var pin in ledPins)
                            {
                                pin.Toggle();
                                System.Threading.Thread.Sleep(Speed);
                                pin.Toggle();
                            }
                            for (var i = ledPins.Length - 2; i >= 1; i--)
                            {
                                var pin = ledPins[i];
                                pin.Toggle();
                                System.Threading.Thread.Sleep(Speed);
                                pin.Toggle();
                            }
                            break;
                        case LedMode.Syren:
                            var tasks = new List<Task>();
                            foreach (var pin in ledPins)
                                tasks.Add(Task.Factory.StartNew(() => pin.Write(true)));
                            Task.WaitAll(tasks.ToArray());
                            System.Threading.Thread.Sleep(Speed);
                            tasks.Clear();
                            foreach (var pin in ledPins)
                                tasks.Add(Task.Factory.StartNew(() => pin.Write(false)));
                            Task.WaitAll(tasks.ToArray());
                            System.Threading.Thread.Sleep(Speed);
                            break;
                        default:
                            break;
                    }

                }

                // reset GPIO to default state
                foreach (var pin in ledPins)
                {
                    Console.WriteLine($"Writing 0 on pin {pin.GpioId}");
                    pin.Write(false);
                }
            });

            while (!ShuttingDown)
            {
                var keyInfo = Console.ReadKey();
                if (keyInfo.Key == ConsoleKey.X && keyInfo.Modifiers == ConsoleModifiers.Control)
                {
                    ShuttingDown = true;
                    Console.WriteLine("shutting down");
                }
                mainTask.Wait();
            }
        }

        static void HandleSpeedButtonRelease(GPIOPin pin, Edge edge)
        {
            try { Speed = Speeds[++SpeedIndex]; }
            catch
            {
                SpeedIndex = 0;
                Speed = Speeds[SpeedIndex];
            }

            Console.WriteLine($"Speed changed to {Speed}");
            pin.WaitForEdge(edge, HandleSpeedButtonRelease);
        }

        static void HandleModeButtonRelease(GPIOPin pin, Edge edge)
        {
            var newValue = Mode + 1;
            if (Enum.IsDefined(typeof(LedMode), newValue))
            { Mode = newValue; }
            else
            { Mode = LedMode.LTR; }
            Console.WriteLine($"Mode changed to {Mode}");
            pin.WaitForEdge(edge, HandleSpeedButtonRelease);
        }
    }
}
