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
        ItemStackSerializationSurrogate itemStackSS = new ItemStackSerializationSurrogate();

        surrogateSelector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), vector3SS);
        surrogateSelector.AddSurrogate(typeof(ItemStack), new StreamingContext(StreamingContextStates.All), itemStackSS);

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
    public object SetObjectData(object obj, SerializationInfo info,
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

public class ItemStackSerializationSurrogate : ISerializationSurrogate
{

    // Method called to serialize a Vector3 object
    public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
    {
        ItemStack stack = (ItemStack)obj;
        info.AddValue("item_id", stack.Item.name);
        info.AddValue("count", stack.Count);
        info.AddValue("data", stack.Attributes, typeof(ItemAttributes));
    }

    // Method called to deserialize a Vector3 object
    public object SetObjectData(object obj, SerializationInfo info,
                                       StreamingContext context, ISurrogateSelector selector)
    {
        string itemId = (string)info.GetValue("item_id", typeof(string));
        Item item = Items.Get(itemId);
        int count = (int)info.GetValue("count", typeof(int));
        ItemAttributes data = (ItemAttributes)info.GetValue("data", typeof(ItemAttributes));

        obj = new ItemStack(item, data, count);
        return obj;
    }

}
