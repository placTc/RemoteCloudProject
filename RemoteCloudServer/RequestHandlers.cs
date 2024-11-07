using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Security.Cryptography;
using CommonClasses;

namespace RemoteCloudServer
{
    internal class RequestHandlers
    {
        // received success error | [in progress] [accepted] [finished] [accepted finish]
        public static string HandleLoginRequest(string data, ref List<User> userList) // 200 1200 2200
        {
            XmlSerializer xml = new XmlSerializer(typeof(List<User>));
            FileStream fs;
            List<User> signedUpUsers;

            fs = new FileStream(@"data\users.xml", FileMode.Open, FileAccess.Read);
            signedUpUsers = (List<User>)xml.Deserialize(fs);
            fs.Close();

            string[] data2 = data.Split(','); // second stage of splitting data
            string[][] data3 = new string[2][];
            for (int i = 0; i < data2.Length; i++)  // third stage of splitting data
            {
                data3[i] = data2[i].Split(':');
            }


            User info = new User(data3[0][1], data3[1][1], ""); // username, password
            bool flag = false;

            foreach (User user in signedUpUsers)
            {
                if(info.getName() == user.getName() && info.getPassword() == user.getPassword())
                {
                    flag = true;
                    info._email = user.getEmail();
                    break;
                }
            }
            if(flag) flag = !UserLoggedIn(info.getName(), ref userList);

            if (flag)
            {
                userList.Add(info);
                return "1200";
            }
            else
                return "2200";
        }
        public static string HandleLogoutRequest(string data, ref List<User> userList) // 201 1201 2201
        {   
            foreach(User user in userList)
            {
                if(user.getName() == data)
                {
                    userList.Remove(user);
                    return "1201";
                }
            }
            return "2201";
        }
        public static string HandleSignupRequest(string data) // 202 1202 2202
        {
            XmlSerializer xml = new XmlSerializer(typeof(List<User>));
            FileStream fs;
            List<User> userList;
            try
            {
                fs = new FileStream(@"data\users.xml", FileMode.Open, FileAccess.Read);
                userList = (List<User>)xml.Deserialize(fs);
                fs.Close();
                fs = new FileStream(@"data\users.xml", FileMode.Create, FileAccess.Write);
            }
            catch (Exception e)
            {
                userList = new List<User>();
                fs = new FileStream(@"data\users.xml", FileMode.Create, FileAccess.Write); // reopening file for re-writing
            }

            string[] data2 = data.Split(','); // second stage of splitting data
            string[][] data3 = new string[3][];
            for(int i = 0; i < data2.Length; i++)  // third stage of splitting data
            {
                data3[i] = data2[i].Split(':');
            }


            User info = new User(data3[0][1], data3[1][1], data3[2][1]); // username, password, email

            foreach(User user in userList)
            {
                if (info.getName() == user.getName() || info.getEmail() == user.getEmail())
                {
                    xml.Serialize(fs, userList); // rewriting the xml
                    fs.Close();
                    return "2202";
                }
            }

            userList.Add(info);
            Directory.CreateDirectory(@"data\" + info.getName());
            xml.Serialize(fs, userList); // rewriting the xml
            fs.Close();

            return "1202";
        }
        public static string HandleFileUploadRequest(string data, ref List<User> userList, string start) // 300 1300 2300 | [3300] [4300] [5300] [6300]
        {
            if (start == "300")
            {
                try
                {
                    string[] data2 = data.Split(";", 2);
                    if (UserLoggedIn(data2[0], ref userList))
                    {
                        File.Create(@"data" + data2[1] + ".b64").Close();
                        return "1300";
                    }
                    else return "2300";
                }
                catch (Exception ex) { return "2300"; }
            }
            else if (start == "3300")
            {
                try
                {
                    string[] data2 = data.Split(";", 3);
                    if (UserLoggedIn(data2[0], ref userList))
                    {
                        File.AppendAllText(@"data" + data2[1] + ".b64", data2[2]);
                        return "4300";
                    }
                    else return "2300";
                }
                catch(Exception ex) { return "2300"; }
            }
            else if (start == "5300")
            {
                try
                {
                    string[] data2 = data.Split(";", 2);
                    ConvertFileFromB64(@"data" + data2[1] + ".b64", @"data" + data2[1]);
                    File.Delete(@"data" + data2[1] + ".b64");
                    return "6300";
                }
                catch (Exception ex) { return "2300"; }
            }
            return "2300";
        }
        public static string HandleFileDownloadRequest(string data, ref List<User> userList, string start) // 301 1301 2301 | [3301] [4301] [5301] [6301]
        {
            try
            {
                if (start == "301")
                {
                    string[] data2 = data.Split(";", 2);
                    if (UserLoggedIn(data2[0], ref userList))
                    {
                        ConvertFileToB64(@"data" + data2[1], @"data" + data2[1] + ".b64");
                        long size = new FileInfo(@"data" + data2[1] + ".b64").Length / 3000;
                        return "1301;" + size.ToString().Length + ";" + size.ToString();
                    }
                    else return "2301";
                }

                if (start == "3301")
                {
                    string[] data2 = data.Split(";", 3);
                    FileStream fs = File.OpenRead(@"data" + data2[1] + ".b64");
                    byte[] temp = new byte[3000];
                    fs.Position = int.Parse(data2[2]) * 3000;
                    fs.Read(temp, 0, 3000);
                    temp = TrimEnd(temp);
                    fs.Close();
                    return "4301;" + Encoding.UTF8.GetString(temp).Length + ";" + Encoding.UTF8.GetString(temp);
                }

                if(start == "5301")
                {
                    string[] data2 = data.Split(";", 2);
                    File.Delete(@"data" + data2[1] + ".b64");
                    return "6301";
                }
            }
            catch(Exception e) { }

            return "2301";
        }
        public static string HandleDeleteFileRequest(string data, ref List<User> userList) // 302 1302 2302
        {
            try
            {
                string[] secondaryData = data.Split(';', 2);
                if (UserLoggedIn(secondaryData[0], ref userList))
                {
                    File.Delete(@"data" + secondaryData[1]);
                    return "1302";
                }
                else return "2302";
            }
            catch (Exception ex)
            {
                return "2302";
            }
        }
        public static string HandleMakeDirectoryRequest(string data, ref List<User> userList) // 310 1310 2310
        {
            try
            {
                string[] secondaryData = data.Split(';', 2);
                if (UserLoggedIn(secondaryData[0], ref userList))
                {
                    Directory.CreateDirectory(@"data" + secondaryData[1]);
                    return "1310";
                }
                else return "2310";
            }
            catch (Exception ex)
            {
                return "2310";
            }
        }
        public static string HandleDeleteDirecotryRequest(string data, ref List<User> userList)  // 311 1311 2311
        {
            try
            {
                string[] secondaryData = data.Split(';', 2);
                if (UserLoggedIn(secondaryData[0], ref userList))
                {
                    Directory.Delete(@"data" + secondaryData[1], true);
                    return "1311";
                }
                else return "2311";
            }
            catch (Exception ex)
            {
                return "2311";
            }
        }
        public static string HandleGenericRequest(string data) // UNKNOWN/ERROR 2900
        {
            return "";
        }
        public static string HandleUpdateRequest(string data, ref List<User> userList) // 100 1100 2100
        {
            try
            {
                string[] secondaryData = data.Split(';', 2);
                if (UserLoggedIn(secondaryData[0], ref userList))
                {
                    string[] directories = Directory.GetDirectories(@"data" + secondaryData[1]);
                    for(int i = 0; i < directories.Length; i++)
                    {
                        directories[i] = directories[i].Split(@"\").Last();
                    }
                    string[] files = Directory.GetFiles(@"data" + secondaryData[1]);
                    for (int i = 0; i < files.Length; i++)
                    {
                        files[i] = files[i].Split(@"\").Last();
                    }
                    string response = "1100;";
                    string secondPart = "";
                    secondPart += string.Join(",", directories);
                    secondPart += ";";
                    secondPart += string.Join(",", files);
                    response += secondPart.Length.ToString() + ";";
                    response += secondPart;

                    return response;
                }
                else return "2100";
            }
            catch (Exception ex)
            {
                return "2100";
            }
        }
        private static bool UserLoggedIn(string name, ref List<User> userList)
        {
            foreach (User user in userList)
            {
                if (name == user.getName())
                {
                    return true;
                }
            }
            return false;
        }

        private static void ConvertFileToB64(string filein, string fileout)
        {
            //encode 
            using (FileStream fs = File.Open(fileout, FileMode.Create))
            using (var cs = new CryptoStream(fs, new ToBase64Transform(),
                                                         CryptoStreamMode.Write))

            using (var fi = File.Open(filein, FileMode.Open))
            {
                fi.CopyTo(cs);
            }
        }

        private static void ConvertFileFromB64(string filein, string fileout)
        {  
            // and decode
            using (FileStream f64 = File.Open(filein, FileMode.Open))
            using (var cs = new CryptoStream(f64, new FromBase64Transform(),
                                                        CryptoStreamMode.Read))
            using (var fo = File.Open(fileout, FileMode.Create))
            {
                cs.CopyTo(fo);
            }
        }
        public static byte[] TrimEnd(byte[] array)
        {
            int lastIndex = Array.FindLastIndex(array, b => b != 0);

            Array.Resize(ref array, lastIndex + 1);

            return array;
        }

    }
}
