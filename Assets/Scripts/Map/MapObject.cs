using System.Collections;
using System.Collections.Generic;
 using System.Xml;
 using System.Xml.Serialization;
 using System.IO;

public interface MapObject 
{
    // also needs to call roomManager.loadScene on startRoom
    string getId(); 
    void setId(string id); 

}
