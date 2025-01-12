using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;

public class GroupOfComThreadConnectionMono : MonoBehaviour
{

    public List<ComThreadPortConnectionMono> comThreadConnectionMonos = new List<ComThreadPortConnectionMono>();


    public void SendDataAsBytes(byte[] bytes)
    {
        foreach (var comThreadConnectionMono in comThreadConnectionMonos)
        {
            if(comThreadConnectionMono != null)
            comThreadConnectionMono.SendDataAsBytes(bytes);
        }
    }
    public void SendDataAsByte(byte data)
    {
        foreach (var comThreadConnectionMono in comThreadConnectionMonos)
        {
            if (comThreadConnectionMono != null)
                comThreadConnectionMono.SendDataAsByte(data);
        }
    }

    public void SendDataAsTextCharArray(string data)
    {
        foreach (var comThreadConnectionMono in comThreadConnectionMonos)
        {
            if (comThreadConnectionMono != null)
                comThreadConnectionMono.SendDataAsTextCharArray(data);
        }
    }
    public void SendDataAsTextWithLineReturn(string text)
    {
        SendDataAsTextCharArray(text + "\n");
    }
    public void SendIntegerAsTextWithLineReturn(int value)
    {
        foreach (var comThreadConnectionMono in comThreadConnectionMonos)
        {
            if (comThreadConnectionMono != null)
                comThreadConnectionMono.SendIntegerAsTextWithLineReturn(value);
        }
    }

    public void SendIntegerLittleEndian(int data)
    {
        foreach (var comThreadConnectionMono in comThreadConnectionMonos)
        {
            if (comThreadConnectionMono != null)
                comThreadConnectionMono.SendIntegerLittleEndian(data);
        }
    }
   


}
