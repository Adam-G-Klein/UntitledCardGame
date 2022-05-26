using System.Collections;
using System.Collections.Generic;
 using System.Xml;
 using System.Xml.Serialization;
 using System.IO;

[XmlRoot("Map"), XmlType("Map")]
public class DeserializedMap 
{
    [XmlArray("Rooms")]
    [XmlArrayItem("Room")]
    public List<DefaultRoom> Rooms;

}
