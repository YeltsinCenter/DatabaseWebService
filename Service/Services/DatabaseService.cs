using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DatabaseWebService.Services
{
    public class DatabaseService
    {
        private static Dictionary<string, ConnectionInfo> _connections = new Dictionary<string, ConnectionInfo>();

        private class ConnectionInfo
        {
            public SqlConnection Connection { get; set; }
            public DateTime LastUsed { get; set; }
        }

        public string Connect(string connectionString, out string sessionId)
        {
            sessionId = null;

            try
            {
                sessionId = Guid.NewGuid().ToString();

                var connection = new SqlConnection(connectionString);
                connection.Open();

                _connections[sessionId] = new ConnectionInfo
                {
                    Connection = connection,
                    LastUsed = DateTime.Now
                };

                return $"Подключение успешно установлено. SessionId: {sessionId}";
            }
            catch (SqlException ex)
            {
                return $"Ошибка SQL Server: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"Ошибка: {ex.Message}";
            }
        }

        public string GetVersion(string sessionId)
        {
            try
            {
                if (!_connections.ContainsKey(sessionId))
                {
                    return "Сессия не найдена. Сначала вызовите Connect";
                }

                var connectionInfo = _connections[sessionId];

                if (connectionInfo.Connection.State != System.Data.ConnectionState.Open)
                {
                    return "Подключение закрыто. Вызовите Connect заново";
                }

                connectionInfo.LastUsed = DateTime.Now;

                SqlCommand command = new SqlCommand("SELECT @@VERSION", connectionInfo.Connection);
                string version = command.ExecuteScalar().ToString();

                return version;
            }
            catch (Exception ex)
            {
                return $"Ошибка при получении версии: {ex.Message}";
            }
        }

        public string Disconnect(string sessionId)
        {
            try
            {
                if (_connections.ContainsKey(sessionId))
                {
                    var connectionInfo = _connections[sessionId];
                    connectionInfo.Connection.Close();
                    connectionInfo.Connection.Dispose();
                    _connections.Remove(sessionId);

                    return "Подключение закрыто";
                }

                return "Сессия не найдена";
            }
            catch (Exception ex)
            {
                return $"Ошибка при закрытии: {ex.Message}";
            }
        }

        public static void CleanupOldConnections()
        {
            var oldConnections = new List<string>();
            var timeout = TimeSpan.FromMinutes(5);

            foreach (var kvp in _connections)
            {
                if (DateTime.Now - kvp.Value.LastUsed > timeout)
                {
                    oldConnections.Add(kvp.Key);
                }
            }

            foreach (var sessionId in oldConnections)
            {
                try
                {
                    _connections[sessionId].Connection.Close();
                    _connections.Remove(sessionId);
                }
                catch { }
            }
        }
    }
}