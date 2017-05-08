using ManiaNextControl.FileManagement;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static ManiaNextControl.Manialink.CManialink;

namespace ManiaNextControl.Plugin
{
    public class CPlugin
    {
        public string manialinkPrefix => GetType().FullName.Replace(".", "_").Replace(" ", "").Replace("-", "");
        public CFileIO FileIO;
        public Assembly RunningAssembly => GetType().GetTypeInfo().Assembly;

        public virtual Task Init()
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

            return Task.CompletedTask;
        }

        public virtual Task OnServerAdded(string login) { return Task.CompletedTask; }
        public virtual Task OnServerLoaded(string login) { return Task.CompletedTask; }
    }
}
