using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

public static class BinarySaveDataSerializer
{

    private static readonly BinaryFormatter _binaryFormatter;

    static BinarySaveDataSerializer()
    {
        _binaryFormatter = new BinaryFormatter();

        SurrogateSelector surrogateSelector = new SurrogateSelector();
        Vector3SerializationSurrogate vector3SS = new Vector3SerializationSurrogate();

        surrogateSelector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), vector3SS);
        _binaryFormatter.SurrogateSelector = surrogateSelector;
    }

    public static SaveData Deserialize(string filePath)
    {
        using (var stream = File.Open(filePath, FileMode.Open))
        {
            return (SaveData)_binaryFormatter.Deserialize(stream);
        }
    }

    public static void Serialize(string filePath, SaveData data)
    {
        using (var stream = File.Open(filePath, FileMode.Create))
        {
            _binaryFormatter.Serialize(stream, data);
        }
    }

    public static bool DoesFileExist(string filePath)
    {
        return File.Exists(filePath);
    }

}

public class Vector3SerializationSurrogate : ISerializationSurrogate
{

    // Method called to serialize a Vector3 object
    public void GetObjectData(System.Object obj, SerializationInfo info, StreamingContext context)
    {
        Vector3 v3 = (Vector3)obj;
        info.AddValue("x", v3.x);
        info.AddValue("y", v3.y);
        info.AddValue("z", v3.z);
    }

    // Method called to deserialize a Vector3 object
    public System.Object SetObjectData(System.Object obj, SerializationInfo info,
                                       StreamingContext context, ISurrogateSelector selector)
    {
        Vector3 v3 = (Vector3)obj;
        v3.x = (float)info.GetValue("x", typeof(float));
        v3.y = (float)info.GetValue("y", typeof(float));
        v3.z = (float)info.GetValue("z", typeof(float));
        obj = v3;
        return obj;
    }

}
