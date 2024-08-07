using System.IO;
using UnityEngine;

namespace Project.DataSaving
{
    public class DataSavingService : MonoBehaviour
    {
        public T LoadData<T>(string name)
        {
            TryLoadData<T>(name, out T data);
            return data;
        }

        public bool TryLoadData<T>(string name, out T data)
        {
            if (string.IsNullOrEmpty(name))
            {
                data = default(T);
                return false;
            }

            var dataPath = GetDataPath(name);
            if (!File.Exists(dataPath))
            {
                data = default(T);
                return false;
            }

            var fileData = File.ReadAllText(dataPath);
            if(string.IsNullOrEmpty(fileData))
            {
                data = default(T);
                return false;
            }

            data = JsonUtility.FromJson<T>(fileData);
            return data != null;
        }

        public void SaveData(string name, object data)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            var dataPath = GetDataPath(name);
            var fileData = JsonUtility.ToJson(data);
            File.WriteAllText(dataPath, fileData);
        }

        private string GetDataPath(string filePath) 
        {
            return Path.Combine(Application.persistentDataPath, filePath);
        }
    }
}
