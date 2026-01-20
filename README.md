# CustomReadWriteLock

A lightweight, custom implementation of a read-write lock in C# that allows multiple concurrent readers or a single exclusive writer.

## Features

- **Multiple Readers**: Allows multiple threads to read concurrently when no writer is active
- **Exclusive Writer**: Ensures only one writer can access the resource at a time, blocking all readers
- **Disposable Lock Scopes**: Provides `ReadLockScope` and `WriteLockScope` structs for safe, exception-safe lock management with `using` statements
- **Thread-Safe**: Uses `Monitor`, `Interlocked`, and `volatile` for proper synchronization

## Usage

### Basic Enter/Exit Pattern

```csharp
var rwLock = new CustomReadWriteLock();

// Reading
rwLock.EnterRead();
try
{
    // Read shared data
}
finally
{
    rwLock.ExitRead();
}

// Writing
rwLock.EnterWrite();
try
{
    // Modify shared data
}
finally
{
    rwLock.ExitWrite();
}
```

### Using Disposable Scopes (Recommended)

```csharp
var rwLock = new CustomReadWriteLock();

// Reading with automatic release
using (rwLock.AcquireReadLock())
{
    // Read shared data
}

// Writing with automatic release
using (rwLock.AcquireWriteLock())
{
    // Modify shared data
}
```

## API Reference

| Method | Description |
|--------|-------------|
| `EnterRead()` | Acquires a read lock. Blocks if a writer holds the lock. |
| `ExitRead()` | Releases a read lock. |
| `EnterWrite()` | Acquires a write lock. Blocks if any readers or another writer holds the lock. |
| `ExitWrite()` | Releases a write lock. |
| `AcquireReadLock()` | Returns a disposable `ReadLockScope` for use with `using` statements. |
| `AcquireWriteLock()` | Returns a disposable `WriteLockScope` for use with `using` statements. |

## Requirements

- .NET 8.0 or later

## License

MIT
