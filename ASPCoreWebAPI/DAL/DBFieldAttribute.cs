﻿namespace ASPCoreWebAPI.DAL
{
    /// <summary>
    /// Specifies the name of the field in the table that the property maps to 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DBFieldAttribute : Attribute
    {
        private string mFieldName;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="fieldName">name of the field that the property will be mapped to</param>
        public DBFieldAttribute(string fieldName)
        {
            mFieldName = fieldName;
        }

        public string FieldName
        {
            get { return mFieldName; }
        }
    }
}
