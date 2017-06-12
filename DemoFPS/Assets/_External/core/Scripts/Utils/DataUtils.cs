using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class DataUtils : MonoBehaviour {
    public static bool saveData(string dataPath, object data)
    {
        try
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(dataPath);
            bf.Serialize(file, data);
            file.Close();
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
            return false;
        }
        return true;
    }
    public static bool loadData<T>(string dataPath, ref T data)
    {
        if (File.Exists(dataPath))
        {
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(dataPath, FileMode.Open);
                data = (T)bf.Deserialize(file);
                file.Close();
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                return false;
            }
            
        }
        return true;
    }
}
