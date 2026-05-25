using System;
using System.Collections.Generic;
using System.Threading;
using OpenIPC.Viewer.Core.Entities;

namespace OpenIPC.Viewer.Core.Events;

// Dev/test source: motion ticks are injected manually via Trigger(cameraId).
// Lets the Events page demonstrate the full ingestion path before real per-
// camera sources land. Singleton in DI so anyone can grab it and Trigger.
public sealed class ManualMotionEventSource : IMotionEventSource
{
    public string Name => "manual";

    private readonly object _gate = new();
    private readonly Dictionary<CameraId, List<IObserver<MotionTick>>> _observers = new();

    public IDisposable Watch(CameraId cameraId, IObserver<MotionTick> observer, CancellationToken ct)
    {
        lock (_gate)
        {
            if (!_observers.TryGetValue(cameraId, out var list))
                _observers[cameraId] = list = new List<IObserver<MotionTick>>();
            list.Add(observer);
        }
        return new Subscription(this, cameraId, observer);
    }

    public void Trigger(CameraId cameraId)
    {
        IObserver<MotionTick>[] snapshot;
        lock (_gate)
        {
            if (!_observers.TryGetValue(cameraId, out var list)) return;
            snapshot = list.ToArray();
        }
        var tick = new MotionTick(cameraId, DateTime.UtcNow, Name);
        foreach (var o in snapshot)
        {
            try { o.OnNext(tick); } catch { /* swallow */ }
        }
    }

    private sealed class Subscription : IDisposable
    {
        private readonly ManualMotionEventSource _owner;
        private readonly CameraId _cameraId;
        private readonly IObserver<MotionTick> _observer;
        public Subscription(ManualMotionEventSource owner, CameraId id, IObserver<MotionTick> obs)
        {
            _owner = owner; _cameraId = id; _observer = obs;
        }
        public void Dispose()
        {
            lock (_owner._gate)
            {
                if (_owner._observers.TryGetValue(_cameraId, out var list))
                    list.Remove(_observer);
            }
        }
    }
}
