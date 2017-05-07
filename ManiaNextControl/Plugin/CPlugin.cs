using ManiaNextControl.FileManagement;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using static ManiaNextControl.Manialink.CManialink;

namespace ManiaNextControl.Plugin
{
    public class CPlugin
    {
        public string manialinkPrefix => GetType().FullName.Replace(".", "_").Replace(" ", "").Replace("-", "");
        public CFileIO FileIO;
        public Assembly RunningAssembly => GetType().GetTypeInfo().Assembly;

        public virtual void Init()
        {
            FileIO = new CFileIO();
            FileIO.pathFolder = RunningAssembly.Location.Replace(RunningAssembly.GetName().Name + ".dll", "");
            FieldInfo[] fields = GetType().GetFields();
            foreach (var field in fields)
            {
                Debug.CDebug.Log(field.FieldType.FullName);
                if (field.FieldType.FullName.Contains("CManialink+SharerAction"))
                {
                    var genericType = field.FieldType.GetGenericArguments()[0];
                    Type doneClass = typeof(SharerAction<>).MakeGenericType(genericType);
                    object newObj = Activator.CreateInstance(doneClass, new string[] { field.Name, manialinkPrefix });

                    field.SetValue(this, newObj);
                }
            }
        }

        public virtual void OnServerAdded(string login) { }
        public virtual void OnServerLoaded(string login) { }
    }
}
