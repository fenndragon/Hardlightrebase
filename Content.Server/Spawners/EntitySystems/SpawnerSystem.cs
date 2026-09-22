using Content.Server.Spawners.Components;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.Spawners.EntitySystems;

public sealed class SpawnerSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TimedSpawnerComponent, ComponentShutdown>(OnTimedSpawnerShutdown);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<TimedSpawnerComponent>();
        while (query.MoveNext(out var uid, out var timedSpawner))
        {
            if (timedSpawner.NextFire > curTime)
                continue;
            OnTimerFired(uid, timedSpawner);
            timedSpawner.NextFire += timedSpawner.IntervalSeconds;
        }
    }

    private void OnTimerFired(EntityUid uid, TimedSpawnerComponent component)
    {
        if (!_random.Prob(component.Chance))
            return;

        var number = _random.Next(component.MinimumEntitiesSpawned, component.MaximumEntitiesSpawned);
        var coordinates = Transform(uid).Coordinates;

        for (var i = 0; i < number; i++)
        {
            var entity = _random.Pick(component.Prototypes);
            SpawnAtPosition(entity, coordinates);
        }
    }

    private void OnTimedSpawnerShutdown(EntityUid uid, TimedSpawnerComponent component, ComponentShutdown args)
    {
        component.TokenSource?.Cancel();
    }
}
