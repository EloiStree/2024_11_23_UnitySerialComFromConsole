using UnityEngine;
using System.Diagnostics;
using System;
using System.Collections.Generic;


namespace Eloi.Port { 

[System.Serializable]
public class ComPortToInfo
{
    public string m_comPort = "COM0";
    public string m_name = "None";
    public string m_manufacturer = "None";
    public string m_deviceID = "None";
    public string m_rawDescription = "None";

        public ComPortToInfo() { }
        public ComPortToInfo(string comPort, string name="", string manufacturer = "", string deviceID = "", string rawDescription = "")
        {
            m_comPort = comPort;
            m_name = name;
            m_manufacturer = manufacturer;
            m_deviceID = deviceID;
            m_rawDescription = rawDescription;
        }

        public string GetComPortId()
        {
            return m_comPort;
        }
        public string GetName() {
            return m_name;
        }
        public string GetManufacturer() {
            return m_manufacturer;
        }
        public string GetDeviceID() {
            return m_deviceID;
        }
        public string GetRawDescription() {
            return m_rawDescription;
        }
        public string GetLineDescription() {
            return $"Port:{m_comPort} Name:{m_name} Manufacturer:{m_manufacturer} DeviceID:{m_deviceID}";
        }

    }

public class ComPortInformationHolderMono : MonoBehaviour
{


    public List<ComPortToInfo> m_comPortInformation = new List<ComPortToInfo>();
    [TextArea(2, 20)]
    public string m_allPortNameInfo;






        public void TryToFindFromExactName(string name,out bool found,  out ComPortToInfo comPortInfo)
    {
        comPortInfo = m_comPortInformation.Find(x => x.m_name.Contains(name));
        found = comPortInfo != null;
    }

    public void TryToFindFromExactComId(string comId, out bool found, out ComPortToInfo comPortInfo)
    {
        comPortInfo = m_comPortInformation.Find(x => x.m_comPort.Contains(comId));
        found = comPortInfo != null;
    }

    public void TryToFindFromExactManufacturer(string manufacturer, out bool found, out ComPortToInfo comPortInfo)
    {
        comPortInfo = m_comPortInformation.Find(x => x.m_manufacturer.Contains(manufacturer));
        found = comPortInfo != null;
    }

    public void TryToFindFromContainsText(string text, bool ignoreCase, out bool found, out ComPortToInfo comPortInfo)
    {

            TryToFindFromExactComId(text, out found, out comPortInfo);
            if (found)
                return;

            TryToFindFromExactName(text, out found, out comPortInfo);
            if (found)
                return;
            TryToFindFromExactManufacturer(text, out found, out comPortInfo);
            if (found)
                return;

            if (ignoreCase)
                comPortInfo = m_comPortInformation.Find(x => x.m_name.ToLower().Contains(text.ToLower()));
            else
                comPortInfo = m_comPortInformation.Find(x => x.m_name.Contains(text));
            found = comPortInfo != null;
            if (found)
                return;



            if (ignoreCase)
                comPortInfo = m_comPortInformation.Find(x => x.m_rawDescription.ToLower().Contains(text.ToLower()));
            else
                comPortInfo = m_comPortInformation.Find(x => x.m_rawDescription.Contains(text));
            found = comPortInfo != null;
    }

    

    private void Awake()
    {
        RefreshComPortNamesDetails();
    }

    [ContextMenu("Refresh Com Port Names Details")]
    public void RefreshComPortNamesDetails()
    {
        m_comPortInformation.Clear();
        string[] strings = System.IO.Ports.SerialPort.GetPortNames();
        string allPortInfo = "";
        foreach (string s in strings)
        {
            allPortInfo +=  $"\n\n---------{s}--------\n";
            GetComDetailsFromPortName(s, out string portInfo, out ComPortToInfo comInfo);
            allPortInfo += portInfo;
            m_comPortInformation.Add(comInfo);
        }
        m_allPortNameInfo = allPortInfo;
    }

    public void GetComDetailsFromPortName(string portName,out string rawDetails, out ComPortToInfo comInfo)
    {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        Process process = new Process();
        process.StartInfo.FileName = "cmd.exe";
        process.StartInfo.Arguments = $"/C wmic path Win32_PnPEntity where \"Name like '%{portName.ToUpper()}%'\" get Name,Manufacturer,DeviceID /format:list";
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        rawDetails = output.Trim();
        rawDetails = rawDetails.Replace("&amp;", "_");
        comInfo = ParseRawText(portName, rawDetails);
#else
        rawDetails = "Only working on Window";
        comInfo = new ComPortToInfo();
#endif
    }


    public static ComPortToInfo ParseRawText(string portName, string rawText)
    {
        ComPortToInfo comPortInfo = new ComPortToInfo();
        comPortInfo.m_comPort = portName;
        comPortInfo.m_rawDescription = rawText;
        string[] lines = rawText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            string[] parts = line.Split(new[] { '=' }, 2);

            if (parts.Length == 2)
            {
                string key = parts[0].Trim();
                string value = parts[1].Trim();

                if (key.ToLower().Equals("DeviceID".ToLower()))
                {
                    comPortInfo.m_deviceID = value; 
                }
                else if (key.ToLower().Equals("Manufacturer".ToLower()))
                {
                    comPortInfo.m_manufacturer = value; 
                }
                else if (key.ToLower().Equals("Name".ToLower()))
                {
                    comPortInfo.m_name = value; 
                }
            }
        }

        return comPortInfo;
    }


        public void TryToFindFromCOM(string port, out bool found, out ComPortToInfo foundInfo)
        {
            port = port.ToUpper();
            foundInfo = m_comPortInformation.Find(x => x.m_comPort.ToUpper().Contains(port));
            found = foundInfo != null;
        }

        public void TryToAppendComInfoIfNotExisting(string portId)
        {
            TryToFindFromCOM(portId, out bool found, out ComPortToInfo foundInfo);
            if (!found)
            {
                GetComDetailsFromPortName(portId, out string portInfo, out ComPortToInfo comInfo);
                m_comPortInformation.Add(comInfo);
            }

        }
    }



}