﻿using System.Xml;
using System.IO;
using UnityEngine;
using System.Reflection;
using System;
using UnityEngine.Networking;

namespace TDataPersistent
{
    public class CDataSave<T> where T:class,new()
    {
        public static readonly FieldInfo[] s_FieldInfos = typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        public static readonly string s_FilePath =   typeof(T).Name ;
        public virtual bool DataCrypt() => true;
    }
    public static class TDataSave
    {
        private const string c_DataCryptKey = "StriteRTestCrypt";
        private static readonly string s_persistentPath = Application.persistentDataPath + "/Save/";
        private static readonly string s_streamingPath = Application.dataPath + "/Resources/Save/";
        private static XmlDocument m_Doc = new XmlDocument();
        private static string GetStreamingPathInDevice<T>(string fileName = null) where T : CDataSave<T>, new() => "Save/" +(fileName == null ? CDataSave<T>.s_FilePath : fileName);
        private static string GetStreamingPath<T>(string fileName = null) where T : CDataSave<T>, new() => s_streamingPath + (fileName == null ? CDataSave<T>.s_FilePath : fileName)+ ".bytes";
        public static string GetPersistentPath<T>(string fileName = null) where T : CDataSave<T>, new() => s_persistentPath +(fileName == null ? CDataSave<T>.s_FilePath : fileName) + ".sav";
        public static void ReadPersistentData<T>(this T _data, bool isDefault = false, string fileName = null) where T : CDataSave<T>,new ()
        {
            if (isDefault)
            {
                ReadDefaultData(_data, isDefault,fileName);
                return;
            }

            try
            {
                Validate<T>(File.ReadAllText(GetPersistentPath<T>(fileName)),out var parentNode);
                ReadData(_data,parentNode);
            }
            catch (Exception ePersistent)
            {
                Debug.LogWarning("Data Read Fail,Use Streaming Data:\n" + ePersistent.Message);
                ReadDefaultData(_data,isDefault,fileName);
            }
        }       

        public static void SavePersistentData<T>(this CDataSave<T> _data, bool isDefault = false, string fileName=null) where T: CDataSave<T>,new ()
        {
            string persistentPath = isDefault? GetStreamingPath<T>(fileName) : GetPersistentPath<T>(fileName);
            try
            {
                Validate<T>(File.ReadAllText( persistentPath),out var parentNode);
                SaveData(_data,parentNode,persistentPath);
            }
            catch(Exception e)
            {
                Debug.LogWarning("Data Save Error,Use Streaming Data\n" + e.Message);
                ReadDefaultData(_data,isDefault,fileName);
            }
        }

        static void Validate<T>(string _xmlText,out XmlNode _node) where T : CDataSave<T>, new()
        {
            m_Doc.LoadXml(_xmlText);
            _node = m_Doc.SelectSingleNode(typeof(T).Name);
            if (_node == null)
                throw new Exception("None Xml Parent Found:" + typeof(T).Name);

            foreach(var fieldInfo in CDataSave<T>.s_FieldInfos)
                if (_node.SelectSingleNode(fieldInfo.Name) == null)
                    throw new Exception("Invalid Xml Child:" + fieldInfo.Name);
        }

        static void ReadData<T>(CDataSave<T> _data,XmlNode _node) where T:CDataSave<T>,new()
        {
            bool dataCrypt = _data.DataCrypt();
            FieldInfo[] fieldInfo = CDataSave<T>.s_FieldInfos;
            for (int i = 0; i < fieldInfo.Length; i++)
            {
                string readData = _node.SelectSingleNode(fieldInfo[i].Name).InnerText;
                if (dataCrypt) readData = TDataCrypt.EasyCryptData(readData, c_DataCryptKey);
                fieldInfo[i].SetValue(_data, TDataConvert.Convert(fieldInfo[i].FieldType, readData));
            }
        }
        
        static void SaveData<T>(CDataSave<T> _data, XmlNode _node,string _path) where T : CDataSave<T>, new()
        {
            bool dataCrypt = _data.DataCrypt();
            FieldInfo[] fieldInfos = CDataSave<T>.s_FieldInfos;
            foreach (var t in fieldInfos)
            {
                var fieldNode = _node.SelectSingleNode(t.Name);
                string saveData = TDataConvert.Convert(t.FieldType, t.GetValue(_data));
                if (dataCrypt) saveData = TDataCrypt.EasyCryptData(saveData, c_DataCryptKey);
                fieldNode.InnerText = saveData;
                _node.AppendChild(fieldNode);
            }
            m_Doc.Save(_path);
        }

        static void ReadDefaultData<T>(CDataSave<T> _data, bool isDefalut = false, string fileName = null) where T : CDataSave<T>, new()
        {
            string filePath = isDefalut? GetStreamingPath<T>(fileName) : GetPersistentPath<T>(fileName);
            try
            {
#if UNITY_EDITOR
                Validate<T>(File.ReadAllText(GetStreamingPath<T>(fileName)),out XmlNode node);
#else
                Validate<T>(Resources.Load<TextAsset>(GetStreamingPathInDevice<T>(fileName)).text,out XmlNode node);
#endif


                ReadData(_data,node);
                SaveData(_data,node,filePath);
            }
            catch(Exception eStreaming)
            {
                Debug.LogError("Streaming Data Invalid,Use BuiltIn-Code:\n"+eStreaming.Message);

                if (!Directory.Exists(s_persistentPath))
                    Directory.CreateDirectory(s_persistentPath);
        
                if (File.Exists(filePath))
                    File.Delete(filePath);

                m_Doc = new XmlDocument();
                var node = m_Doc.AppendChild(m_Doc.CreateElement(typeof(T).Name));
                foreach(var fieldInfo in CDataSave<T>.s_FieldInfos)
                    node.AppendChild(m_Doc.CreateElement(fieldInfo.Name));
            
                SaveData(new T(),node,filePath);
            }
        }
    }

}