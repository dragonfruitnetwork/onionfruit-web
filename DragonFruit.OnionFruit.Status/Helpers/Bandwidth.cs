// OnionFruit.Status Copyright 2021 DragonFruit Network <inbox@dragonfruit.network>
// Licensed under MIT. Please refer to the LICENSE file for more info

using System;

namespace DragonFruit.OnionFruit.Status.Converters
{
    public class Bandwidth
    {
        public Bandwidth(long bandwidth)
        {
            Bytes = Convert.ToDecimal(bandwidth);
        }

        public decimal Bytes { get; }

        public decimal Kilobytes => Bytes / 1000;
        public decimal Megabytes => Bytes / 1000000;
        public decimal Gigabytes => Bytes / 1000000000;

        public decimal Kilobits => Bytes / 125;
        public decimal Megabits => Bytes / 125000;
        public decimal Gigabits => Bytes / 125000000;
    }
}
