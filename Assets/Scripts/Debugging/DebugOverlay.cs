using System;
using System.Collections.Generic;
using UnityEngine;

namespace SCARLET.DbOverlay
{
    public class DebugOverlay : MonoBehaviour
    {
        [Header("Sizing")]
        [SerializeField] private Vector2 screenPosition = Vector2.one * 5;
        [SerializeField] private Vector2 screenArea = new Vector2(200, 150);

        private static List<Log> logs = new List<Log>();
        private RectTransform rTransform;
        
        public static DebugOverlay Instance { get; private set; }        

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(this);

            rTransform = GetComponent<RectTransform>();
        }
        private void OnGUI()
        {
            // Set up area
            var drawArea = screenArea;
            var drawPos = screenPosition;
            GUILayout.BeginArea(new Rect(drawPos, drawArea));
            
            // Draw each of the logs in order
            for (int i = 0; i < logs.Count; i++)
            {
                var log = logs[i];
                GUILayout.Label(log.Name + ": " + log.Message);
            }

            GUILayout.EndArea();
        }

        public static void CreateLog(string name)
        {
            // Create new log entry and store it
            logs.Add(new Log(name));
        }
        public static void RemoveLog(string name)
        {
            // Remove an entry by name
            foreach (Log log in logs)
            {
                if (log.Name == name) logs.Remove(log);
            }
        }
        public static void UpdateLog(string name, string content)
        {
            // Update an entry by name
            foreach (Log log in logs)
            {
                if (log.Name == name) log.Message = content;
            }
        }
    }
    internal class Log
    {
        public readonly string Name;
        public string Message;

        public Log(string name)
        {
            Name = name;
        }
        public Log(string name, string message)
            : this(name)
        {
            Message = message;
        }
    }
}