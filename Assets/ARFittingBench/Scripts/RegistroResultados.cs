using System;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace ARFittingBench
{
    /// <summary>
    /// Grava o resultado de cada rodada em um CSV no armazenamento do app,
    /// para comparar os modos de controle.
    /// </summary>
    public static class RegistroResultados
    {
        public static string Caminho => Path.Combine(Application.persistentDataPath, "resultados_bancada.csv");

        public static void Registrar(ControleConfig modo, RodadaResultado resultado)
        {
            try
            {
                bool novo = !File.Exists(Caminho);
                using var arquivo = new StreamWriter(Caminho, append: true);

                if (novo)
                    arquivo.WriteLine("data_hora,modo,origem,multiplicador,distancia_frente_m,rodada,tempo_s,erro_posicao_cm,erro_angulo_graus");

                arquivo.WriteLine(string.Join(",",
                    DateTime.Now.ToString("s", CultureInfo.InvariantCulture),
                    modo.nomeModo.Replace(',', ' '),
                    modo.origem,
                    Numero(modo.multiplicador),
                    Numero(modo.distanciaFrente),
                    resultado.rodada,
                    Numero(resultado.tempo),
                    Numero(resultado.erroPosicao * 100f),
                    Numero(resultado.erroAngulo)));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Não foi possível gravar o resultado da rodada: {e.Message}");
            }
        }

        static string Numero(float valor) => valor.ToString("0.###", CultureInfo.InvariantCulture);
    }
}
