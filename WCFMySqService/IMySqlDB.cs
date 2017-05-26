using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace WCFMySqConnector
{
    [ServiceContract]
    public interface IMySqlDB
    {
        
        [OperationContract]
        [WebGet(), CorsEnabled]
        Stream GetAllUsers();

    
        [OperationContract]
        [WebGet]
        Stream CheckAuthtintication(string userName, string password);

       
        [OperationContract]
        [WebGet(), CorsEnabled]
        Stream GetUserID(string email);

        [OperationContract]
        [WebGet(), CorsEnabled]
        Stream Login(string username, string password);

         
      
        [OperationContract]
        [WebInvoke(Method = "POST",
            UriTemplate = @"SuspendUser?id={id}&suspend={suspend}"
            ,
            BodyStyle = WebMessageBodyStyle.Bare), CorsEnabled]
        Stream SuspendUser(int id, bool suspend);


        [OperationContract]
        [WebInvoke(Method = "POST",
            UriTemplate = @"SaveUserChanges?ID={ID}&email={email}&password={password}&firstName={firstName}&lastName={lastName}&phoneNumber={phoneNumber}&active={active}"
            ,
            BodyStyle = WebMessageBodyStyle.Bare), CorsEnabled]
       Stream SaveUserChanges(int ID,
                               string email,
                               string password,
                               string firstName,
                               string lastName,
                               string phoneNumber,
                               bool active);


        [OperationContract]
        [WebInvoke(Method = "POST",
            UriTemplate = @"CreateNewUser?ID={ID}&email={email}&password={password}&firstName={firstName}&lastName={lastName}&phoneNumber={phoneNumber}&active={active}"
            ,
            BodyStyle = WebMessageBodyStyle.Bare), CorsEnabled]
        Stream CreateNewUser(int ID,
                             string email,
                             string password,
                             string firstName,
                             string lastName,
                             string phoneNumber,
                             bool active);


    }

    // Use a data contract as illustrated in the sample below to add composite types to service operations.
    // You can add XSD files into the project. After building the project, you can directly use the data types defined there, with the namespace "WCFMySqConnector.ContractType".
    [DataContract]
    public class CompositeType
    {
        bool boolValue = true;
        string stringValue = "Hello ";

        [DataMember]
        public bool BoolValue
        {
            get { return boolValue; }
            set { boolValue = value; }
        }

        [DataMember]
        public string StringValue
        {
            get { return stringValue; }
            set { stringValue = value; }
        }
    }
}
