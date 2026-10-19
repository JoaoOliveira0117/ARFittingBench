using System.Collections;
using UnityEngine;

namespace ARFittingBench
{
    public struct RodadaResultado
    {
        public int rodada;
        public float tempo;
        public float erroPosicao;
        public float erroAngulo;
    }

    /// <summary>
    /// Área de jogo ancorada na superfície real. Sorteia as posições da peça e do encaixe
    /// a cada rodada, colore o encaixe conforme a proximidade e reconhece o encaixe.
    /// </summary>
    public class Bancada : MonoBehaviour
    {
        [Header("Objetos")]
        [SerializeField] Transform peca;
        [SerializeField] Transform encaixe;
        [SerializeField] Renderer[] paredesEncaixe;

        [Header("Área de jogo (m)")]
        [SerializeField] Vector2 tamanhoArea = new Vector2(0.6f, 0.4f);
        [SerializeField] float margem = 0.09f;
        [SerializeField] float alturaSuperficie = 0.02f;
        [SerializeField] float distanciaMinima = 0.22f;
        [SerializeField] float diferencaAnguloMinima = 45f;

        [Header("Tolerâncias")]
        [SerializeField] float toleranciaPosicao = 0.02f;
        [SerializeField] float toleranciaAngulo = 10f;

        [Header("Feedback")]
        [SerializeField] float distanciaFeedback = 0.35f;
        [SerializeField] Color corLonge = new Color(0.9f, 0.15f, 0.1f);
        [SerializeField] Color corMeio = new Color(1f, 0.8f, 0.1f);
        [SerializeField] Color corPerto = new Color(0.65f, 0.9f, 0.2f);
        [SerializeField] Color corAcerto = new Color(0.1f, 1f, 0.25f);
        [SerializeField] float pausaEntreRodadas = 1.5f;

        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        MaterialPropertyBlock bloco;
        float inicioRodada;

        public Transform Peca => peca;
        public float AlturaSuperficie => alturaSuperficie;
        public int Rodada { get; private set; }
        public bool EmTransicao { get; private set; }
        public float ErroPosicao { get; private set; }
        public float ErroAngulo { get; private set; }
        public float TempoRodada => Time.time - inicioRodada;

        public event System.Action<RodadaResultado> RodadaConcluida;

        void Awake()
        {
            bloco = new MaterialPropertyBlock();
        }

        void Start()
        {
            NovaRodada();
        }

        void Update()
        {
            if (EmTransicao)
                return;

            ErroPosicao = Vector3.Distance(peca.position, encaixe.position);
            ErroAngulo = Quaternion.Angle(peca.rotation, encaixe.rotation);

            if (ErroPosicao <= toleranciaPosicao && ErroAngulo <= toleranciaAngulo)
            {
                StartCoroutine(ConcluirRodada());
                return;
            }

            float proximidade = Mathf.Clamp01(1f - ErroPosicao / distanciaFeedback)
                * Mathf.Lerp(0.5f, 1f, Mathf.Clamp01(1f - ErroAngulo / 90f));
            Color cor = proximidade < 0.5f
                ? Color.Lerp(corLonge, corMeio, proximidade * 2f)
                : Color.Lerp(corMeio, corPerto, (proximidade - 0.5f) * 2f);
            PintarEncaixe(cor, Color.black);
        }

        public void NovaRodada()
        {
            Rodada++;

            Vector3 posicaoEncaixe = PontoAleatorio();
            Vector3 posicaoPeca = PontoAleatorio();
            for (int i = 0; i < 50 && Vector3.Distance(posicaoPeca, posicaoEncaixe) < distanciaMinima; i++)
                posicaoPeca = PontoAleatorio();

            Quaternion rotacaoEncaixe = GiroAleatorio();
            Quaternion rotacaoPeca = GiroAleatorio();
            for (int i = 0; i < 50 && Quaternion.Angle(rotacaoPeca, rotacaoEncaixe) < diferencaAnguloMinima; i++)
                rotacaoPeca = GiroAleatorio();

            encaixe.SetLocalPositionAndRotation(posicaoEncaixe, rotacaoEncaixe);
            peca.SetLocalPositionAndRotation(posicaoPeca, rotacaoPeca);

            inicioRodada = Time.time;
            EmTransicao = false;
        }

        /// <summary>Restringe um ponto (em coordenadas de mundo) à área da bancada.</summary>
        public Vector3 LimitarArea(Vector3 pontoMundo)
        {
            Vector3 local = transform.InverseTransformPoint(pontoMundo);
            local.x = Mathf.Clamp(local.x, -tamanhoArea.x / 2f, tamanhoArea.x / 2f);
            local.z = Mathf.Clamp(local.z, -tamanhoArea.y / 2f, tamanhoArea.y / 2f);
            return transform.TransformPoint(local);
        }

        IEnumerator ConcluirRodada()
        {
            EmTransicao = true;

            var resultado = new RodadaResultado
            {
                rodada = Rodada,
                tempo = TempoRodada,
                erroPosicao = ErroPosicao,
                erroAngulo = ErroAngulo
            };

            peca.SetPositionAndRotation(encaixe.position, encaixe.rotation);
            PintarEncaixe(corAcerto, corAcerto * 1.5f);
            RodadaConcluida?.Invoke(resultado);

            yield return new WaitForSeconds(pausaEntreRodadas);
            NovaRodada();
        }

        Vector3 PontoAleatorio()
        {
            float x = tamanhoArea.x / 2f - margem;
            float z = tamanhoArea.y / 2f - margem;
            return new Vector3(Random.Range(-x, x), alturaSuperficie, Random.Range(-z, z));
        }

        static Quaternion GiroAleatorio() => Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        void PintarEncaixe(Color cor, Color emissao)
        {
            bloco.SetColor(BaseColor, cor);
            bloco.SetColor(EmissionColor, emissao);
            foreach (var parede in paredesEncaixe)
                parede.SetPropertyBlock(bloco);
        }
    }
}
