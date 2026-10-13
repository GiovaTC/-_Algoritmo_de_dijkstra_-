using System;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;

namespace DijkstraOracle19c
{
    internal class Program
    {
        // actualiza estos datos segun tu instalacion de ORACLE 19c.
        private const string ConnectionString =
            "User Id=system;" + 
            "Password=Tapiero123;" +
            "Data Source=localhost:1521/orcl;";

        private sealed class Arista
        {
            public string Destino { get; }
            public decimal Peso { get; }

            public Arista(string destino, decimal peso)
            {
                Destino = destino;
                Peso = peso;
            }
        }

        private sealed class ResultadoRuta
        {
            public bool Encontrada { get; init; }
            public decimal Distancia { get; init; }
            public List<string> Ruta { get; init; } = new();
        }

        static void Main()
        {
            Console.OutputEncoding =
                System.Text.Encoding.UTF8;

            Console.WriteLine("==================================");
            Console.WriteLine(" ALGORITMO DE DIJKSTRA");
            Console.WriteLine(" C# + ORACLE DATABASE 19c");
            Console.WriteLine("==================================");

            try
            {
                using var conexion =
                    new OracleConnection(ConnectionString);
                conexion.Open();

                Console.WriteLine(
                    "\nConexion a oracle 19c exitosa.");

                Dictionary<string, string> nodos = CargarNodos(conexion);

                if (nodos.Count == 0)
                {
                    Console.WriteLine(
                        "No hay nodos registrados en la tabla NODOS.");
                    return;
                }

                Dictionary<string, List<Arista>> grafo = CargarGrafo(conexion, nodos.Keys);

                Console.WriteLine("\nNodos disponibles:");

                foreach (var nodo in nodos)
                {
                    Console.WriteLine(
                        $"{nodo.Key} - {nodo.Value}");
                }

                Console.Write("\nIngrese el nodo de origen: ");
                string origen = LeerNodo();

                Console.Write("Ingrese el nodo de destino: ");
                string destino = LeerNodo();

                if (!nodos.ContainsKey(origen) || !nodos.ContainsKey(destino))
                {
                    Console.WriteLine(
                        "\nError: el origen o destino no existe!");
                    return;
                }

                ResultadoRuta resultado =
                    CalcularDijkstra(grafo, origen, destino);

                Console.WriteLine("\n========== RESULTADO ==========");

                if (resultado.Encontrada)
                {
                    Console.WriteLine(
                        $"Ruta mas corta: " +
                        string.Join(" -> ", resultado.Ruta));

                    Console.WriteLine(
                        $"Distancia total: {resultado.Distancia}");
                }
                else
                {
                    Console.WriteLine(
                        "No existe una ruta entre los nodos.");
                }

                GuardarHistorial(conexion, origen, destino, resultado);

                Console.WriteLine(
                "Consulta registrada en HISTORIAL_RUTAS.");

                Console.WriteLine("===============================");
            }
            catch (OracleException ex)
            {
                Console.WriteLine(
                    "\nError de oracle: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "\nError: " + ex.Message);
            }

            Console.WriteLine("\nPresione una tecla para salir.");
            Console.ReadKey();
        }

        private static string LeerNodo()
        {
            return  (Console.ReadLine() ?? "")
                .Trim()
                .ToUpperInvariant();
        }

        // CONSULTA los nodos almacenados en Oracle .
        private static Dictionary<string, string> CargarNodos(OracleConnection conexion)
        {
            var nodos = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);  

            const string sql = "SELECT ID,NODO, NOMBRE FROM NODOS";

            using var comando = new OracleCommand(sql, conexion);

            using var lector = comando.ExecuteReader();

            while (lector.Read())
            {
                string id = lector.GetString(0);
                string nombre = lector.GetString(2);
                nodos[id] = nombre;
            }

            return nodos;
        }

        // CARGA las aristas y crea un grafo bidireccional .
        private static Dictionary<string, List<Arista>> 
            CargarGrafo(
            OracleConnection conexion,
            IEnumerable<string> idsNodos)
        {
            var grafo = new Dictionary<string, List<Arista>>(
                StringComparer.OrdinalIgnoreCase);

            foreach (string id in idsNodos)
            {
                grafo[id] = new List<Arista>();
            }

            const string sql =
                @"SELECT NODO_ORIGEN, NODO_DESTINO, PESO 
                FROM ARISTAS";

            using var comando = 
                new OracleCommand(sql, conexion);

            using var lector = comando.ExecuteReader();
            
            while (lector.Read())
            {
                string origen = lector.GetString(0);
                string destino = lector.GetString(1);
                decimal peso = lector.GetDecimal(2);

                if (peso < 0)
                {
                    throw new InvalidOperationException(
                        "Dijkstra no admite pesos negativos. ");
                }

                grafo[origen].Add(new Arista(destino, peso));

                grafo[destino].Add(new Arista(origen, peso));
            }

            return grafo;
        }

        private static ResultadoRuta CalcularDijkstra(Dictionary<string, List<Arista>> grafo, string origen, string destino)
        {
            throw new NotImplementedException();
        }

        private static void GuardarHistorial(OracleConnection conexion, string origen, string destino, ResultadoRuta resultado)
        {
            throw new NotImplementedException();
        }
 
    }
}
