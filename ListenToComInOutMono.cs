using Eloi.Port;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class ListenToComInOutMono : MonoBehaviour
{

    public ComPortInformationHolderMono m_informationHolder;
    public ComPortToInfo m_lastChangedPort;
    public string m_currentComPort = "";
    public string[] m_currentPortNamesList = new string[0];
    public string[] m_previousPortNamesList = new string[0];
    public string m_comPortNew = "";
    public string[] m_newPortNamesList = new string[0];
    public string m_comPortRemoved = "";
    public string[] m_removedPortNamesList = new string[0];

    public UnityEvent<string> m_onNewDevice;
    public UnityEvent<string> m_onRemovedDevice;
    public UnityEvent<ComPortToInfo> m_onNewPortInfo;
    public UnityEvent<ComPortToInfo> m_onRemovedPortInfo;

    public float m_updateRate = 3.0f;

    public void OnEnable()
    {
        StartCoroutine(CheckPortThenAndThen());
    }

    private IEnumerator CheckPortThenAndThen()
    {
        ResetToZero();
        while (true)
        {
            yield return new WaitForSeconds(m_updateRate);
            yield return new WaitForEndOfFrame();
            UpdatePortsName();
        }
    }

    private void ResetToZero()
    {
        m_currentPortNamesList = new string[0];
        m_previousPortNamesList = new string[0];
        m_newPortNamesList = new string[0];
        m_removedPortNamesList = new string[0];
        m_comPortNew = "";
        m_comPortRemoved = "";
        m_currentComPort = "";
    }

    [ContextMenu("Update Port Names")]
    private void UpdatePortsName()
    {
        m_previousPortNamesList = m_currentPortNamesList;
        m_currentPortNamesList = SerialPort.GetPortNames();
        m_newPortNamesList = m_currentPortNamesList.Except(m_previousPortNamesList).ToArray();
        m_removedPortNamesList = m_previousPortNamesList.Except(m_currentPortNamesList).ToArray();

        m_currentComPort = string.Join(",", m_currentPortNamesList);
        m_comPortNew= string.Join(",", m_newPortNamesList);
        m_comPortRemoved= string.Join(",", m_removedPortNamesList);


        foreach (var port in m_removedPortNamesList)
        {
            m_onRemovedDevice.Invoke(port);
            if (m_informationHolder != null)
            {
                m_informationHolder.TryToFindFromCOM(port, out bool found, out ComPortToInfo foundInfo);
                if (!found)
                {
                    m_informationHolder.TryToAppendComInfoIfNotExisting(port);
                    m_informationHolder.TryToFindFromCOM(port, out found, out foundInfo);
                }
                if (found)
                {
                    m_lastChangedPort = foundInfo;
                    m_onRemovedPortInfo.Invoke(foundInfo);

                }
                
            }
        }
        foreach (var port in m_newPortNamesList)
        {
            m_onNewDevice.Invoke(port);
            if (m_informationHolder != null) { 
                m_informationHolder.TryToFindFromCOM(port, out bool found, out ComPortToInfo foundInfo);
                if (found)
                {
                    m_lastChangedPort = foundInfo;
                    m_onNewPortInfo.Invoke(foundInfo);
                
                }
            }

        }

    }
}
