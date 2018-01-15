namespace RaspiCore
{
    public class Raspberry
    {
        public static class Pins
        {
            public static GPIOPin Pin2 { get; } = new GPIOPin(3, 2);
            public static GPIOPin Pin3 { get; } = new GPIOPin(5, 3);
            public static GPIOPin Pin4 { get; } = new GPIOPin(7, 4);

            public static GPIOPin Pin17 { get; } = new GPIOPin(11, 17);
            public static GPIOPin Pin27 { get; } = new GPIOPin(13, 27);
            public static GPIOPin Pin22 { get; } = new GPIOPin(15, 22);

            public static GPIOPin Pin20 { get; } = new GPIOPin(38, 20);
            public static GPIOPin Pin21 { get; } = new GPIOPin(40, 21);
        }
    }
}
