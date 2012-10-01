using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading;
using System.Xml.Serialization;

namespace PhoneApp.GpsBackground.Util
{
    public class XmlIsoStorageBase<T> where T : new()
    {
        private Mutex _mutex;
        private string _storageDataFile;


        // construtor
        public XmlIsoStorageBase(string storeName)
        {
            _mutex = new Mutex(false, storeName);
            SetStorageDataFileName(storeName);
        }

        // publicos
        public void SetStorageDataFileName(string storeDataFileName)
        {
            _storageDataFile = string.Format("{0}.dat", storeDataFileName);
        }
        public virtual T Read()
        {
            var data = new T();

            _mutex.WaitOne();
            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                using (var stream = new IsolatedStorageFileStream(_storageDataFile, FileMode.OpenOrCreate, FileAccess.Read, store))
                using (var reader = new StreamReader(stream))
                {
                    if (!reader.EndOfStream)
                    {

                        var serializer = new XmlSerializer(typeof(T));
                        data = (T)serializer.Deserialize(reader);
                    }
                }
            }
            finally
            {
                _mutex.ReleaseMutex();
            }

            return data;
        }
        public virtual void Write(T data)
        {
            _mutex.WaitOne();
            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                using (var stream = new IsolatedStorageFileStream(_storageDataFile, FileMode.Create, FileAccess.Write, store))
                {
                    var serializer = new XmlSerializer(typeof(T));
                    serializer.Serialize(stream, data);
                }
            }
            catch (Exception)
            {
                _mutex.ReleaseMutex();
            }
        }


        internal void EnsureDirectory(string directoryPath)
        {
            var isoFile = IsolatedStorageFile.GetUserStoreForApplication();
            if (!isoFile.DirectoryExists(directoryPath))
            {
                isoFile.CreateDirectory(directoryPath);
            }
        }
    }
}
