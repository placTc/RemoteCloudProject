using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonClasses;

namespace RemoteCloudClient
{
    class RequestSerializer
    {
        public static string SerializeLoginRequest(User user)
        {
            string request = "200;";
            string data = "username:" + user.getName() + ",password:" + user.getPassword();
            request = request + data.Length.ToString() + ';' + data;
            return request;
        }

        public static string SerializeSignupRequest(User user)
        {
            string request = "202;";
            string data = "username:" + user.getName() + ",password:" + user.getPassword() + ",email:" + user.getEmail();
            request = request + data.Length.ToString() + ';' + data;
            return request;
        }

        public static string SerializeDirectoryRelatedRequest(string directory, User user, string requestType)
        {
            string request = requestType + ";";
            string data = user.getName() + ";" + @"\" + user.getName() + directory;
            request = request + data.Length.ToString() + ";" + data;
            return request;
        }

        public static string SerializeLogoutRequest(string name)
        {
            string request = "201;";
            request = request + name.Length.ToString() + ';' + name;
            return request;
        }

        public static string SerializeFileRequest(string filepath, User user, string requestType, string dataToSend = "")
        {
            string request = requestType + ";";
            string data = user.getName() + ";" + @"\" + user.getName() + filepath;
            if (dataToSend != "") data += ";" + dataToSend;
            request = request + data.Length.ToString() + ';' + data;
            return request;
        }
    }
}
