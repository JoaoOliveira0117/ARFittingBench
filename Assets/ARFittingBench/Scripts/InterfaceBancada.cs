using UnityEngine;
using UnityEngine.UI;

namespace ARFittingBench
{
    /// <summary>
    /// Instruções na tela, seleção do modo de controle e informações da rodada.
    /// </summary>
    public class InterfaceBancada : MonoBehaviour
    {
        [SerializeField] BancadaPlacer placer;
        [SerializeField] MovimentoPeca movimento;

        [Header("Modos de controle")]
        [SerializeField] ControleConfig[] modos;
        [SerializeField] Button[] botoesModo;
        [SerializeField] Color corBotaoAtivo = new Color(0.1f, 0.55f, 1f, 0.95f);
        [SerializeField] Color corBotaoInativo = new Color(0.08f, 0.08f, 0.08f, 0.7f);

        [Header("Elementos")]
        [SerializeField] Button botaoReposicionar;
        [SerializeField] Text textoStatus;
        [SerializeField] Text textoInfo;
        [SerializeField] float duracaoMensagemAcerto = 1.5f;

        Bancada bancada;
        string mensagemAcerto;
        float fimMensagemAcerto;

        void Start()
        {
            for (int i = 0; i < botoesModo.Length && i < modos.Length; i++)
            {
                int indice = i;
                botoesModo[i].onClick.AddListener(() => SelecionarModo(indice));
                var rotulo = botoesModo[i].GetComponentInChildren<Text>();
                if (rotulo != null)
                    rotulo.text = modos[i].nomeModo;
            }

            botaoReposicionar.onClick.AddListener(placer.Reposicionar);
            placer.BancadaFixada += AoFixarBancada;

            int atual = System.Array.IndexOf(modos, movimento.Config);
            SelecionarModo(atual >= 0 ? atual : 0);
        }

        void AoFixarBancada(Bancada nova)
        {
            bancada = nova;
            bancada.RodadaConcluida += AoConcluirRodada;
        }

        void AoConcluirRodada(RodadaResultado resultado)
        {
            mensagemAcerto = $"Encaixou! Rodada {resultado.rodada} em {resultado.tempo:0.0} s";
            fimMensagemAcerto = Time.time + duracaoMensagemAcerto;
            RegistroResultados.Registrar(movimento.Config, resultado);
        }

        void SelecionarModo(int indice)
        {
            movimento.Config = modos[indice];
            for (int i = 0; i < botoesModo.Length; i++)
                botoesModo[i].image.color = i == indice ? corBotaoAtivo : corBotaoInativo;
        }

        void Update()
        {
            botaoReposicionar.gameObject.SetActive(bancada != null);
            textoStatus.text = TextoStatus();
            textoInfo.text = bancada == null
                ? ""
                : $"Rodada {bancada.Rodada} · {bancada.TempoRodada:0.0} s\n" +
                  $"Distância {bancada.ErroPosicao * 100f:0.0} cm · Ângulo {bancada.ErroAngulo:0}°";
        }

        string TextoStatus()
        {
            if (bancada == null)
                return placer.HaSuperficies
                    ? "Toque na superfície destacada para fixar a bancada"
                    : "Procurando superfícies… aponte o celular para uma mesa ou o chão e mova-o devagar";

            if (Time.time < fimMensagemAcerto)
                return mensagemAcerto;

            if (!movimento.Segurando)
                return "Toque na peça para pegar";

            return movimento.Config.origem == OrigemControle.Celular
                ? "Mova e gire o celular para levar a peça até o encaixe. Toque para soltar."
                : "Arraste para mover e gire com dois dedos. Toque para soltar.";
        }
    }
}
