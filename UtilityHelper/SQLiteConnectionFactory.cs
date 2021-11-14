using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite;

namespace Utility
{
    public class SqLiteConnectionFactory
    {
        public const string DefaultDatabaseDirectory = "../../../Data";
        public const string SqLiteExtension = "sqlite";

        public static SQLiteConnection Create<T>(string? path = default, Func<Type, bool>? typePredicate = default)
        {
            return Create(string.IsNullOrEmpty(path) ? $"{System.IO.Directory.CreateDirectory(DefaultDatabaseDirectory).FullName}{typeof(T).Name}.{SqLiteExtension}" : path, GetTypes());

            Type[] GetTypes() =>
                typeof(T).Assembly.GetTypes()
                    .Where(typePredicate ?? (type => type.GetMethods().Any() == false || type == typeof(T)))
                    .ToArray();
        }

        public static SQLiteConnection Create(string? path = null, params Type[] types)
        {
            if (!(System.IO.Path.GetDirectoryName(path) is { } name))
                throw new Exception($"Unable to retrieve name of directory from {nameof(path)}");

            System.IO.Directory.CreateDirectory(name);

            SQLiteConnection conn = new SQLiteConnection(path);

            foreach (var type in types)
            {
                conn.CreateTable(type, CreateFlags.AutoIncPK);
            }

            return conn;

        }
    }
}
