using UnityEngine;

namespace ARFittingBench
{
    public enum OrigemControle
    {
        Celular,
        Tela
    }

    /// <summary>
    /// Arquivo de configuração de um modo de controle. A tarefa, os objetos e as
    /// tolerâncias são os mesmos em todos os modos; só muda como a peça é movida.
    /// </summary>
    [CreateAssetMenu(menuName = "AR Fitting Bench/Configuração de Controle", fileName = "ControleConfig")]
    public class ControleConfig : ScriptableObject
    {
        [Tooltip("Nome exibido no botão de seleção do modo.")]
        public string nomeModo = "Celular";

        [Tooltip("Distância (m) à frente do celular em que a peça fica enquanto está sendo segurada.")]
        [Min(0f)] public float distanciaFrente = 0.3f;

        [Tooltip("Multiplicador aplicado ao movimento (deslocamento do celular ou arraste na tela).")]
        [Min(0.01f)] public float multiplicador = 1f;

        [Tooltip("Celular: a peça acompanha a posição e a rotação do aparelho. Tela: a peça é movida com gestos na tela.")]
        public OrigemControle origem = OrigemControle.Celular;
    }
}
