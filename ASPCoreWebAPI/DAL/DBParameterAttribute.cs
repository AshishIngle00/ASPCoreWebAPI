namespace ASPCoreWebAPI.DAL
{
    /// <summary>
    /// Specifies the name of the arameter in the Dbcommand that the property maps to 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DBParameterAttribute : Attribute
    {
        private string mParamName;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="fieldName">name of the parameter that the property will be mapped to</param>
        public DBParameterAttribute(string paramName)
        {
            mParamName = paramName;
        }

        public string ParameterName
        {
            get { return mParamName; }
        }
    }
}
