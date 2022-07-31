using System.Collections;
using System;
using System.IO;
using System.Runtime;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEditor;
using System.Xml.Serialization;
using System.Xml;

public class XmlParser<TMapObject> 
{
    /*
    public static Dictionary<string, MapObject> getMapObjectsById(string pathToDir, string xmlPathToTags){
        //Can't call these generic items "elements" because Xml calls it's lowest level entities 
        // Elements and it would be hella confusing
    }
    */
    public static Dictionary<string, TMapObject> getMapObjectsFromDirectory(string pathToDir, string xmlPathToTags){
        Dictionary<string, TMapObject> mapObjectsById = new Dictionary<string, TMapObject>();
        // get the paths (including the filename) of all the xml files in the directory
        // function excludes .meta files
        string[] filepaths = getXmlFilepaths(pathToDir);
        foreach(string filepath in filepaths){
            // get the map objects from the xml file
            XmlDocument xmlDoc = getXmlDocFromFilepath(filepath);
            Dictionary<string, TMapObject> mapObjectsFromFile = getMapObjectsFromXmlDoc(xmlDoc, xmlPathToTags);
            // add the map objects to the dictionary
            foreach(KeyValuePair<string, TMapObject> kvp in mapObjectsFromFile){
                if(mapObjectsById.ContainsKey(kvp.Key)){
                    Debug.LogError("Duplicate id found in xml file: " + kvp.Key);
                }
                mapObjectsById.Add(kvp.Key, kvp.Value);
            }
        }
        return mapObjectsById;
    }

    private static string[] getXmlFilepaths(string pathToDir){
        string[] filepaths = Directory.GetFiles(pathToDir);
        List<string> xmlFilepaths = new List<string>();
        foreach(string fpath in filepaths){
            if(fpath.EndsWith(".xml") && !fpath.EndsWith(".meta")){
                xmlFilepaths.Add(fpath);
            }
        }
        return xmlFilepaths.ToArray();
    }
    public static XmlDocument getXmlDocFromFilepath(string filepath){
        Debug.Log("Loading xml file: " + filepath);
        XmlDocument xmlDoc = new XmlDocument();
        StreamReader reader = new StreamReader(filepath); 
        xmlDoc.LoadXml(reader.ReadToEnd());
        Debug.Log("Loaded xml file: " + filepath);
        return xmlDoc;
    }

    public static Dictionary<string, TMapObject> getMapObjectsFromXmlDoc(XmlDocument xmlDoc, string xmlPathToTags){
        string id;
        TMapObject mo;
        Dictionary<string, TMapObject> newMosById= new Dictionary<string, TMapObject>();
        foreach(XmlElement node in xmlDoc.SelectNodes(xmlPathToTags))
        {
            id = node.GetAttribute("id");
            mo = getMapObjectWithExceptions(node);
            // okay, if we at SOME POINT decide to have a non-nullable
            // MapObject, this check is going to fail
            if(mo is not null)
                newMosById.Add(id, mo);
        }
        //var dictionaryIMyObject = newMosById.Select(d => new KeyValuePair<String, IMyObject>(d.Key, d.Value)).ToDictionary(k => k.Key, k => k.Value);
        return newMosById; 
    }

    public static TMapObject getMapObjectWithExceptions(XmlElement moElement){
        TMapObject newMo = default(TMapObject);
        string id = moElement.GetAttribute("id");
        newMo = getMapObjectFromElement(moElement);
        return newMo;
    }

    private static TMapObject getMapObjectFromElement(XmlElement moElement){
        TMapObject newMo;
        String moTypeStr = moElement.GetAttribute("type");
        //default return type of GetAttribute is empty string
        if(moTypeStr.Equals("")) {
            Debug.Log("No type found for MapObject " + moElement.GetAttribute("id") + ", assuming it's declared in another file");
            return default(TMapObject);
        }
        try {
            Type moType= Type.GetType(moElement.GetAttribute("type"));
            newMo = (TMapObject) System.Activator.CreateInstance(moType);
        } catch {
            Debug.LogError("Couldn't convert Map Object type " 
                + moElement.GetAttribute("type") 
                + " to a valid MapObject implementation during map generation. "
                + "Example: Use one of the classnames in Assets/Rooms/RoomImplementations for the type field of a room. ");
            newMo= default(TMapObject);
            Manager.Quit();
        }
        return newMo;
    }




   
}