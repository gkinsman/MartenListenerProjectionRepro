using System;
using Marten.Events.Aggregation;
using Marten.Events.Projections;

namespace MartenProjectionListenerRepro
{
    public class TestEntity
    {
        public Guid Id { get; set; }
        public int Counter { get; set; }
    }

    public class TestEvent
    { }

    public class TestProjection : AggregateProjection<TestEntity>
    {
        public void Apply(TestEvent @event, TestEntity entity)
        {
            entity.Counter += 1;
        }
    }
}