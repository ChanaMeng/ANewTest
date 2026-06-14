using System;
using System.Collections.Generic;

namespace SDClub.Core
{
    public class TimerComponent : Entity, IAwake, IUpdate
    {
        private long frameCounter;
        private readonly Dictionary<long, ATimer> timers = new();
        private readonly Queue<long> toRemove = new();
        private long idGenerator;
        private readonly Dictionary<Type, Queue<ATimer>> timerPool = new();

        public long NewOnceTimer(long interval, Action callback)
        {
            return NewTimer(interval, false, callback);
        }

        public long NewOnceTimer<T>(long interval, Action<T> callback, T arg)
        {
            return NewTimer(interval, false, callback, arg);
        }

        public long NewOnceTimer<T1, T2>(long interval, Action<T1, T2> callback, T1 arg1, T2 arg2)
        {
            return NewTimer(interval, false, callback, arg1, arg2);
        }

        public long NewRepeatedTimer(long interval, Action callback)
        {
            return NewTimer(interval, true, callback);
        }

        public long NewRepeatedTimer<T>(long interval, Action<T> callback, T arg)
        {
            return NewTimer(interval, true, callback, arg);
        }

        public long NewRepeatedTimer<T1, T2>(long interval, Action<T1, T2> callback, T1 arg1, T2 arg2)
        {
            return NewTimer(interval, true, callback, arg1, arg2);
        }

        public void RemoveTimer(ref long id)
        {
            if (id == 0)
            {
                return;
            }

            if (this.timers.TryGetValue(id, out ATimer timer))
            {
                this.toRemove.Enqueue(id);
            }

            id = 0;
        }

        public void RemoveTimer(long id)
        {
            if (id == 0)
            {
                return;
            }

            if (this.timers.TryGetValue(id, out ATimer timer))
            {
                this.toRemove.Enqueue(id);
            }
        }

        public void Tick()
        {
            this.frameCounter++;

            CleanupTimers();

            foreach (var kv in this.timers)
            {
                ATimer timer = kv.Value;
                if (timer.IsDisposed)
                {
                    this.toRemove.Enqueue(timer.Id);
                    continue;
                }

                if (this.frameCounter - timer.StartFrame >= timer.Interval)
                {
                    try
                    {
                        timer.Handle();
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }

                    if (timer.IsRepeated)
                    {
                        timer.StartFrame = this.frameCounter;
                    }
                    else
                    {
                        timer.Dispose();
                        this.toRemove.Enqueue(timer.Id);
                    }
                }
            }

            CleanupTimers();
        }

        public override void Dispose()
        {
            foreach (var kv in this.timers)
            {
                kv.Value.Dispose();
            }

            this.timers.Clear();
            this.toRemove.Clear();
            this.timerPool.Clear();
            base.Dispose();
        }

        private long NewTimer(long interval, bool isRepeated, Action callback)
        {
            ActionTimer timer = FetchTimer<ActionTimer>();
            timer.SetAction(callback);
            return InitTimer(timer, interval, isRepeated);
        }

        private long NewTimer<T>(long interval, bool isRepeated, Action<T> callback, T arg)
        {
            ActionTimer<T> timer = FetchTimer<ActionTimer<T>>();
            timer.SetAction(callback, arg);
            return InitTimer(timer, interval, isRepeated);
        }

        private long NewTimer<T1, T2>(long interval, bool isRepeated, Action<T1, T2> callback, T1 arg1, T2 arg2)
        {
            ActionTimer<T1, T2> timer = FetchTimer<ActionTimer<T1, T2>>();
            timer.SetAction(callback, arg1, arg2);
            return InitTimer(timer, interval, isRepeated);
        }

        private T FetchTimer<T>() where T : ATimer, new()
        {
            Type type = typeof(T);
            if (this.timerPool.TryGetValue(type, out Queue<ATimer> pool) && pool.Count > 0)
            {
                return (T)pool.Dequeue();
            }

            return new T();
        }

        private long InitTimer(ATimer timer, long interval, bool isRepeated)
        {
            long id = ++this.idGenerator;
            timer.Id = id;
            timer.StartFrame = this.frameCounter;
            timer.Interval = interval;
            timer.IsRepeated = isRepeated;
            this.timers[id] = timer;
            return id;
        }

        private void CleanupTimers()
        {
            while (this.toRemove.Count > 0)
            {
                long id = this.toRemove.Dequeue();
                if (this.timers.Remove(id, out ATimer timer))
                {
                    Type type = timer.GetType();
                    if (!this.timerPool.TryGetValue(type, out Queue<ATimer> pool))
                    {
                        pool = new Queue<ATimer>();
                        this.timerPool[type] = pool;
                    }
                    pool.Enqueue(timer);
                }
            }
        }

        private class ActionTimer : ATimer
        {
            private Action action;

            public void SetAction(Action action)
            {
                this.action = action;
            }

            public override void Handle()
            {
                this.action?.Invoke();
            }

            public override void Dispose()
            {
                this.action = null;
                base.Dispose();
            }
        }

        private class ActionTimer<T> : ATimer
        {
            private Action<T> action;
            private T arg;

            public void SetAction(Action<T> action, T arg)
            {
                this.action = action;
                this.arg = arg;
            }

            public override void Handle()
            {
                this.action?.Invoke(this.arg);
            }

            public override void Dispose()
            {
                this.action = null;
                this.arg = default;
                base.Dispose();
            }
        }

        private class ActionTimer<T1, T2> : ATimer
        {
            private Action<T1, T2> action;
            private T1 arg1;
            private T2 arg2;

            public void SetAction(Action<T1, T2> action, T1 arg1, T2 arg2)
            {
                this.action = action;
                this.arg1 = arg1;
                this.arg2 = arg2;
            }

            public override void Handle()
            {
                this.action?.Invoke(this.arg1, this.arg2);
            }

            public override void Dispose()
            {
                this.action = null;
                this.arg1 = default;
                this.arg2 = default;
                base.Dispose();
            }
        }
    }

    [EntitySystem]
    public class TimerUpdateSystem : UpdateSystem<TimerComponent>
    {
        protected override void Update(TimerComponent self)
        {
            self.Tick();
        }
    }
}
