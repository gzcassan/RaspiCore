using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RaspiCore
{
    public class GPIOPin : Pin
    {
        List<Process> processes = new List<Process>();

        public Byte GpioId { get; protected set; }

        private Object _lock = new Object();

        public GPIOPin(Byte physicalId, Byte gpioId)
        {
            PhysicalId = physicalId;
            GpioId = gpioId;
        }

        public void Toggle()
        {
            lock (_lock)
            {
                var p = Process.Start("gpio", $"-g toggle {GpioId}");
                p.WaitForExit();
            }
        }

        public void Write(Boolean level)
        {
            var value = level ? "1" : "0";
            lock (_lock)
            {
                var p = Process.Start("gpio", $"-g write {GpioId} {value}");
                p.WaitForExit();
            }
        }

        public void ChangeMode(GPIOMode mode)
        {
            var value = mode == GPIOMode.In
                ? "input"
                : mode == GPIOMode.Out
                    ? "output"
                    : "pwm";

            lock (_lock)
            {
                var p = Process.Start("gpio", $"-g mode {GpioId} {value}");
                p.WaitForExit();
            }
        }

        public void WaitForEdge(Edge edge, Action<GPIOPin, Edge> callback)
        {
            Task.Factory.StartNew(() =>
            {
                lock (_lock)
                {
                    var p = Process.Start("gpio", $"-g wfi {GpioId} {edge}");
                    processes.Add(p);
                    p.WaitForExit();
                }
                callback(this, edge);
            });
        }

        public override string ToString()
            => $"{Name}, Id: {PhysicalId}, GPIOID: {GpioId}";
    }
}
