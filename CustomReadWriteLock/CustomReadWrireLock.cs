using System;
using System.Threading;
using System.Threading.Tasks;

namespace CustomReadWriteLock
{
    class Program
    {
        static readonly CustomReadWriteLock _lock = new CustomReadWriteLock();
        static int _sharedValue = 0;

        static void Main(string[] args)
        {
            Console.WriteLine("CustomReadWriteLock Demo\n");

            // Start multiple reader and writer tasks
            var tasks = new Task[6];

            tasks[0] = Task.Run(() => WriterTask(1));
            tasks[1] = Task.Run(() => ReaderTask(1));
            tasks[2] = Task.Run(() => ReaderTask(2));
            tasks[3] = Task.Run(() => WriterTask(2));
            tasks[4] = Task.Run(() => ReaderTask(3));
            tasks[5] = Task.Run(() => ReaderTask(4));

            Task.WaitAll(tasks);

            Console.WriteLine($"\nFinal shared value: {_sharedValue}");
            Console.WriteLine("Demo completed successfully!");
        }

        static void ReaderTask(int id)
        {
            for (int i = 0; i < 3; i++)
            {
                using (_lock.AcquireReadLock())
                {
                    Console.WriteLine($"Reader {id}: Read value = {_sharedValue}");
                    Thread.Sleep(50);
                }
                Thread.Sleep(10);
            }
        }

        static void WriterTask(int id)
        {
            for (int i = 0; i < 2; i++)
            {
                using (_lock.AcquireWriteLock())
                {
                    _sharedValue++;
                    Console.WriteLine($"Writer {id}: Wrote value = {_sharedValue}");
                    Thread.Sleep(100);
                }
                Thread.Sleep(20);
            }
        }
    }

    public class CustomReadWriteLock
    {
        private volatile int _readerCount;
        private volatile int _writerCount;
        private readonly object _readerLock = new object();
        private readonly object _writerLock = new object();


        public void EnterRead()
        {
            lock (_readerLock)
            {
                while (_writerCount > 0)
                {
                    Monitor.Wait(_readerLock);
                }

                Interlocked.Increment(ref _readerCount);
            }
        }

        public void ExitRead()
        {
            int remainingReaders = Interlocked.Decrement(ref _readerCount);

            lock (_readerLock)
            {
                Monitor.PulseAll(_readerLock);
            }

            if (remainingReaders == 0)
            {
                lock (_writerLock)
                {
                    Monitor.PulseAll(_writerLock);
                }
            }
        }

        public void EnterWrite()
        {
            lock (_writerLock)
            {
                while (_writerCount > 0)
                {
                    Monitor.Wait(_writerLock);
                }

                Interlocked.Increment(ref _writerCount);

                lock (_readerLock)
                {
                    Monitor.PulseAll(_readerLock);

                    while (_readerCount > 0)
                    {
                        Monitor.Wait(_readerLock);
                    }
                }
            }
        }

        public void ExitWrite()
        {
            Interlocked.Decrement(ref _writerCount);

            lock (_writerLock)
            {
                Monitor.PulseAll(_writerLock);
            }

            lock (_readerLock)
            {
                Monitor.PulseAll(_readerLock);
            }
        }

        public ReadLockScope AcquireReadLock()
        {
            EnterRead();
            return new ReadLockScope(this);
        }

        public WriteLockScope AcquireWriteLock()
        {
            EnterWrite();
            return new WriteLockScope(this);
        }


        public readonly struct ReadLockScope : IDisposable
        {
            private readonly CustomReadWriteLock _lock;

            internal ReadLockScope(CustomReadWriteLock lockInstance) => _lock = lockInstance;

            public void Dispose() => _lock.ExitRead();
        }

        public readonly struct WriteLockScope : IDisposable
        {
            private readonly CustomReadWriteLock _lock;

            internal WriteLockScope(CustomReadWriteLock lockInstance) => _lock = lockInstance;

            public void Dispose() => _lock.ExitWrite();
        }
    }
}
