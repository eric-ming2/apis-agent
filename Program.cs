// See https://aka.ms/new-console-template for more information
using FirebirdSql.Data.FirebirdClient;
using System;
using System.IO;
using System.Text;
using System.Linq;


namespace Probe
{
	class Program
	{
		private const string prodDbPath = @"C:\Microsip datos\ERIC.FDB";
		private const string workingDir = @"C:\apis";
		private static string fbConnectionString = string.Format("database=localhost:{0};user=sysdba;password=masterkey", prodDbPath);

		private static void createBackupAndShadowDBs()
		{
			try
			{
				Directory.CreateDirectory(workingDir);
				File.Copy(prodDbPath, Path.Combine(workingDir, "BACKUP.FDB"), true);
				File.Copy(prodDbPath, Path.Combine(workingDir, "SHADOW.FDB"), true);
			}
			catch (IOException iox)
			{
				Console.WriteLine(iox.Message);
			}

		}
		static void Main(string[] args)
		{
			createBackupAndShadowDBs();
			using (var connection = new FbConnection(fbConnectionString))
			{
				connection.Open();
				using (var events = new FbRemoteEvent(fbConnectionString))
				{
					List<string> eventList = new List<string>();
					eventList.Add("ARTICULOS_ADD");
					events.RemoteEventCounts += (sender, e) => Console.WriteLine($"Event: {e.Name} | Counts: {e.Counts}");
					events.RemoteEventError += (sender, e) => Console.WriteLine($"ERROR: {e.Error}");
					events.Open();
					events.QueueEvents(eventList);
					Console.WriteLine("Listening...");
					Console.ReadLine();
				}
				// using (var transaction = connection.BeginTransaction())
				// {
				// 	using (var command = new FbCommand("select * from articulos", connection, transaction))
				// 	{
				// 		using (var reader = command.ExecuteReader())
				// 		{
				// 			while (reader.Read())
				// 			{
				// 				var values = new object[reader.FieldCount];
				// 				reader.GetValues(values);
				// 				Console.WriteLine(string.Join("|", values));
				// 			}
				// 		}
				// 	}
				// }

			}
		}
	}
}