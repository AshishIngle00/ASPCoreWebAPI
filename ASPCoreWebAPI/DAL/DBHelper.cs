using Microsoft.AspNetCore.Components;
using Microsoft.Data.SqlClient;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Reflection;

namespace ASPCoreWebAPI.DAL
{
    public class DBHelper
    {
        private readonly IConfiguration _configuration;

        public DBHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public static T ReadObject<T>(IDbCommand command) where T : class
        {
            try
            {
                IDataReader reader = command.ExecuteReader();
                if (reader.Read())
                    return GetItemFromReader<T>(reader);
                else
                    return default(T);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get Connection from web.config file & used for connection to command in BLL
        /// </summary>
        SqlConnection con, conuser;
        public SqlConnection GetConnection()
        {
            try
            {
                if (con == null || con.State.ToString() == "Closed")
                {
                    con = new SqlConnection(_configuration.GetConnectionString("orbitDbConnectionString"));

                    con.Open();
                }
                return con;
            }
            catch (Exception) { return null; }
        }

        public SqlConnection GetConnection(string uname)    //logginguser After successful login
        {
            try
            {
                try
                {
                    if (con.State.ToString() == "Open")
                    {
                        con.Close();
                    }
                }
                catch { }
                if (conuser == null || conuser.State.ToString() == "Closed")
                {
                    string cstring = _configuration.GetConnectionString("logginguser");
                    cstring = cstring + "User ID=" + uname + ";Password=orbit@123";
                    conuser = new SqlConnection(cstring);
                    conuser.Open();
                }
                return conuser;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public SqlConnection GetConnectionForOldDb()
        {
            try
            {
                if (con == null || con.State.ToString() == "Closed")
                {
                    con = new SqlConnection(_configuration.GetConnectionString("OldOrbitDbConnectionString"));

                    con.Open();
                }
                return con;
            }
            catch (Exception) { return null; }
        }

        /// <summary>
        /// Get one object from the database by using the mapping information frovided by Mapper class
        /// </summary>
        /// <typeparam name="T">the type of object the collection will hold</typeparam>
        /// <param name="command">DbCommand that is used to read data from the database</param>
        /// <returns>populated object from the database</returns>
        public static T ReadObject<T>(IDbCommand command, Mapper mappingInfo) where T : class
        {
            try
            {
                IDataReader reader = command.ExecuteReader();
                if (reader.Read())
                    return GetItemFromReader<T>(reader, mappingInfo);
                else
                    return default(T);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static T GetItemFromReader<T>(IDataReader rdr)
        {
            try
            {
                Type type = typeof(T);
                T item = Activator.CreateInstance<T>();
                PropertyInfo[] properties = type.GetProperties();

                foreach (PropertyInfo property in properties)
                {
                    // for each property declared in the type provided check if the property is
                    // decorated with the DBField attribute
                    if (Attribute.IsDefined(property, typeof(DBFieldAttribute)))
                    {
                        DBFieldAttribute attrib = (DBFieldAttribute)Attribute.GetCustomAttribute(property, typeof(DBFieldAttribute));

                        // int iIndex = rdr.GetOrdinal(attrib.FieldName); 

                        if (Convert.IsDBNull(rdr[attrib.FieldName])) // data in database is null, so do not set the value of the property
                            continue;

                        if (property.PropertyType == rdr[attrib.FieldName].GetType()) // if the property and database field are the same
                            property.SetValue(item, rdr[attrib.FieldName], null); // set the value of property
                        else
                        {
                            // change the type of the data in table to that of property and set the value of the property
                            property.SetValue(item, Convert.ChangeType(rdr[attrib.FieldName], property.PropertyType), null);
                        }
                    }
                }

                return item;
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Generic method. Gets an object of type T from the data reader. It uses mapping information 
        /// provided to read a field from the reader, and gets the property name and sets
        /// the value of the property with the data which are held in database field
        /// </summary>
        /// <typeparam name="T">The type of object to be instantiated</typeparam>
        /// <param name="rdr">Data Reader where the data will be read from</param>
        /// <param name="mappings">mapping information</param>
        /// <returns>an instance of type T with the properties populated from database</returns>
        private static T GetItemFromReader<T>(IDataReader rdr, Mapper mappings)
        {
            try
            {
                Type type = typeof(T);
                T item = Activator.CreateInstance<T>(); // create an instance of the type provided
                foreach (string map in mappings.MappingInformation)
                {
                    // for each mapping information 
                    string property = Mapper.GetProperty(map);
                    string field = Mapper.GetField(map);

                    PropertyInfo propInfo = type.GetProperty(property); // ge the property by name

                    if (Convert.IsDBNull(rdr[field])) // data in database is null, so do not set the value of the property
                        continue;

                    if (propInfo.PropertyType == rdr[field].GetType()) // if the property and database field are the same
                        propInfo.SetValue(item, rdr[field], null); // set the value of property
                    else
                    {
                        // change the type of the data in table to that of property and set the value of the property
                        propInfo.SetValue(item, Convert.ChangeType(rdr[field], propInfo.PropertyType), null);
                    }
                }
                return item;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<T> ReadCollection<T>(IDbCommand command) where T : class
        {
            try
            {
                List<T> collection = new List<T>();
                IDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    T item = GetItemFromReader<T>(reader);
                    collection.Add(item);
                }

                return collection;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Get a collection of objects from the database by using the mapping information provided 
        /// by Mapper class
        /// </summary>
        /// <typeparam name="T">the type of object the collection will hold</typeparam>
        /// <param name="command">DbCommand that is used to read data from the database</param>
        /// <returns>>a collection of populated objects from the database</returns>
        public static List<T> ReadCollection<T>(IDbCommand command, Mapper mappingInfo) where T : class
        {
            try
            {
                List<T> collection = new List<T>();
                IDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    T item = GetItemFromReader<T>(reader, mappingInfo);
                    collection.Add(item);
                }

                return collection;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<T> ReadCollection<T, K>(IDbCommand command, Mapper mappingInfo)
        {
            try
            {

                List<T> collection = new List<T>();
                List<K> collection1 = new List<K>();

                IDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    T item = GetItemFromReader<T>(reader, mappingInfo);
                    collection.Add(item);
                }

                reader.NextResult();

                while (reader.Read())
                {
                    K item1 = GetItemFromReader<K>(reader, mappingInfo);
                    collection1.Add(item1);
                }
                return collection;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
            }
        }

        /// <summary>
        /// Returning Two Table's From DB.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <param name="command"></param>
        /// <returns></returns>
        public static Dictionary<string, object> ReadCollection<T, K>(IDbCommand command, List<Mapper> mapperList)
        {
            try
            {
                Dictionary<string, object> collectionOfObjects = new Dictionary<string, object>();
                List<T> collectionFirst = new List<T>();
                List<K> collectionSecond = new List<K>();
                IDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    T item = GetItemFromReader<T>(reader, mapperList[0]);
                    collectionFirst.Add(item);

                }

                collectionOfObjects.Add("T", collectionFirst);
                reader.NextResult();
                while (reader.Read())
                {
                    K item = GetItemFromReader<K>(reader, mapperList[1]);
                    collectionSecond.Add(item);
                }
                collectionOfObjects.Add("K", collectionSecond);
                //do
                //{
                // while (reader.Read())
                // {
                // T item = GetItemFromReader<T>(reader);
                // collection.Add(item);
                // }
                // Console.WriteLine("".PadLeft(60, '='));
                //} while (reader.NextResult());
                return collectionOfObjects;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Returning Three Tables from DB.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="M"></typeparam>
        /// <param name="command"></param>
        /// <returns></returns>
        public static Dictionary<string, object> ReadCollection<T, K, M>(IDbCommand command, List<Mapper> mapperList)
        {
            try
            {
                Dictionary<string, object> collectionOfObjects = new Dictionary<string, object>();
                List<T> collectionFirst = new List<T>();
                List<K> collectionSecond = new List<K>();
                List<M> collectionThree = new List<M>();

                IDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    T item = GetItemFromReader<T>(reader, mapperList[0]);
                    collectionFirst.Add(item);
                }

                collectionOfObjects.Add("T", collectionFirst);

                reader.NextResult();
                while (reader.Read())
                {
                    K item = GetItemFromReader<K>(reader, mapperList[1]);
                    collectionSecond.Add(item);
                }
                collectionOfObjects.Add("K", collectionSecond);

                reader.NextResult();
                while (reader.Read())
                {
                    M item = GetItemFromReader<M>(reader, mapperList[2]);
                    collectionThree.Add(item);
                }
                collectionOfObjects.Add("M", collectionThree);

                return collectionOfObjects;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Returning Four Table's from DB
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="M"></typeparam>
        /// <typeparam name="N"></typeparam>
        /// <param name="command"></param>
        /// <returns></returns>
        public static Dictionary<string, object> ReadCollection<T, K, M, N>(IDbCommand command, List<Mapper> lstMapper)
        {
            try
            {
                Dictionary<string, object> collectionOfObjects = new Dictionary<string, object>();
                List<T> collectionFirst = new List<T>();
                List<K> collectionSecond = new List<K>();
                List<M> collectionThree = new List<M>();
                List<N> collectionFour = new List<N>();

                IDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    T item = GetItemFromReader<T>(reader, lstMapper[0]);
                    collectionFirst.Add(item);
                }

                collectionOfObjects.Add("T", collectionFirst);

                reader.NextResult();
                while (reader.Read())
                {
                    K item = GetItemFromReader<K>(reader, lstMapper[1]);
                    collectionSecond.Add(item);
                }
                collectionOfObjects.Add("K", collectionSecond);

                reader.NextResult();
                while (reader.Read())
                {
                    M item = GetItemFromReader<M>(reader, lstMapper[2]);
                    collectionThree.Add(item);
                }
                collectionOfObjects.Add("M", collectionThree);

                reader.NextResult();
                while (reader.Read())
                {
                    N item = GetItemFromReader<N>(reader, lstMapper[3]);
                    collectionFour.Add(item);
                }
                collectionOfObjects.Add("N", collectionFour);


                return collectionOfObjects;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Returning Five Table's from DB
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="M"></typeparam>
        /// <typeparam name="N"></typeparam>
        /// <typeparam name="P"></typeparam>
        /// <param name="command"></param>
        /// <returns></returns>
        /// By Vasant
        public static Dictionary<string, object> ReadCollection<T, K, M, N, O>(IDbCommand command, List<Mapper> lstMapper)
        {
            try
            {
                Dictionary<string, object> collectionOfObjects = new Dictionary<string, object>();
                List<T> collectionFirst = new List<T>();
                List<K> collectionSecond = new List<K>();
                List<M> collectionThree = new List<M>();
                List<N> collectionFour = new List<N>();
                List<O> collectionFive = new List<O>();

                IDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    T item = GetItemFromReader<T>(reader, lstMapper[0]);
                    collectionFirst.Add(item);
                }

                collectionOfObjects.Add("T", collectionFirst);

                reader.NextResult();
                while (reader.Read())
                {
                    K item = GetItemFromReader<K>(reader, lstMapper[1]);
                    collectionSecond.Add(item);
                }
                collectionOfObjects.Add("K", collectionSecond);

                reader.NextResult();
                while (reader.Read())
                {
                    M item = GetItemFromReader<M>(reader, lstMapper[2]);
                    collectionThree.Add(item);
                }
                collectionOfObjects.Add("M", collectionThree);

                reader.NextResult();
                while (reader.Read())
                {
                    N item = GetItemFromReader<N>(reader, lstMapper[3]);
                    collectionFour.Add(item);
                }
                collectionOfObjects.Add("N", collectionFour);

                reader.NextResult();
                while (reader.Read())
                {
                    O item = GetItemFromReader<O>(reader, lstMapper[4]);
                    collectionFive.Add(item);
                }
                collectionOfObjects.Add("O", collectionFive);

                return collectionOfObjects;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Returning Five Table's from DB
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="M"></typeparam>
        /// <typeparam name="N"></typeparam>
        /// <typeparam name="P"></typeparam>
        /// <param name="command"></param>
        /// <returns></returns>
        public static Dictionary<string, object> ReadCollection<T, K, M, N, O>(IDbCommand command)
        {
            try
            {
                Dictionary<string, object> collectionOfObjects = new Dictionary<string, object>();
                List<T> collectionFirst = new List<T>();
                List<K> collectionSecond = new List<K>();
                List<M> collectionThree = new List<M>();
                List<N> collectionFour = new List<N>();
                List<O> collectionFive = new List<O>();

                IDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    T item = GetItemFromReader<T>(reader);
                    collectionFirst.Add(item);
                }

                collectionOfObjects.Add("T", collectionFirst);

                reader.NextResult();
                while (reader.Read())
                {
                    K item = GetItemFromReader<K>(reader);
                    collectionSecond.Add(item);
                }
                collectionOfObjects.Add("K", collectionSecond);

                reader.NextResult();
                while (reader.Read())
                {
                    M item = GetItemFromReader<M>(reader);
                    collectionThree.Add(item);
                }
                collectionOfObjects.Add("M", collectionThree);

                reader.NextResult();
                while (reader.Read())
                {
                    N item = GetItemFromReader<N>(reader);
                    collectionFour.Add(item);
                }
                collectionOfObjects.Add("N", collectionFour);

                reader.NextResult();
                while (reader.Read())
                {
                    O item = GetItemFromReader<O>(reader);
                    collectionFive.Add(item);
                }
                collectionOfObjects.Add("O", collectionFive);


                return collectionOfObjects;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Returning Five Table's from DB
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="M"></typeparam>
        /// <typeparam name="N"></typeparam>
        /// <typeparam name="P"></typeparam>
        /// <param name="command"></param>
        /// <returns></returns>
        /// By Vasant
        public static Dictionary<string, object> ReadCollection<T, K, M, N, O, P>(IDbCommand command, List<Mapper> lstMapper)
        {
            try
            {
                Dictionary<string, object> collectionOfObjects = new Dictionary<string, object>();
                List<T> collectionFirst = new List<T>();
                List<K> collectionSecond = new List<K>();
                List<M> collectionThree = new List<M>();
                List<N> collectionFour = new List<N>();
                List<O> collectionFive = new List<O>();
                List<P> collectionSix = new List<P>();

                IDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    T item = GetItemFromReader<T>(reader, lstMapper[0]);
                    collectionFirst.Add(item);
                }

                collectionOfObjects.Add("T", collectionFirst);

                reader.NextResult();
                while (reader.Read())
                {
                    K item = GetItemFromReader<K>(reader, lstMapper[1]);
                    collectionSecond.Add(item);
                }
                collectionOfObjects.Add("K", collectionSecond);

                reader.NextResult();
                while (reader.Read())
                {
                    M item = GetItemFromReader<M>(reader, lstMapper[2]);
                    collectionThree.Add(item);
                }
                collectionOfObjects.Add("M", collectionThree);

                reader.NextResult();
                while (reader.Read())
                {
                    N item = GetItemFromReader<N>(reader, lstMapper[3]);
                    collectionFour.Add(item);
                }
                collectionOfObjects.Add("N", collectionFour);

                reader.NextResult();
                while (reader.Read())
                {
                    O item = GetItemFromReader<O>(reader, lstMapper[4]);
                    collectionFive.Add(item);
                }
                collectionOfObjects.Add("O", collectionFive);

                reader.NextResult();
                while (reader.Read())
                {
                    P item = GetItemFromReader<P>(reader, lstMapper[5]);
                    collectionSix.Add(item);
                }
                collectionOfObjects.Add("P", collectionSix);

                return collectionOfObjects;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Returning Five Table's from DB
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="M"></typeparam>
        /// <typeparam name="N"></typeparam>
        /// <typeparam name="P"></typeparam>
        /// <param name="command"></param>
        /// <returns></returns>
        public static Dictionary<string, object> ReadCollection<T, K, M, N, O, P>(IDbCommand command)
        {
            try
            {
                Dictionary<string, object> collectionOfObjects = new Dictionary<string, object>();
                List<T> collectionFirst = new List<T>();
                List<K> collectionSecond = new List<K>();
                List<M> collectionThree = new List<M>();
                List<N> collectionFour = new List<N>();
                List<O> collectionFive = new List<O>();
                List<P> collectionSix = new List<P>();

                IDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    T item = GetItemFromReader<T>(reader);
                    collectionFirst.Add(item);
                }

                collectionOfObjects.Add("T", collectionFirst);

                reader.NextResult();
                while (reader.Read())
                {
                    K item = GetItemFromReader<K>(reader);
                    collectionSecond.Add(item);
                }
                collectionOfObjects.Add("K", collectionSecond);

                reader.NextResult();
                while (reader.Read())
                {
                    M item = GetItemFromReader<M>(reader);
                    collectionThree.Add(item);
                }
                collectionOfObjects.Add("M", collectionThree);

                reader.NextResult();
                while (reader.Read())
                {
                    N item = GetItemFromReader<N>(reader);
                    collectionFour.Add(item);
                }
                collectionOfObjects.Add("N", collectionFour);

                reader.NextResult();
                while (reader.Read())
                {
                    O item = GetItemFromReader<O>(reader);
                    collectionFive.Add(item);
                }
                collectionOfObjects.Add("O", collectionFive);

                reader.NextResult();
                while (reader.Read())
                {
                    P item = GetItemFromReader<P>(reader);
                    collectionSix.Add(item);
                }
                collectionOfObjects.Add("P", collectionSix);


                return collectionOfObjects;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Returning Five Table's from DB
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="M"></typeparam>
        /// <typeparam name="N"></typeparam>
        /// <typeparam name="P"></typeparam>
        /// <typeparam name="Q"></typeparam>
        /// <param name="command"></param>
        /// <returns></returns>
        /// By Vasant
        public static Dictionary<string, object> ReadCollection<T, K, M, N, O, P, Q>(IDbCommand command, List<Mapper> lstMapper)
        {
            try
            {
                Dictionary<string, object> collectionOfObjects = new Dictionary<string, object>();
                List<T> collectionFirst = new List<T>();
                List<K> collectionSecond = new List<K>();
                List<M> collectionThree = new List<M>();
                List<N> collectionFour = new List<N>();
                List<O> collectionFive = new List<O>();
                List<P> collectionSix = new List<P>();
                List<Q> collectionSeven = new List<Q>();

                IDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    T item = GetItemFromReader<T>(reader, lstMapper[0]);
                    collectionFirst.Add(item);
                }

                collectionOfObjects.Add("T", collectionFirst);

                reader.NextResult();
                while (reader.Read())
                {
                    K item = GetItemFromReader<K>(reader, lstMapper[1]);
                    collectionSecond.Add(item);
                }
                collectionOfObjects.Add("K", collectionSecond);

                reader.NextResult();
                while (reader.Read())
                {
                    M item = GetItemFromReader<M>(reader, lstMapper[2]);
                    collectionThree.Add(item);
                }
                collectionOfObjects.Add("M", collectionThree);

                reader.NextResult();
                while (reader.Read())
                {
                    N item = GetItemFromReader<N>(reader, lstMapper[3]);
                    collectionFour.Add(item);
                }
                collectionOfObjects.Add("N", collectionFour);

                reader.NextResult();
                while (reader.Read())
                {
                    O item = GetItemFromReader<O>(reader, lstMapper[4]);
                    collectionFive.Add(item);
                }
                collectionOfObjects.Add("O", collectionFive);

                reader.NextResult();
                while (reader.Read())
                {
                    P item = GetItemFromReader<P>(reader, lstMapper[5]);
                    collectionSix.Add(item);
                }
                collectionOfObjects.Add("P", collectionSix);

                reader.NextResult();
                while (reader.Read())
                {
                    Q item = GetItemFromReader<Q>(reader, lstMapper[6]);
                    collectionSeven.Add(item);
                }
                collectionOfObjects.Add("Q", collectionSeven);

                return collectionOfObjects;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Returning Five Table's from DB
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="M"></typeparam>
        /// <typeparam name="N"></typeparam>
        /// <typeparam name="P"></typeparam>
        /// <typeparam name="Q"></typeparam>
        /// <param name="command"></param>
        /// <returns></returns>
        public static Dictionary<string, object> ReadCollection<T, K, M, N, O, P, Q>(IDbCommand command)
        {
            try
            {
                Dictionary<string, object> collectionOfObjects = new Dictionary<string, object>();
                List<T> collectionFirst = new List<T>();
                List<K> collectionSecond = new List<K>();
                List<M> collectionThree = new List<M>();
                List<N> collectionFour = new List<N>();
                List<O> collectionFive = new List<O>();
                List<P> collectionSix = new List<P>();
                List<Q> collectionSeven = new List<Q>();

                IDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    T item = GetItemFromReader<T>(reader);
                    collectionFirst.Add(item);
                }

                collectionOfObjects.Add("T", collectionFirst);

                reader.NextResult();
                while (reader.Read())
                {
                    K item = GetItemFromReader<K>(reader);
                    collectionSecond.Add(item);
                }
                collectionOfObjects.Add("K", collectionSecond);

                reader.NextResult();
                while (reader.Read())
                {
                    M item = GetItemFromReader<M>(reader);
                    collectionThree.Add(item);
                }
                collectionOfObjects.Add("M", collectionThree);

                reader.NextResult();
                while (reader.Read())
                {
                    N item = GetItemFromReader<N>(reader);
                    collectionFour.Add(item);
                }
                collectionOfObjects.Add("N", collectionFour);

                reader.NextResult();
                while (reader.Read())
                {
                    O item = GetItemFromReader<O>(reader);
                    collectionFive.Add(item);
                }
                collectionOfObjects.Add("O", collectionFive);

                reader.NextResult();
                while (reader.Read())
                {
                    P item = GetItemFromReader<P>(reader);
                    collectionSix.Add(item);
                }
                collectionOfObjects.Add("P", collectionSix);

                reader.NextResult();
                while (reader.Read())
                {
                    Q item = GetItemFromReader<Q>(reader);
                    collectionSeven.Add(item);
                }
                collectionOfObjects.Add("Q", collectionSeven);

                return collectionOfObjects;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Returning Five Table's from DB
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="M"></typeparam>
        /// <typeparam name="N"></typeparam>
        /// <typeparam name="P"></typeparam>
        /// <typeparam name="Q"></typeparam>
        /// <typeparam name="S"></typeparam>
        /// <param name="command"></param>
        /// <returns></returns>
        /// By Vasant
        public static Dictionary<string, object> ReadCollection<T, K, M, N, O, P, Q, S>(IDbCommand command, List<Mapper> lstMapper)
        {
            try
            {
                Dictionary<string, object> collectionOfObjects = new Dictionary<string, object>();
                List<T> collectionFirst = new List<T>();
                List<K> collectionSecond = new List<K>();
                List<M> collectionThree = new List<M>();
                List<N> collectionFour = new List<N>();
                List<O> collectionFive = new List<O>();
                List<P> collectionSix = new List<P>();
                List<Q> collectionSeven = new List<Q>();
                List<S> collectionEight = new List<S>();

                IDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    T item = GetItemFromReader<T>(reader, lstMapper[0]);
                    collectionFirst.Add(item);
                }

                collectionOfObjects.Add("T", collectionFirst);

                reader.NextResult();
                while (reader.Read())
                {
                    K item = GetItemFromReader<K>(reader, lstMapper[1]);
                    collectionSecond.Add(item);
                }
                collectionOfObjects.Add("K", collectionSecond);

                reader.NextResult();
                while (reader.Read())
                {
                    M item = GetItemFromReader<M>(reader, lstMapper[2]);
                    collectionThree.Add(item);
                }
                collectionOfObjects.Add("M", collectionThree);

                reader.NextResult();
                while (reader.Read())
                {
                    N item = GetItemFromReader<N>(reader, lstMapper[3]);
                    collectionFour.Add(item);
                }
                collectionOfObjects.Add("N", collectionFour);

                reader.NextResult();
                while (reader.Read())
                {
                    O item = GetItemFromReader<O>(reader, lstMapper[4]);
                    collectionFive.Add(item);
                }
                collectionOfObjects.Add("O", collectionFive);

                reader.NextResult();
                while (reader.Read())
                {
                    P item = GetItemFromReader<P>(reader, lstMapper[5]);
                    collectionSix.Add(item);
                }
                collectionOfObjects.Add("P", collectionSix);

                reader.NextResult();
                while (reader.Read())
                {
                    Q item = GetItemFromReader<Q>(reader, lstMapper[6]);
                    collectionSeven.Add(item);
                }
                collectionOfObjects.Add("Q", collectionSeven);

                reader.NextResult();
                while (reader.Read())
                {
                    S item = GetItemFromReader<S>(reader, lstMapper[7]);
                    collectionEight.Add(item);
                }
                collectionOfObjects.Add("S", collectionEight);

                return collectionOfObjects;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Returning Five Table's from DB
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="M"></typeparam>
        /// <typeparam name="N"></typeparam>
        /// <typeparam name="P"></typeparam>
        /// <typeparam name="Q"></typeparam>
        /// <typeparam name="S"></typeparam>
        /// <param name="command"></param>
        /// <returns></returns>
        public static Dictionary<string, object> ReadCollection<T, K, M, N, O, P, Q, S>(IDbCommand command)
        {
            try
            {
                Dictionary<string, object> collectionOfObjects = new Dictionary<string, object>();
                List<T> collectionFirst = new List<T>();
                List<K> collectionSecond = new List<K>();
                List<M> collectionThree = new List<M>();
                List<N> collectionFour = new List<N>();
                List<O> collectionFive = new List<O>();
                List<P> collectionSix = new List<P>();
                List<Q> collectionSeven = new List<Q>();
                List<S> collectionEight = new List<S>();

                IDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    T item = GetItemFromReader<T>(reader);
                    collectionFirst.Add(item);
                }

                collectionOfObjects.Add("T", collectionFirst);

                reader.NextResult();
                while (reader.Read())
                {
                    K item = GetItemFromReader<K>(reader);
                    collectionSecond.Add(item);
                }
                collectionOfObjects.Add("K", collectionSecond);

                reader.NextResult();
                while (reader.Read())
                {
                    M item = GetItemFromReader<M>(reader);
                    collectionThree.Add(item);
                }
                collectionOfObjects.Add("M", collectionThree);

                reader.NextResult();
                while (reader.Read())
                {
                    N item = GetItemFromReader<N>(reader);
                    collectionFour.Add(item);
                }
                collectionOfObjects.Add("N", collectionFour);

                reader.NextResult();
                while (reader.Read())
                {
                    O item = GetItemFromReader<O>(reader);
                    collectionFive.Add(item);
                }
                collectionOfObjects.Add("O", collectionFive);

                reader.NextResult();
                while (reader.Read())
                {
                    P item = GetItemFromReader<P>(reader);
                    collectionSix.Add(item);
                }
                collectionOfObjects.Add("P", collectionSix);

                reader.NextResult();
                while (reader.Read())
                {
                    Q item = GetItemFromReader<Q>(reader);
                    collectionSeven.Add(item);
                }
                collectionOfObjects.Add("Q", collectionSeven);

                reader.NextResult();
                while (reader.Read())
                {
                    S item = GetItemFromReader<S>(reader);
                    collectionEight.Add(item);
                }
                collectionOfObjects.Add("S", collectionEight);

                return collectionOfObjects;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Returning Two Table's From DB.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <param name="command"></param>
        /// <returns></returns>
        public static Dictionary<string, object> ReadCollection<T, K>(IDbCommand command)
        {
            try
            {
                Dictionary<string, object> collectionOfObjects = new Dictionary<string, object>();
                List<T> collectionFirst = new List<T>();
                List<K> collectionSecond = new List<K>();
                IDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    T item = GetItemFromReader<T>(reader);
                    collectionFirst.Add(item);

                }

                collectionOfObjects.Add("T", collectionFirst);
                reader.NextResult();
                while (reader.Read())
                {
                    K item = GetItemFromReader<K>(reader);
                    collectionSecond.Add(item);
                }
                collectionOfObjects.Add("K", collectionSecond);
                //do
                //{
                // while (reader.Read())
                // {
                // T item = GetItemFromReader<T>(reader);
                // collection.Add(item);
                // }
                // Console.WriteLine("".PadLeft(60, '='));
                //} while (reader.NextResult());
                return collectionOfObjects;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Returning Three Tables from DB.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="M"></typeparam>
        /// <param name="command"></param>
        /// <returns></returns>
        public static Dictionary<string, object> ReadCollection<T, K, M>(IDbCommand command)
        {
            try
            {
                Dictionary<string, object> collectionOfObjects = new Dictionary<string, object>();
                List<T> collectionFirst = new List<T>();
                List<K> collectionSecond = new List<K>();
                List<M> collectionThree = new List<M>();

                IDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    T item = GetItemFromReader<T>(reader);
                    collectionFirst.Add(item);
                }

                collectionOfObjects.Add("T", collectionFirst);

                reader.NextResult();
                while (reader.Read())
                {
                    K item = GetItemFromReader<K>(reader);
                    collectionSecond.Add(item);
                }
                collectionOfObjects.Add("K", collectionSecond);

                reader.NextResult();
                while (reader.Read())
                {
                    M item = GetItemFromReader<M>(reader);
                    collectionThree.Add(item);
                }
                collectionOfObjects.Add("M", collectionThree);

                return collectionOfObjects;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Returning Four Table's from DB
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="M"></typeparam>
        /// <typeparam name="N"></typeparam>
        /// <param name="command"></param>
        /// <returns></returns>
        public static Dictionary<string, object> ReadCollection<T, K, M, N>(IDbCommand command)
        {
            try
            {
                Dictionary<string, object> collectionOfObjects = new Dictionary<string, object>();
                List<T> collectionFirst = new List<T>();
                List<K> collectionSecond = new List<K>();
                List<M> collectionThree = new List<M>();
                List<N> collectionFour = new List<N>();

                IDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    T item = GetItemFromReader<T>(reader);
                    collectionFirst.Add(item);
                }

                collectionOfObjects.Add("T", collectionFirst);

                reader.NextResult();
                while (reader.Read())
                {
                    K item = GetItemFromReader<K>(reader);
                    collectionSecond.Add(item);
                }
                collectionOfObjects.Add("K", collectionSecond);

                reader.NextResult();
                while (reader.Read())
                {
                    M item = GetItemFromReader<M>(reader);
                    collectionThree.Add(item);
                }
                collectionOfObjects.Add("M", collectionThree);

                reader.NextResult();
                while (reader.Read())
                {
                    N item = GetItemFromReader<N>(reader);
                    collectionFour.Add(item);
                }
                collectionOfObjects.Add("N", collectionFour);


                return collectionOfObjects;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Saves the object into database using attribute information
        /// </summary>
        /// <param name="obj">the object to be saved</param>
        /// <param name="command">the Dbcommand used to save the object</param>
        public static void SaveObject(object obj, IDbCommand command)
        {
            try
            {
                Type type = obj.GetType();
                PropertyInfo[] properties = type.GetProperties();

                foreach (PropertyInfo property in properties)
                {
                    // for each property declared in the type provided check if the property is
                    // decorated with the DBField attribute
                    if (Attribute.IsDefined(property, typeof(DBParameterAttribute)))
                    {
                        DBParameterAttribute attrib = (DBParameterAttribute)Attribute.GetCustomAttribute(property, typeof(DBParameterAttribute));
                        IDataParameter param = (IDataParameter)command.Parameters[attrib.ParameterName]; // get parameter
                        param.Value = property.GetValue(obj, null); // set parameter value
                    }
                }

                command.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Saves the object into database using mapping information
        /// </summary>
        /// <param name="obj">object ot be saved</param>
        /// <param name="command">the Dbcommand used to save the objec</param>
        /// <param name="mappingInfo">mapping information (property=Sql Parameter)</param>
        public static void SaveObject(object obj, IDbCommand command, Mapper mappingInfo)
        {
            try
            {
                Type type = obj.GetType();
                foreach (string map in mappingInfo.MappingInformation)
                {
                    // for each mapping information 
                    string property = Mapper.GetProperty(map);
                    string parameter = Mapper.GetParameter(map);

                    PropertyInfo propInfo = type.GetProperty(property); // ge the property by name
                    IDataParameter param = (IDataParameter)command.Parameters[parameter]; // get the parameter
                    param.Value = propInfo.GetValue(obj, null); // set paramter value
                }

                command.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void SaveCollection(IList collection, IDbCommand command)
        {
            try
            {
                foreach (object item in collection)
                {
                    SaveObject(item, command);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void SaveCollection(IList collection, IDbCommand command, Mapper mappingInfo)
        {
            foreach (object item in collection)
            {
                SaveObject(item, command, mappingInfo);
            }
        }

        /// <summary>
        /// Saves the object into database using attribute information
        /// </summary>
        /// <param name="obj">the object to be saved</param>
        /// <param name="command">the Dbcommand used to save the object</param>
        public static List<T> SaveObject<T>(object obj, IDbCommand command, IDbDataAdapter adapter)
        {
            try
            {
                DataSet ds = new DataSet();
                DataTable dt = CreateTable<T>();


                Type type = obj.GetType();
                PropertyInfo[] properties = type.GetProperties();

                foreach (PropertyInfo property in properties)
                {
                    // for each property declared in the type provided check if the property is
                    // decorated with the DBField attribute
                    if (Attribute.IsDefined(property, typeof(DBParameterAttribute)))
                    {
                        DBParameterAttribute attrib = (DBParameterAttribute)Attribute.GetCustomAttribute(property, typeof(DBParameterAttribute));
                        IDataParameter param = (IDataParameter)command.Parameters[attrib.ParameterName]; // get parameter
                        param.Value = property.GetValue(obj, null); // set parameter value
                    }
                }

                adapter.SelectCommand = command;

                adapter.Fill(ds);
                dt = ds.Tables[0];

                List<T> returnList = FromDataTableToList<T>(dt);

                return returnList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Saves the object into database using attribute information
        /// </summary>
        /// <param name="obj">the object to be saved</param>
        /// <param name="command">the Dbcommand used to save the object</param>
        public static Dictionary<string, object> SaveObject<T, K>(object obj, IDbCommand command, IDbDataAdapter adapter)
        {
            try
            {
                DataSet ds = new DataSet();
                DataTable dtFirst = CreateTable<T>();
                DataTable dtSecond = CreateTable<K>();


                Type type = obj.GetType();
                PropertyInfo[] properties = type.GetProperties();

                foreach (PropertyInfo property in properties)
                {
                    // for each property declared in the type provided check if the property is
                    // decorated with the DBField attribute
                    if (Attribute.IsDefined(property, typeof(DBParameterAttribute)))
                    {
                        DBParameterAttribute attrib = (DBParameterAttribute)Attribute.GetCustomAttribute(property, typeof(DBParameterAttribute));
                        IDataParameter param = (IDataParameter)command.Parameters[attrib.ParameterName]; // get parameter
                        param.Value = property.GetValue(obj, null); // set parameter value
                    }
                }

                adapter.SelectCommand = command;
                adapter.Fill(ds);
                dtFirst = ds.Tables[0];
                dtSecond = ds.Tables[1];

                List<T> firstTable = FromDataTableToList<T>(dtFirst);
                List<K> secondTable = FromDataTableToList<K>(dtSecond);

                Dictionary<string, object> dicTables = new Dictionary<string, object>();
                dicTables.Add("T", firstTable);
                dicTables.Add("K", secondTable);


                return dicTables;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Saves the object into database using attribute information
        /// </summary>
        /// <param name="obj">the object to be saved</param>
        /// <param name="command">the Dbcommand used to save the object</param>
        public static Dictionary<string, object> SaveObject<T, K, M>(object obj, IDbCommand command, Mapper mappingInfo, IDbDataAdapter adapter)
        {
            try
            {
                DataSet ds = new DataSet();
                DataTable dtFirst = CreateTable<T>();
                DataTable dtSecond = CreateTable<K>();
                DataTable dtThird = CreateTable<M>();


                Type type = obj.GetType();
                PropertyInfo[] properties = type.GetProperties();

                //foreach (PropertyInfo property in properties)
                //{
                //    // for each property declared in the type provided check if the property is
                //    // decorated with the DBField attribute
                //    if (Attribute.IsDefined(property, typeof(DBParameterAttribute)))
                //    {
                //        DBParameterAttribute attrib = (DBParameterAttribute)Attribute.GetCustomAttribute(property, typeof(DBParameterAttribute));
                //        IDataParameter param = (IDataParameter)command.Parameters[attrib.ParameterName]; // get parameter
                //        param.Value = property.GetValue(obj, null); // set parameter value
                //    }
                //}

                foreach (string map in mappingInfo.MappingInformation)
                {
                    // for each mapping information 
                    string property = Mapper.GetProperty(map);
                    string parameter = Mapper.GetParameter(map);

                    PropertyInfo propInfo = type.GetProperty(property); // ge the property by name
                    IDataParameter param = (IDataParameter)command.Parameters[parameter]; // get the parameter
                    param.Value = propInfo.GetValue(obj, null); // set paramter value
                }

                adapter.SelectCommand = command;
                adapter.Fill(ds);
                dtFirst = ds.Tables[0];
                dtSecond = ds.Tables[1];
                dtThird = ds.Tables[2];

                List<T> firstTable = FromDataTableToList<T>(dtFirst);
                List<K> secondTable = FromDataTableToList<K>(dtSecond);
                List<M> thirdTable = FromDataTableToList<M>(dtThird);

                Dictionary<string, object> dicTables = new Dictionary<string, object>();
                dicTables.Add("T", firstTable);
                dicTables.Add("K", secondTable);
                dicTables.Add("M", thirdTable);

                return dicTables;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static Dictionary<string, object> SaveObject<T, K, M, N>(object obj, IDbCommand command, Mapper mappingInfo, IDbDataAdapter adapter)
        {
            try
            {
                DataSet ds = new DataSet();
                DataTable dtFirst = CreateTable<T>();
                DataTable dtSecond = CreateTable<K>();
                DataTable dtThird = CreateTable<M>();
                DataTable dtFour = CreateTable<N>();

                Type type = obj.GetType();
                PropertyInfo[] properties = type.GetProperties();

                //foreach (PropertyInfo property in properties)
                //{
                //    // for each property declared in the type provided check if the property is
                //    // decorated with the DBField attribute
                //    if (Attribute.IsDefined(property, typeof(DBParameterAttribute)))
                //    {
                //        DBParameterAttribute attrib = (DBParameterAttribute)Attribute.GetCustomAttribute(property, typeof(DBParameterAttribute));
                //        IDataParameter param = (IDataParameter)command.Parameters[attrib.ParameterName]; // get parameter
                //        param.Value = property.GetValue(obj, null); // set parameter value
                //    }
                //}
                foreach (string map in mappingInfo.MappingInformation)
                {
                    // for each mapping information 
                    string property = Mapper.GetProperty(map);
                    string parameter = Mapper.GetParameter(map);

                    PropertyInfo propInfo = type.GetProperty(property); // ge the property by name
                    IDataParameter param = (IDataParameter)command.Parameters[parameter]; // get the parameter
                    param.Value = propInfo.GetValue(obj, null); // set paramter value
                }

                adapter.SelectCommand = command;
                adapter.Fill(ds);
                dtFirst = ds.Tables[0];
                dtSecond = ds.Tables[1];
                dtThird = ds.Tables[2];
                dtFour = ds.Tables[3];

                List<T> firstTable = FromDataTableToList<T>(dtFirst);
                List<K> secondTable = FromDataTableToList<K>(dtSecond);
                List<M> thirdTable = FromDataTableToList<M>(dtThird);
                List<N> fourTable = FromDataTableToList<N>(dtFour);

                Dictionary<string, object> dicTables = new Dictionary<string, object>();
                dicTables.Add("T", firstTable);
                dicTables.Add("K", secondTable);
                dicTables.Add("M", thirdTable);
                dicTables.Add("N", fourTable);

                return dicTables;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static Dictionary<string, object> SaveObject<T, K, M, N, O>(object obj, IDbCommand command, Mapper mappingInfo, IDbDataAdapter adapter)
        {
            try
            {
                DataSet ds = new DataSet();
                DataTable dtFirst = CreateTable<T>();
                DataTable dtSecond = CreateTable<K>();
                DataTable dtThird = CreateTable<M>();
                DataTable dtFour = CreateTable<N>();
                DataTable dtFive = CreateTable<O>();

                Type type = obj.GetType();
                PropertyInfo[] properties = type.GetProperties();

                //foreach (PropertyInfo property in properties)
                //{
                //    // for each property declared in the type provided check if the property is
                //    // decorated with the DBField attribute
                //    if (Attribute.IsDefined(property, typeof(DBParameterAttribute)))
                //    {
                //        DBParameterAttribute attrib = (DBParameterAttribute)Attribute.GetCustomAttribute(property, typeof(DBParameterAttribute));
                //        IDataParameter param = (IDataParameter)command.Parameters[attrib.ParameterName]; // get parameter
                //        param.Value = property.GetValue(obj, null); // set parameter value
                //    }
                //}

                foreach (string map in mappingInfo.MappingInformation)
                {
                    // for each mapping information 
                    string property = Mapper.GetProperty(map);
                    string parameter = Mapper.GetParameter(map);

                    PropertyInfo propInfo = type.GetProperty(property); // ge the property by name
                    IDataParameter param = (IDataParameter)command.Parameters[parameter]; // get the parameter
                    param.Value = propInfo.GetValue(obj, null); // set paramter value
                }

                adapter.SelectCommand = command;
                adapter.Fill(ds);
                dtFirst = ds.Tables[0];
                dtSecond = ds.Tables[1];
                dtThird = ds.Tables[2];
                dtFour = ds.Tables[3];
                dtFive = ds.Tables[4];

                List<T> firstTable = FromDataTableToList<T>(dtFirst);
                List<K> secondTable = FromDataTableToList<K>(dtSecond);
                List<M> thirdTable = FromDataTableToList<M>(dtThird);
                List<N> fourTable = FromDataTableToList<N>(dtFour);
                List<O> fiveTable = FromDataTableToList<O>(dtFive);

                Dictionary<string, object> dicTables = new Dictionary<string, object>();
                dicTables.Add("T", firstTable);
                dicTables.Add("K", secondTable);
                dicTables.Add("M", thirdTable);
                dicTables.Add("N", fourTable);
                dicTables.Add("O", fiveTable);

                return dicTables;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static Dictionary<string, object> SaveObject<T, K, M, N, O, P>(object obj, IDbCommand command, Mapper mappingInfo, IDbDataAdapter adapter)
        {
            try
            {
                DataSet ds = new DataSet();
                DataTable dtFirst = CreateTable<T>();
                DataTable dtSecond = CreateTable<K>();
                DataTable dtThird = CreateTable<M>();
                DataTable dtFour = CreateTable<N>();
                DataTable dtFive = CreateTable<O>();
                DataTable dtSix = CreateTable<P>();

                Type type = obj.GetType();
                PropertyInfo[] properties = type.GetProperties();

                //foreach (PropertyInfo property in properties)
                //{
                //    // for each property declared in the type provided check if the property is
                //    // decorated with the DBField attribute
                //    if (Attribute.IsDefined(property, typeof(DBParameterAttribute)))
                //    {
                //        DBParameterAttribute attrib = (DBParameterAttribute)Attribute.GetCustomAttribute(property, typeof(DBParameterAttribute));
                //        IDataParameter param = (IDataParameter)command.Parameters[attrib.ParameterName]; // get parameter
                //        param.Value = property.GetValue(obj, null); // set parameter value
                //    }
                //}

                foreach (string map in mappingInfo.MappingInformation)
                {
                    // for each mapping information 
                    string property = Mapper.GetProperty(map);
                    string parameter = Mapper.GetParameter(map);

                    PropertyInfo propInfo = type.GetProperty(property); // ge the property by name
                    IDataParameter param = (IDataParameter)command.Parameters[parameter]; // get the parameter
                    param.Value = propInfo.GetValue(obj, null); // set paramter value
                }

                adapter.SelectCommand = command;
                adapter.Fill(ds);
                dtFirst = ds.Tables[0];
                dtSecond = ds.Tables[1];
                dtThird = ds.Tables[2];
                dtFour = ds.Tables[3];
                dtFive = ds.Tables[4];
                dtSix = ds.Tables[5];


                List<T> firstTable = FromDataTableToList<T>(dtFirst);
                List<K> secondTable = FromDataTableToList<K>(dtSecond);
                List<M> thirdTable = FromDataTableToList<M>(dtThird);
                List<N> fourTable = FromDataTableToList<N>(dtFour);
                List<O> fiveTable = FromDataTableToList<O>(dtFive);
                List<P> sixTable = FromDataTableToList<P>(dtSix);

                Dictionary<string, object> dicTables = new Dictionary<string, object>();
                dicTables.Add("T", firstTable);
                dicTables.Add("K", secondTable);
                dicTables.Add("M", thirdTable);
                dicTables.Add("N", fourTable);
                dicTables.Add("O", fiveTable);
                dicTables.Add("P", sixTable);

                return dicTables;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static Dictionary<string, object> SaveObject<T, K, M, N, O, P, Q, S>(object obj, IDbCommand command, Mapper mappingInfo, IDbDataAdapter adapter)
        {
            try
            {
                DataSet ds = new DataSet();
                DataTable dtFirst = CreateTable<T>();
                DataTable dtSecond = CreateTable<K>();
                DataTable dtThird = CreateTable<M>();
                DataTable dtFour = CreateTable<N>();
                DataTable dtFive = CreateTable<O>();
                DataTable dtSix = CreateTable<P>();
                DataTable dtSeven = CreateTable<Q>();
                DataTable dtEight = CreateTable<S>();

                Type type = obj.GetType();
                PropertyInfo[] properties = type.GetProperties();

                //foreach (PropertyInfo property in properties)
                //{
                //    // for each property declared in the type provided check if the property is
                //    // decorated with the DBField attribute
                //    if (Attribute.IsDefined(property, typeof(DBParameterAttribute)))
                //    {
                //        DBParameterAttribute attrib = (DBParameterAttribute)Attribute.GetCustomAttribute(property, typeof(DBParameterAttribute));
                //        IDataParameter param = (IDataParameter)command.Parameters[attrib.ParameterName]; // get parameter
                //        param.Value = property.GetValue(obj, null); // set parameter value
                //    }
                //}

                foreach (string map in mappingInfo.MappingInformation)
                {
                    // for each mapping information 
                    string property = Mapper.GetProperty(map);
                    string parameter = Mapper.GetParameter(map);

                    PropertyInfo propInfo = type.GetProperty(property); // ge the property by name
                    IDataParameter param = (IDataParameter)command.Parameters[parameter]; // get the parameter
                    param.Value = propInfo.GetValue(obj, null); // set paramter value
                }

                adapter.SelectCommand = command;
                adapter.Fill(ds);
                dtFirst = ds.Tables[0];
                dtSecond = ds.Tables[1];
                dtThird = ds.Tables[2];
                dtFour = ds.Tables[3];
                dtFive = ds.Tables[4];
                dtSix = ds.Tables[5];
                dtSeven = ds.Tables[6];
                dtEight = ds.Tables[7];


                List<T> firstTable = FromDataTableToList<T>(dtFirst);
                List<K> secondTable = FromDataTableToList<K>(dtSecond);
                List<M> thirdTable = FromDataTableToList<M>(dtThird);
                List<N> fourTable = FromDataTableToList<N>(dtFour);
                List<O> fiveTable = FromDataTableToList<O>(dtFive);
                List<P> sixTable = FromDataTableToList<P>(dtSix);
                List<Q> sevenTable = FromDataTableToList<Q>(dtSeven);
                List<S> eightTable = FromDataTableToList<S>(dtEight);

                Dictionary<string, object> dicTables = new Dictionary<string, object>();
                dicTables.Add("T", firstTable);
                dicTables.Add("K", secondTable);
                dicTables.Add("M", thirdTable);
                dicTables.Add("N", fourTable);
                dicTables.Add("O", fiveTable);
                dicTables.Add("P", sixTable);
                dicTables.Add("Q", sevenTable);
                dicTables.Add("S", eightTable);

                return dicTables;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<T> SaveObject<T>(object obj, IDbCommand command, Mapper mappingInfo, IDbDataAdapter adapter)
        {
            try
            {
                DataSet ds = new DataSet();
                DataTable dt = CreateTable<T>();


                Type type = obj.GetType();
                PropertyInfo[] properties = type.GetProperties();

                //foreach (PropertyInfo property in properties)
                //{
                //    // for each property declared in the type provided check if the property is
                //    // decorated with the DBField attribute
                //    if (Attribute.IsDefined(property, typeof(DBParameterAttribute)))
                //    {
                //        DBParameterAttribute attrib = (DBParameterAttribute)Attribute.GetCustomAttribute(property, typeof(DBParameterAttribute));
                //        IDataParameter param = (IDataParameter)command.Parameters[attrib.ParameterName]; // get parameter
                //        param.Value = property.GetValue(obj, null); // set parameter value
                //    }
                //}

                foreach (string map in mappingInfo.MappingInformation)
                {
                    // for each mapping information 
                    string property = Mapper.GetProperty(map);
                    string parameter = Mapper.GetParameter(map);

                    PropertyInfo propInfo = type.GetProperty(property); // ge the property by name
                    IDataParameter param = (IDataParameter)command.Parameters[parameter]; // get the parameter
                    param.Value = propInfo.GetValue(obj, null); // set paramter value
                }

                adapter.SelectCommand = command;
                adapter.Fill(ds);
                dt = ds.Tables[0];

                List<T> returnList = FromDataTableToList<T>(dt);

                return returnList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //Insert Will Return Two Tables

        public static Dictionary<string, object> SaveObject<T, K>(object obj, IDbCommand command, Mapper mappingInfo, IDbDataAdapter adapter)
        {
            try
            {
                DataSet ds = new DataSet();
                DataTable dtFirst = CreateTable<T>();
                DataTable dtSecond = CreateTable<K>();

                Type type = obj.GetType();
                PropertyInfo[] properties = type.GetProperties();

                //foreach (PropertyInfo property in properties)
                //{
                //    // for each property declared in the type provided check if the property is
                //    // decorated with the DBField attribute
                //    if (Attribute.IsDefined(property, typeof(DBParameterAttribute)))
                //    {
                //        DBParameterAttribute attrib = (DBParameterAttribute)Attribute.GetCustomAttribute(property, typeof(DBParameterAttribute));
                //        IDataParameter param = (IDataParameter)command.Parameters[attrib.ParameterName]; // get parameter
                //        param.Value = property.GetValue(obj, null); // set parameter value
                //    }
                //}

                foreach (string map in mappingInfo.MappingInformation)
                {
                    // for each mapping information 
                    string property = Mapper.GetProperty(map);
                    string parameter = Mapper.GetParameter(map);

                    PropertyInfo propInfo = type.GetProperty(property); // ge the property by name
                    IDataParameter param = (IDataParameter)command.Parameters[parameter]; // get the parameter
                    param.Value = propInfo.GetValue(obj, null); // set paramter value
                }

                adapter.SelectCommand = command;
                adapter.Fill(ds);
                dtFirst = ds.Tables[0];
                dtSecond = ds.Tables[1];

                Dictionary<string, object> dicObjectCollection = new Dictionary<string, object>();

                List<T> firstObject = FromDataTableToList<T>(dtFirst);
                List<K> secondObject = FromDataTableToList<K>(dtSecond);
                dicObjectCollection.Add("T", firstObject);
                dicObjectCollection.Add("K", secondObject);

                return dicObjectCollection;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static List<T> FromDataTableToList<T>(DataTable dataTable)
        {
            try
            {
                //This create a new list with the same type of the generic class
                List<T> genericList = new List<T>();
                //Obtains the type of the generic class
                Type t = typeof(T);

                //Obtains the properties definition of the generic class.
                //With this, we are going to know the property names of the class
                PropertyInfo[] pi = t.GetProperties();

                //For each row in the datatable

                foreach (DataRow row in dataTable.Rows)
                {
                    //Create a new instance of the generic class
                    object defaultInstance = Activator.CreateInstance(t);
                    //For each property in the properties of the class
                    foreach (PropertyInfo prop in pi)
                    {
                        try
                        {
                            //Get the value of the row according to the field name
                            //Remember that the classïs members and the tableïs field names
                            //must be identical

                            DBFieldAttribute attrib = (DBFieldAttribute)Attribute.GetCustomAttribute(prop, typeof(DBFieldAttribute));
                            if (attrib != null)
                            {
                                if (row.Table.Columns.Contains(attrib.FieldName))
                                {
                                    object columnvalue = row[attrib.FieldName];
                                    //Know check if the value is null. 
                                    //If not, it will be added to the instance
                                    if (columnvalue != DBNull.Value)
                                    {
                                        //Set the value dinamically. Now you need to pass as an argument
                                        //an instance class of the generic class. This instance has been
                                        //created with Activator.CreateInstance(t)
                                        //prop.SetValue(defaultInstance, columnvalue, null);

                                        if (prop.PropertyType == columnvalue.GetType()) // if the property and database field are the same
                                            prop.SetValue(defaultInstance, columnvalue, null); // set the value of property
                                        else
                                        {
                                            // change the type of the data in table to that of property and set the value of the property
                                            prop.SetValue(defaultInstance, Convert.ChangeType(columnvalue, prop.PropertyType), null);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            throw;
                            //Console.WriteLine(prop.Name + ": " + ex.ToString());
                            //return null;
                        }
                    }
                    //Now, create a class of the same type of the generic class. 
                    //Then a conversion itïs done to set the value
                    T myclass = (T)defaultInstance;
                    //Add the generic instance to the generic list
                    genericList.Add(myclass);
                }
                //At this moment, the generic list contains all de datatable values
                return genericList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static DataTable CreateTable<T>()
        {
            try
            {
                Type entType = typeof(T);
                DataTable tbl = new DataTable("Table_Data");
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(entType);
                foreach (PropertyDescriptor prop in properties)
                {
                    if (prop.PropertyType == typeof(Nullable<decimal>))
                        tbl.Columns.Add(prop.Name, typeof(decimal));
                    else if (prop.PropertyType == typeof(Nullable<int>))
                        tbl.Columns.Add(prop.Name, typeof(int));
                    else if (prop.PropertyType == typeof(Nullable<Int64>))
                        tbl.Columns.Add(prop.Name, typeof(Int64));
                    else
                        tbl.Columns.Add(prop.Name, prop.PropertyType);
                }
                return tbl;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
