﻿
using System;
using System.Linq;
using Metrics.Utils;
namespace Metrics.Sampling
{
    public sealed class UniformReservoir : Reservoir
    {
        private const int DefaultSize = 1028;
        private const int BitsPerLong = 63;

        private AtomicLong count = new AtomicLong();

        private readonly UserValueWrapper[] values;

        public UniformReservoir()
            : this(DefaultSize)
        { }

        public UniformReservoir(int size)
        {
            this.values = new UserValueWrapper[size];
        }

        public int Size
        {
            get
            {
                return Math.Min((int)this.count.Value, this.values.Length);
            }
        }

        public Snapshot Snapshot
        {
            get
            {
                var size = this.Size;
                if (size == 0)
                {
                    return new UniformSnapshot(Enumerable.Empty<long>());
                }

                UserValueWrapper[] values = new UserValueWrapper[size];
                Array.Copy(this.values, values, size);
                Array.Sort(values, UserValueWrapper.Comparer);
                var minValue = values[0].UserValue;
                var maxValue = values[size - 1].UserValue;
                return new UniformSnapshot(values.Select(v => v.Value), valuesAreSorted: true, minUserValue: minValue, maxUserValue: maxValue);
            }
        }

        public void Update(long value, string userValue = null)
        {
            long c = this.count.Increment();
            if (c <= this.values.Length)
            {
                values[(int)c - 1] = new UserValueWrapper(value, userValue);
            }
            else
            {
                long r = NextLong(c);
                if (r < values.Length)
                {
                    values[(int)r] = new UserValueWrapper(value, userValue);
                }
            }
        }

        public void Reset()
        {
            count.SetValue(0L);
        }

        private static long NextLong(long max)
        {
            long bits, val;
            do
            {
                bits = ThreadLocalRandom.NextLong() & (~(1L << BitsPerLong));
                val = bits % max;
            } while (bits - val + (max - 1) < 0L);
            return val;
        }
    }
}
