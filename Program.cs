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
        }
    }
}
