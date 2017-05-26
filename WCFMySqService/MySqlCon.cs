using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace WCFMySqConnector
{

    public enum ASCD
    {
        ASC,
        DESC,
    }
    
    public class User
    {
        public int ID;
        public string email;
        public string password;
        public string firstName;
        public string lastName;
        public string phoneNumber;
        public bool active;
    }
    
  
 
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.
     [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
     ConcurrencyMode = ConcurrencyMode.Multiple,
     Name = "MySqlCon", Namespace = "WCFMySqConnector"
     )]
    public class MySqlCon : IMySqlDB
    {

        static string m_loginEmail = string.Empty;
        static bool m_loggedIn = false;

        static string myConnectionString;

        string m_serverIp;
        string m_userName;
        string m_password;

        public MySqlCon()
        {            
            SetServerInfo("localhost", "root", "1234");
             
        }
        static Object m_lock = new Object();

        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }
           
        void SetServerInfo(string serverIp, string userName, string password)
        {
            m_serverIp = serverIp;
            m_userName = userName;
            m_password = password;
            myConnectionString = string.Format("server={0};database=mydb;uid={1};pwd={2};", serverIp, userName, password);

        }

        Stream PrepareResponse(JObject jsonObject)
        {
            var s = JsonSerializer.Create();
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                s.Serialize(sw, jsonObject);
            }


            WebOperationContext.Current.OutgoingResponse.ContentType = "application/json; charset=utf-8";
            return new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
        }

        Stream PrepareResponseOk()
        {
            dynamic jsonObject = new JObject();
            jsonObject.Result = "ok";

            var s = JsonSerializer.Create();
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                s.Serialize(sw, jsonObject);
            }


            WebOperationContext.Current.OutgoingResponse.ContentType = "application/json; charset=utf-8";
            WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.OK;
            return new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
        }

        Stream PrepareResponseMsg(string msg, bool ok = false)
        {
            dynamic jsonObject = new JObject();
            jsonObject.Result = msg;

            var s = JsonSerializer.Create();
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                s.Serialize(sw, jsonObject);
            }


            WebOperationContext.Current.OutgoingResponse.ContentType = "application/json; charset=utf-8";
            if (ok == true)
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.OK;
            }
            else
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.InternalServerError;
            }
            return new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
        }

        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite == null)
            {
                throw new ArgumentNullException("composite");
            }
            if (composite.BoolValue)
            {
                composite.StringValue += "Suffix";
            }
            return composite;
        }
 
        bool IsValidUser(string email, string password)
        {
            lock (m_lock)
            {
                using (MySqlConnection cn = new MySqlConnection(myConnectionString))
                {

                    try
                    {

                        SHA256Managed crypt = new SHA256Managed();
                        string passwordhash = String.Empty;
                        byte[] crypto = crypt.ComputeHash(Encoding.ASCII.GetBytes(password), 0, Encoding.ASCII.GetByteCount(password));
                        foreach (byte theByte in crypto)
                        {
                            passwordhash += theByte.ToString("x2");
                        }

                        string query = "SELECT ID FROM users where email = @email and hashpassword = @password";

                        cn.Open();


                        int ID = -1;
                        using (MySqlCommand cmd = new MySqlCommand(query, cn))
                        {

                            cmd.Parameters.AddWithValue("@email", email);
                            cmd.Parameters.AddWithValue("@password", passwordhash);

                            MySqlDataReader dataReader = cmd.ExecuteReader();

                            //Read the data and store them in the list
                            while (dataReader.Read())
                            {
                                ID = int.Parse(dataReader["ID"].ToString());
                            }
                        }
                        cn.Close();
                        return ID == -1 ? false: true;
                    }
                    catch (MySqlException err)
                    {
                        throw (new SystemException(err.Message));
                    }
                }
            }
        }

        public Stream Login(string email, string password)
        {

            try
            {
                if (IsValidUser(email, password) == true)
                {
                    m_loginEmail = email;
                    m_loggedIn = true;
                }
                else
                {
                    m_loggedIn = false;
                }
                return PrepareResponseMsg(m_loggedIn.ToString(), true);
            }
            catch (Exception err)
            {
                return PrepareResponseMsg(err.Message);
            }
        }        
        
        public Stream GetAllUsers()
        {
            lock (m_lock)
            {
                using (MySqlConnection cn = new MySqlConnection(myConnectionString))
                {

                    try
                    {

                        string query = "SELECT * FROM users";
                        cn.Open();
                        List<User> U = new List<User>();
                        using (MySqlCommand cmd = new MySqlCommand(query, cn))
                        {

                            MySqlDataReader dataReader = cmd.ExecuteReader();

                            while (dataReader.Read())
                            {
                                User u = new User();
                                u.ID = int.Parse(dataReader["ID"].ToString());
                                u.email = dataReader["email"].ToString();
                                u.firstName = dataReader["firstname"].ToString();
                                u.lastName = dataReader["lastname"].ToString();
                                u.phoneNumber = dataReader["phonenumber"].ToString();
                                if (int.Parse(dataReader["active"].ToString()) == 1)
                                    u.active = true;
                                else
                                    u.active = false;
                                U.Add(u);
                            }
                        }
                        cn.Close();
                        string json = JsonConvert.SerializeObject(U);
                        return PrepareResponseMsg(json, true);
                    }
                    catch (MySqlException err)
                    {
                        throw (new SystemException(err.Message));
                    }
                }
            }
        }
          
       
        public Stream SaveUserChanges(int ID,
                               string email,
                               string password,
                               string firstName,
                               string lastName,
                               string phoneNumber,
                               bool active)
        {
            lock (m_lock)
            {
                using (MySqlConnection cn = new MySqlConnection(myConnectionString))
                {
                    // Here we have to create a "try - catch" block, this makes sure your app
                    // catches a MySqlException if the connection can't be opened, 
                    // or if any other error occurs.


                    SHA256Managed crypt = new SHA256Managed();
                    string passwordhash = String.Empty;
                    byte[] crypto = crypt.ComputeHash(Encoding.ASCII.GetBytes(password), 0, Encoding.ASCII.GetByteCount(password));
                    foreach (byte theByte in crypto)
                    {
                        passwordhash += theByte.ToString("x2");
                    }

                    try
                    {
                        string query = @"UPDATE users SET email = @email ,
                                       firstname = @firstname , lastname = @lastname ,phonenumber = @phone , 
                                        active = @active    where id = @id";
                        cn.Open();

                        using (MySqlCommand cmd = new MySqlCommand(query, cn))
                        {
                            // Now we can start using the passed values in our parameters:
                            cmd.Parameters.AddWithValue("@id", ID);
                            cmd.Parameters.AddWithValue("@email", email);
                            cmd.Parameters.AddWithValue("@firstname", firstName);
                            cmd.Parameters.AddWithValue("@lastname", lastName);
                            cmd.Parameters.AddWithValue("@phone", phoneNumber);
                            cmd.Parameters.AddWithValue("@active", active);

                            // Execute the query
                            cmd.ExecuteNonQuery();
                        }
                        cn.Close();
                        return PrepareResponseOk();
                    }
                    catch (MySqlException err)
                    {
                        return PrepareResponseMsg(err.Message);
                    }
                }
            }
        }

        public Stream SuspendUser(int id, bool suspend)
        {
            lock (m_lock)
            {
                using (MySqlConnection cn = new MySqlConnection(myConnectionString))
                {

                    try
                    {
                        string query = "UPDATE users SET active = @active where id = @id";

                        cn.Open();

                        using (MySqlCommand cmd = new MySqlCommand(query, cn))
                        {
                            cmd.Parameters.AddWithValue("@id", id);
                            int active = suspend == true ? 0 : 1;
                            cmd.Parameters.AddWithValue("@active", active);
                            cmd.ExecuteNonQuery();
                        }
                        cn.Close();
                        return PrepareResponseOk();
                    }
                    catch (MySqlException err)
                    {
                        return PrepareResponseMsg(err.Message);
                    }
                }
            }
        }

        public Stream CreateNewUser(int ID,
                               string email,
                               string password,
                               string firstName,
                               string lastName,
                               string phoneNumber,
                               bool active)

        {
            lock (m_lock)
            {
                using (MySqlConnection cn = new MySqlConnection(myConnectionString))
                {
                    // Here we have to create a "try - catch" block, this makes sure your app
                    // catches a MySqlException if the connection can't be opened, 
                    // or if any other error occurs.


                    SHA256Managed crypt = new SHA256Managed();
                    string passwordhash = String.Empty;
                    byte[] crypto = crypt.ComputeHash(Encoding.ASCII.GetBytes(password), 0, Encoding.ASCII.GetByteCount(password));
                    foreach (byte theByte in crypto)
                    {
                        passwordhash += theByte.ToString("x2");
                    }

                    try
                    {
                        string query = @"INSERT INTO users (email, hashpassword, firstname, lastname, phonenumber, active)
                        VALUES (@email, @hashpassword,@firstname, @lastname, @phone, @active);";

                        cn.Open();

                        // Yet again, we are creating a new object that implements the IDisposable
                        // interface. So we create a new using statement.

                        using (MySqlCommand cmd = new MySqlCommand(query, cn))
                        {
                            cmd.Parameters.AddWithValue("@email", email);
                            cmd.Parameters.AddWithValue("@hashpassword", passwordhash);
                            cmd.Parameters.AddWithValue("@firstname", firstName);
                            cmd.Parameters.AddWithValue("@lastname", lastName);
                            cmd.Parameters.AddWithValue("@phone", phoneNumber);
                            cmd.Parameters.AddWithValue("@active", active);
                            cmd.ExecuteNonQuery();
                        }
                        cn.Close();
                        return PrepareResponseOk();
                    }
                    catch (MySqlException err)
                    {
                        return PrepareResponseMsg(err.Message);
                    }
                }
            }
        }

        
        
        public Stream CheckAuthtintication(string userName, string password)
        {

            lock (m_lock)
            {
                using (MySqlConnection cn = new MySqlConnection(myConnectionString))
                {
                    try
                    {

                        SHA256Managed crypt = new SHA256Managed();
                        string passwordhash = String.Empty;
                        byte[] crypto = crypt.ComputeHash(Encoding.ASCII.GetBytes(password), 0, Encoding.ASCII.GetByteCount(password));
                        foreach (byte theByte in crypto)
                        {
                            passwordhash += theByte.ToString("x2");
                        }

                        string query = "SELECT ID FROM users where email = @email and hashpassword = @password";

                        cn.Open();


                        int ID = -1;
                        using (MySqlCommand cmd = new MySqlCommand(query, cn))
                        {

                            cmd.Parameters.AddWithValue("@email", userName);
                            cmd.Parameters.AddWithValue("@password", passwordhash);

                            MySqlDataReader dataReader = cmd.ExecuteReader();

                            //Read the data and store them in the list
                            while (dataReader.Read())
                            {
                                ID = int.Parse(dataReader["ID"].ToString());
                            }
                        }
                        cn.Close();
                        return PrepareResponseMsg(ID.ToString(), true);
                    }
                    catch (MySqlException err)
                    {
                        return PrepareResponseMsg(err.Message);
                    }
                }
            }
        }

        public Stream DeleteUser(int id)
        {
            lock (m_lock)
            {
                using (MySqlConnection cn = new MySqlConnection(myConnectionString))
                {
                    try
                    {
                        string query = "DELETE from users  where id = " + id;
                        cn.Open();

                        using (MySqlCommand cmd = new MySqlCommand(query, cn))
                        {
                            cmd.Parameters.AddWithValue("@id", id);
                            cmd.ExecuteNonQuery();
                        }
                        cn.Close();
                        return PrepareResponseOk();
                    }
                    catch (MySqlException err)
                    {
                        return PrepareResponseMsg(err.Message);
                    }
                }
            }
        }
          
    
        public Stream GetUserID(string email)
        {
            lock (m_lock)
            {
                using (MySqlConnection cn = new MySqlConnection(myConnectionString))
                {
                    try
                    {
                        string query = "SELECT ID FROM users where email= @email";

                        cn.Open();

                        int ID = -1;
                        using (MySqlCommand cmd = new MySqlCommand(query, cn))
                        {

                            cmd.Parameters.AddWithValue("@email", email);

                            MySqlDataReader dataReader = cmd.ExecuteReader();

                            //Read the data and store them in the list
                            while (dataReader.Read())
                            {
                                ID = int.Parse(dataReader["ID"].ToString());
                            }
                        }                        
                        cn.Close();

                        dynamic jsonObject = new JObject();
                        jsonObject.ID = ID;
                        return PrepareResponse(jsonObject);

                    }
                    catch (MySqlException err)
                    {
                        return PrepareResponseMsg(err.Message);
                    }
                }
            }
        }
    }
}
