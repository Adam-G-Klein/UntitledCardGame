using System;

// Interface for game state components that can be serialized and deserialized
public interface ISerializableGameState<T> where T : class
{
    // Returns the serializable data object
    T GetSerializableData();

} 