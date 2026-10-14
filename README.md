# -_Algoritmo_de_dijkstra_- :.
# Algoritmo de Dijkstra en C# con Visual Studio 2022 y Oracle 19c:

<img width="1254" height="1254" alt="image" src="https://github.com/user-attachments/assets/51b9a7e1-d13f-4627-a0b1-7f358853bafd" />  

<img width="2551" height="1079" alt="image" src="https://github.com/user-attachments/assets/3a1a9f50-cb69-460d-a741-a5c2cd4c2ac3" />    
```

Proyecto de consola en C# que implementa el algoritmo de Dijkstra para encontrar la ruta de menor costo entre dos nodos de un grafo. Los nodos, las conexiones y el historial de rutas se almacenan en Oracle Database 19c.

## 1. Características

- C# en Visual Studio 2022.
- .NET 8.0 o una versión compatible instalada.
- Conexión a Oracle 19c mediante `Oracle.ManagedDataAccess.Core`.
- Lectura de nodos y aristas desde Oracle.
- Algoritmo de Dijkstra con cola de prioridad.
- Conexiones bidireccionales.
- Registro de rutas encontradas y no encontradas.
- Uso de parámetros SQL para registrar el historial.

> Dijkstra requiere que todos los pesos sean no negativos. Este ejemplo modela las conexiones como bidireccionales.

## 2. Estructura del proyecto

```text
DijkstraOracle19c/
├── DijkstraOracle19c.csproj
├── Program.cs
└── 01_crear_base_datos.sql
```

## 3. Crear las tablas y los datos de prueba en Oracle 19c

Guarda el siguiente script como `01_crear_base_datos.sql` y ejecútalo en SQL Developer o SQL*Plus con el usuario que utilizará la aplicación.

```sql
-- =====================================================
-- PROYECTO: ALGORITMO DE DIJKSTRA
-- BASE DE DATOS: ORACLE 19c
-- =====================================================

CREATE TABLE NODOS (
    ID_NODO VARCHAR2(20) PRIMARY KEY,
    NOMBRE VARCHAR2(100) NOT NULL
);

CREATE TABLE ARISTAS (
    ID_ARISTA NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    NODO_ORIGEN VARCHAR2(20) NOT NULL,
    NODO_DESTINO VARCHAR2(20) NOT NULL,
    PESO NUMBER(10,2) NOT NULL,

    CONSTRAINT FK_ARISTA_ORIGEN
        FOREIGN KEY (NODO_ORIGEN)
        REFERENCES NODOS(ID_NODO),

    CONSTRAINT FK_ARISTA_DESTINO
        FOREIGN KEY (NODO_DESTINO)
        REFERENCES NODOS(ID_NODO),

    CONSTRAINT CK_ARISTA_PESO
        CHECK (PESO >= 0),

    CONSTRAINT CK_ARISTA_NODOS
        CHECK (NODO_ORIGEN <> NODO_DESTINO)
);

CREATE TABLE HISTORIAL_RUTAS (
    ID_HISTORIAL NUMBER GENERATED ALWAYS AS IDENTITY
        PRIMARY KEY,
    NODO_ORIGEN VARCHAR2(20) NOT NULL,
    NODO_DESTINO VARCHAR2(20) NOT NULL,
    RUTA VARCHAR2(2000),
    DISTANCIA NUMBER(12,2),
    FECHA_CALCULO TIMESTAMP DEFAULT SYSTIMESTAMP NOT NULL,
    ENCONTRADA CHAR(1) NOT NULL,

    CONSTRAINT CK_HISTORIAL_ENCONTRADA
        CHECK (ENCONTRADA IN ('S', 'N'))
);

-- Nodos de ejemplo
INSERT INTO NODOS VALUES ('A', 'Nodo A');
INSERT INTO NODOS VALUES ('B', 'Nodo B');
INSERT INTO NODOS VALUES ('C', 'Nodo C');
INSERT INTO NODOS VALUES ('D', 'Nodo D');
INSERT INTO NODOS VALUES ('E', 'Nodo E');
INSERT INTO NODOS VALUES ('F', 'Nodo F');

-- Conexiones y pesos
INSERT INTO ARISTAS (NODO_ORIGEN, NODO_DESTINO, PESO)
VALUES ('A', 'B', 4);

INSERT INTO ARISTAS (NODO_ORIGEN, NODO_DESTINO, PESO)
VALUES ('A', 'C', 2);

INSERT INTO ARISTAS (NODO_ORIGEN, NODO_DESTINO, PESO)
VALUES ('B', 'C', 1);

INSERT INTO ARISTAS (NODO_ORIGEN, NODO_DESTINO, PESO)
VALUES ('B', 'D', 5);

INSERT INTO ARISTAS (NODO_ORIGEN, NODO_DESTINO, PESO)
VALUES ('C', 'D', 8);

INSERT INTO ARISTAS (NODO_ORIGEN, NODO_DESTINO, PESO)
VALUES ('C', 'E', 10);

INSERT INTO ARISTAS (NODO_ORIGEN, NODO_DESTINO, PESO)
VALUES ('D', 'E', 2);

INSERT INTO ARISTAS (NODO_ORIGEN, NODO_DESTINO, PESO)
VALUES ('D', 'F', 6);

INSERT INTO ARISTAS (NODO_ORIGEN, NODO_DESTINO, PESO)
VALUES ('E', 'F', 3);

COMMIT;

-- Verificar los datos
SELECT * FROM NODOS ORDER BY ID_NODO;

SELECT
    NODO_ORIGEN,
    NODO_DESTINO,
    PESO
FROM ARISTAS
ORDER BY ID_ARISTA;
```

> Si vas a ejecutar el script más de una vez, elimina primero las tablas en orden inverso de dependencias o utiliza un esquema de prueba limpio.

## 4. Crear el proyecto en Visual Studio 2022

1. Abre Visual Studio 2022.
2. Selecciona **Crear un proyecto nuevo**.
3. Elige **Aplicación de consola** en C#.
4. Asigna el nombre `DijkstraOracle19c`.
5. Selecciona .NET 8.0, si está instalado, o un framework compatible.
6. Abre la terminal integrada en la carpeta del proyecto.
7. Instala el proveedor de Oracle:

```powershell
dotnet add package Oracle.ManagedDataAccess.Core
```

## 5. Archivo `DijkstraOracle19c.csproj`

Si necesitas crear el archivo del proyecto manualmente, puedes usar este ejemplo. Visual Studio también puede generarlo automáticamente.

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Oracle.ManagedDataAccess.Core" Version="23.9.1" />
  </ItemGroup>

</Project>
```

Si NuGet ofrece una versión más reciente compatible, puedes instalarla con `dotnet add package Oracle.ManagedDataAccess.Core` y dejar que el comando actualice el archivo del proyecto.

## 6. Código completo: `Program.cs`

Antes de ejecutar, cambia `TU_USUARIO`, `TU_CLAVE` y `localhost:1521/orcl` por los datos de conexión de tu instalación de Oracle.

```csharp
using System;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;

namespace DijkstraOracle19c
{
    internal class Program
    {
        // Actualiza estos datos según tu instalación de Oracle 19c.
        private const string ConnectionString =
            "User Id=TU_USUARIO;" +
            "Password=TU_CLAVE;" +
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
                    "\nConexion a Oracle 19c exitosa.");

                Dictionary<string, string> nodos =
                    CargarNodos(conexion);

                if (nodos.Count == 0)
                {
                    Console.WriteLine(
                        "No hay nodos registrados en la tabla NODOS.");
                    return;
                }

                Dictionary<string, List<Arista>> grafo =
                    CargarGrafo(conexion, nodos.Keys);

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

                if (!nodos.ContainsKey(origen) ||
                    !nodos.ContainsKey(destino))
                {
                    Console.WriteLine(
                        "\nError: el origen o destino no existe.");
                    return;
                }

                ResultadoRuta resultado =
                    CalcularDijkstra(grafo, origen, destino);

                Console.WriteLine("\n========== RESULTADO ==========");

                if (resultado.Encontrada)
                {
                    Console.WriteLine(
                        "Ruta mas corta: " +
                        string.Join(" -> ", resultado.Ruta));

                    Console.WriteLine(
                        $"Distancia total: {resultado.Distancia}");
                }
                else
                {
                    Console.WriteLine(
                        "No existe una ruta entre los nodos.");
                }

                GuardarHistorial(
                    conexion, origen, destino, resultado);

                Console.WriteLine(
                    "Consulta registrada en HISTORIAL_RUTAS.");

                Console.WriteLine("===============================");
            }
            catch (OracleException ex)
            {
                Console.WriteLine(
                    "\nError de Oracle: " + ex.Message);
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
            return (Console.ReadLine() ?? "")
                .Trim()
                .ToUpperInvariant();
        }

        // Consulta los nodos almacenados en Oracle.
        private static Dictionary<string, string> CargarNodos(
            OracleConnection conexion)
        {
            var nodos = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);

            const string sql =
                "SELECT ID_NODO, NOMBRE FROM NODOS";

            using var comando =
                new OracleCommand(sql, conexion);

            using var lector = comando.ExecuteReader();

            while (lector.Read())
            {
                string id = lector.GetString(0);
                string nombre = lector.GetString(1);

                nodos[id] = nombre;
            }

            return nodos;
        }

        // Carga las aristas y crea un grafo bidireccional.
        private static Dictionary<string, List<Arista>>
            CargarGrafo(
                OracleConnection conexion,
                IEnumerable<string> idsNodos)
        {
            var grafo =
                new Dictionary<string, List<Arista>>(
                    StringComparer.OrdinalIgnoreCase);

            foreach (string id in idsNodos)
            {
                grafo[id] = new List<Arista>();
            }

            const string sql = @"
                SELECT NODO_ORIGEN, NODO_DESTINO, PESO
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
                        "Dijkstra no admite pesos negativos.");
                }

                grafo[origen].Add(
                    new Arista(destino, peso));

                grafo[destino].Add(
                    new Arista(origen, peso));
            }

            return grafo;
        }

        // Implementación del algoritmo de Dijkstra.
        private static ResultadoRuta CalcularDijkstra(
            Dictionary<string, List<Arista>> grafo,
            string origen,
            string destino)
        {
            var distancias =
                new Dictionary<string, decimal>(
                    StringComparer.OrdinalIgnoreCase);

            var anteriores =
                new Dictionary<string, string>(
                    StringComparer.OrdinalIgnoreCase);

            foreach (string nodo in grafo.Keys)
            {
                distancias[nodo] = decimal.MaxValue;
            }

            distancias[origen] = 0m;

            var pendientes =
                new PriorityQueue<string, decimal>();

            pendientes.Enqueue(origen, 0m);

            while (pendientes.Count > 0)
            {
                pendientes.TryDequeue(
                    out string? actual,
                    out decimal distanciaActual);

                if (actual is null)
                    continue;

                // Ignora entradas antiguas de la cola de prioridad.
                if (distanciaActual != distancias[actual])
                    continue;

                if (actual == destino)
                    break;

                foreach (Arista arista in grafo[actual])
                {
                    // Evita desbordamientos decimales.
                    if (distancias[actual] >
                        decimal.MaxValue - arista.Peso)
                    {
                        continue;
                    }

                    decimal nuevaDistancia =
                        distancias[actual] + arista.Peso;

                    if (nuevaDistancia <
                        distancias[arista.Destino])
                    {
                        distancias[arista.Destino] =
                            nuevaDistancia;

                        anteriores[arista.Destino] = actual;

                        pendientes.Enqueue(
                            arista.Destino, nuevaDistancia);
                    }
                }
            }

            if (distancias[destino] == decimal.MaxValue)
            {
                return new ResultadoRuta
                {
                    Encontrada = false
                };
            }

            var ruta = new List<string>();
            string nodoActual = destino;

            ruta.Add(nodoActual);

            while (nodoActual != origen)
            {
                if (!anteriores.TryGetValue(
                    nodoActual, out string? anterior))
                {
                    return new ResultadoRuta
                    {
                        Encontrada = false
                    };
                }

                nodoActual = anterior;
                ruta.Add(nodoActual);
            }

            ruta.Reverse();

            return new ResultadoRuta
            {
                Encontrada = true,
                Distancia = distancias[destino],
                Ruta = ruta
            };
        }

        // Guarda tanto las rutas encontradas como las no encontradas.
        private static void GuardarHistorial(
            OracleConnection conexion,
            string origen,
            string destino,
            ResultadoRuta resultado)
        {
            const string sql = @"
                INSERT INTO HISTORIAL_RUTAS
                (
                    NODO_ORIGEN,
                    NODO_DESTINO,
                    RUTA,
                    DISTANCIA,
                    ENCONTRADA
                )
                VALUES
                (
                    :origen,
                    :destino,
                    :ruta,
                    :distancia,
                    :encontrada
                )";

            using var comando =
                new OracleCommand(sql, conexion);

            comando.BindByName = true;

            comando.Parameters.Add(
                "origen", OracleDbType.Varchar2)
                .Value = origen;

            comando.Parameters.Add(
                "destino", OracleDbType.Varchar2)
                .Value = destino;

            comando.Parameters.Add(
                "ruta", OracleDbType.Varchar2)
                .Value = resultado.Encontrada
                    ? string.Join(" -> ", resultado.Ruta)
                    : DBNull.Value;

            comando.Parameters.Add(
                "distancia", OracleDbType.Decimal)
                .Value = resultado.Encontrada
                    ? resultado.Distancia
                    : DBNull.Value;

            comando.Parameters.Add(
                "encontrada", OracleDbType.Char)
                .Value = resultado.Encontrada ? "S" : "N";

            comando.ExecuteNonQuery();
        }
    }
}
```

## 7. Configurar la conexión

En la constante `ConnectionString`, reemplaza los valores de ejemplo:

```csharp
private const string ConnectionString =
    "User Id=TU_USUARIO;" +
    "Password=TU_CLAVE;" +
    "Data Source=localhost:1521/orcl;";
```

Ejemplo de los parámetros:

| Parámetro | Ejemplo |
|---|---|
| Servidor | `localhost` |
| Puerto | `1521` |
| Servicio Oracle | `orcl` |
| Usuario | Tu usuario de base de datos |
| Contraseña | Tu contraseña de base de datos |

Si tu base de datos utiliza otro nombre de servicio, modifica `orcl` según corresponda. No guardes contraseñas reales en un repositorio público.

## 8. Ejecutar el programa

1. Ejecuta `01_crear_base_datos.sql` en Oracle.
2. Configura la cadena de conexión.
3. Instala el paquete NuGet.
4. Compila el proyecto en Visual Studio 2022.
5. Ejecuta con **Ctrl + F5**.
6. Ingresa `A` como origen y `F` como destino.

Ejemplo de interacción:

```text
==================================
 ALGORITMO DE DIJKSTRA
 C# + ORACLE DATABASE 19c
==================================

Conexion a Oracle 19c exitosa.

Nodos disponibles:
A - Nodo A
B - Nodo B
C - Nodo C
D - Nodo D
E - Nodo E
F - Nodo F

Ingrese el nodo de origen: A
Ingrese el nodo de destino: F

========== RESULTADO ==========
Ruta mas corta: A -> C -> B -> D -> E -> F
Distancia total: 13
Consulta registrada en HISTORIAL_RUTAS.
===============================
```

La ruta esperada es `A -> C -> B -> D -> E -> F`, con un costo total de `13`.

## 9. Consultar el historial en Oracle

```sql
SELECT
    ID_HISTORIAL,
    NODO_ORIGEN,
    NODO_DESTINO,
    RUTA,
    DISTANCIA,
    FECHA_CALCULO,
    ENCONTRADA
FROM HISTORIAL_RUTAS
ORDER BY ID_HISTORIAL DESC;
```

Consultar específicamente la ruta de A a F:

```sql
SELECT
    RUTA,
    DISTANCIA,
    FECHA_CALCULO
FROM HISTORIAL_RUTAS
WHERE NODO_ORIGEN = 'A'
  AND NODO_DESTINO = 'F'
  AND ENCONTRADA = 'S'
ORDER BY ID_HISTORIAL DESC;
```

## 10. Descripción de los componentes

| Componente | Responsabilidad |
|---|---|
| `NODOS` | Almacena los vértices del grafo. |
| `ARISTAS` | Almacena las conexiones y sus pesos. |
| `HISTORIAL_RUTAS` | Guarda cada consulta de ruta. |
| `CargarNodos()` | Consulta los nodos en Oracle. |
| `CargarGrafo()` | Construye el grafo en memoria. |
| `CalcularDijkstra()` | Calcula la ruta de menor costo. |
| `GuardarHistorial()` | Registra el resultado en Oracle. |

## 11. Consideraciones

- El ejemplo considera las conexiones bidireccionales.
- Los pesos deben ser no negativos.
- Si se agregan nodos o conexiones en Oracle, el programa los consulta al iniciar cada ejecución.
- El historial registra tanto las rutas encontradas (`S`) como las no encontradas (`N`).
- Para grafos dirigidos, elimina la inserción inversa de aristas en `CargarGrafo()`.
- La complejidad habitual con una cola de prioridad es `O((V + E) log V)`, donde `V` es el número de nodos y `E` el número de aristas.

## 12. Autor y propósito

Proyecto educativo para practicar estructuras de datos, grafos, algoritmos de caminos mínimos, C#, Visual Studio 2022 y persistencia de información en Oracle Database 19c .
:. . / .
