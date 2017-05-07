using System;
using System.Collections.Generic;
using System.Text;
using System.Dynamic;
using System.Collections.ObjectModel;

namespace ManiaNextControl.Utils
{
    public struct CContractField
    {
        public Type WantedType;
        public object Value;
    }

    public class CContract : DynamicObject
    {
        public string Name { get; set; }

        public bool Locked { get; private set; }
        private Dictionary<string, CContractField> _fields = new Dictionary<string, CContractField>();
        public ReadOnlyDictionary<string, CContractField> AllFields;

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var flag = _fields.TryGetValue(binder.Name.ToLower(), out CContractField __result);
            result = __result;
            return flag;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (Locked)
                return false;

            _fields[binder.Name.ToLower()] = new CContractField()
            {
                WantedType = value.GetType(),
                Value = value
            };
            return true;
        }

        public void Lock()
        {
            Locked = true;
        }
    }

    public struct CContractResult
    {
        public CContract GivenContract;
        public CContract WantedContract;
        public bool IsError;
        public string ErrorText;
        public int ErrorCode;
    }
}
