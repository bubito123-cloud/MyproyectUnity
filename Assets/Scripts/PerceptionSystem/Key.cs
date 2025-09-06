using System;

/// <summary>
/// A unique identifier for a perceived object in the world.
/// </summary>
public class Key : IEquatable<Key>
{
    private static long nextId = 0;
    public long Id { get; private set; }

    public Key()
    {
        Id = nextId++;
    }

    public bool Equals(Key other)
    {
        if (other == null) return false;
        return this.Id == other.Id;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as Key);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
