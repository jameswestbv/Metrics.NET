﻿using FluentAssertions;
using Metrics.Sampling;
using Xunit;

namespace Metrics.Tests.Sampling
{
    public class UniformReservoirTests
    {
        [Fact]
        public void UniformReservoir_Of100OutOf1000Elements()
        {
            UniformReservoir reservoir = new UniformReservoir(100);

            for (int i = 0; i < 1000; i++)
            {
                reservoir.Update(i);
            }

            reservoir.Size.Should().Be(100);
            reservoir.Snapshot.Size.Should().Be(100);
            reservoir.Snapshot.Values.Should().OnlyContain(v => 0 <= v && v < 1000);
        }

        [Fact]
        public void UniformReservoir_RecordsUserValue()
        {
            UniformReservoir reservoir = new UniformReservoir(100);

            reservoir.Update(2L, "B");
            reservoir.Update(1L, "A");

            reservoir.Snapshot.MinUserValue.Should().Be("A");
            reservoir.Snapshot.MaxUserValue.Should().Be("B");
        }
    }
}
