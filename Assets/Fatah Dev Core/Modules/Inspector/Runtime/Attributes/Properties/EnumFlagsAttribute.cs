using System;

namespace FatahDev
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class EnumFlagsAttribute : Attribute
    {
        public EnumFlagsAttribute()
        {

        }
    }
}
