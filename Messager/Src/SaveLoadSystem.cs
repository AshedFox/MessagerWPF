using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Messager
{
    public static class SaveLoadSystem
    {
        public static readonly string savePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Messages", "Messages.bin");

        public static Dictionary<long, MessageCollection> Load(long userId)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();

            if (File.Exists(savePath))
            {
                using (FileStream fileStream = new FileStream(savePath, FileMode.Open))
                {
                    try
                    {
                        var keyValuePairs =
                            (Dictionary<long, Dictionary<long, MessageCollection>>)binaryFormatter.Deserialize(fileStream);
                        return keyValuePairs[userId];
                    }
                    catch 
                    { 
                        return new Dictionary<long, MessageCollection>(); 
                    }
                }
            }
            else return new Dictionary<long, MessageCollection>();
        }

        public static void Save(long userId, Dictionary<long, MessageCollection> data)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            Dictionary<long, Dictionary<long, MessageCollection>> existingData = 
                new Dictionary<long, Dictionary<long, MessageCollection>>();
            if (File.Exists(savePath))
            {
                using (FileStream fileStream = new FileStream(savePath, FileMode.Open))
                {
                    existingData = (Dictionary<long, Dictionary<long, MessageCollection>>)binaryFormatter.Deserialize(fileStream);
                    
                    if (existingData.ContainsKey(userId))
                    {
                        existingData[userId] = data;
                    }
                    else
                    {
                        existingData.Add(userId, data);
                    }
                }
            }
            else
            {
                existingData.Add(userId, data);
            }

            using (FileStream fileStream = new FileStream(savePath, FileMode.Create))
            {
                binaryFormatter.Serialize(fileStream, existingData);
            }
        }
    }
}
