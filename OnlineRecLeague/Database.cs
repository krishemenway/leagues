﻿using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace OnlineRecLeague
{
	public static class Database
	{
		public static IDbConnection CreateConnection()
		{
			var connection = new NpgsqlConnection(ConnectionString.Value);
			connection.Open();
			return connection;
		}

		private static string CreateConnectionString()
		{
			var connectionParams = new Dictionary<string, string>
				{
					{ "Host", Program.Settings.DatabaseHost },
					{ "Port", Program.Settings.DatabasePort.ToString() },
					{ "Username", Program.Settings.DatabaseUser },
					{ "Password", Program.Settings.DatabasePassword },
					{ "Database", Program.Settings.DatabaseName },
				};

			return string.Join(";", connectionParams.Select(param => $"{param.Key}={param.Value}"));
		}

		private static Lazy<string> ConnectionString = new Lazy<string>(() => CreateConnectionString());
	}
}
