using System;

namespace RemoteCloudProject
{
	public class User
	{
		private string _username;
		private string _password;
		private string _email;

		public User()
		{
			_username = "";
			_password = "";
			_email = "";
		}
		public User(string username, string password, string email)
        {
			_username = username;
			_password = password;
			_email = email;
        }
		public string getName()
        {
			return _username;
        }
		public string getPassword()
        {
			return _password;
        }
		public string getEmail()
        {
			return _email;
        }
	}
}