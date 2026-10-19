using UnityEngine;
using UnityEngine.InputSystem;

namespace ARFittingBench
{
    /// <summary>
    /// Único script de movimentação da peça. Lê do <see cref="ControleConfig"/> a distância
    /// à frente do celular, o multiplicador do movimento e a origem do controle (celular ou tela).
    /// Tocar a peça a pega; tocar de novo a solta sobre a bancada.
    /// </summary>
    public class MovimentoPeca : MonoBehaviour
    {
        [SerializeField] ControleConfig config;
        [SerializeField] Camera cam;

        [Tooltip("Raio em torno da peça na tela, em fração da altura da tela, que também conta como tocar a peça.")]
        [SerializeField] float raioToquePeca = 0.07f;

        [Tooltip("Quanto maior, mais rápido a peça alcança a posição desejada.")]
        [SerializeField] float suavizacao = 18f;

        Bancada bancada;
        Vector3 posicaoCameraInicial;
        Quaternion rotacaoCameraInicial;
        Quaternion rotacaoPecaInicial;
        Vector3 posicaoAlvo;
        Quaternion rotacaoAlvo;
        float anguloDoisDedos;
        bool doisDedosAtivo;

        public bool Segurando { get; private set; }

        public ControleConfig Config
        {
            get => config;
            set
            {
                Soltar();
                config = value;
            }
        }

        void Awake()
        {
            if (cam == null)
                cam = Camera.main;
        }

        public void DefinirBancada(Bancada nova)
        {
            if (bancada != null)
                bancada.RodadaConcluida -= AoConcluirRodada;

            Segurando = false;
            bancada = nova;

            if (bancada != null)
                bancada.RodadaConcluida += AoConcluirRodada;
        }

        void AoConcluirRodada(RodadaResultado _) => Segurando = false;

        void Update()
        {
            if (bancada == null || config == null)
            {
                Segurando = false;
                return;
            }

            if (bancada.EmTransicao)
                return;

            if (Toque.Tap(out Vector2 posicaoTap))
            {
                if (Segurando)
                {
                    Toque.Consumir();
                    Soltar();
                }
                else
                {
                    TentarPegar(posicaoTap);
                }
            }

            if (!Segurando)
                return;

            if (config.origem == OrigemControle.Celular)
                AtualizarPeloCelular();
            else
                AtualizarPelaTela();

            Transform peca = bancada.Peca;
            float t = 1f - Mathf.Exp(-suavizacao * Time.deltaTime);
            peca.SetPositionAndRotation(
                Vector3.Lerp(peca.position, posicaoAlvo, t),
                Quaternion.Slerp(peca.rotation, rotacaoAlvo, t));
        }

        void TentarPegar(Vector2 posicaoTela)
        {
            Transform peca = bancada.Peca;

            bool acertou = Physics.Raycast(cam.ScreenPointToRay(posicaoTela), out RaycastHit hit, 20f)
                && hit.transform.IsChildOf(peca);

            if (!acertou)
            {
                Vector3 pecaNaTela = cam.WorldToScreenPoint(peca.position);
                acertou = pecaNaTela.z > 0f
                    && Vector2.Distance(pecaNaTela, posicaoTela) <= raioToquePeca * Screen.height;
            }

            if (!acertou)
                return;

            Toque.Consumir();
            Segurando = true;
            doisDedosAtivo = false;

            posicaoCameraInicial = cam.transform.position;
            rotacaoCameraInicial = cam.transform.rotation;
            rotacaoPecaInicial = peca.rotation;
            posicaoAlvo = peca.position;
            rotacaoAlvo = peca.rotation;
        }

        /// <summary>
        /// A peça fica a <c>distanciaFrente</c> à frente do celular. O deslocamento do celular desde
        /// o momento em que a peça foi pega é multiplicado por <c>multiplicador</c>; a rotação é 1:1.
        /// </summary>
        void AtualizarPeloCelular()
        {
            Transform transformCamera = cam.transform;
            Vector3 deslocamento = (transformCamera.position - posicaoCameraInicial) * config.multiplicador;
            Vector3 posicao = posicaoCameraInicial + deslocamento + transformCamera.forward * config.distanciaFrente;

            posicaoAlvo = ManterAcimaDaBancada(posicao);
            rotacaoAlvo = transformCamera.rotation * Quaternion.Inverse(rotacaoCameraInicial) * rotacaoPecaInicial;
        }

        /// <summary>
        /// Um dedo arrasta a peça sobre a bancada (deslocamento multiplicado por <c>multiplicador</c>);
        /// dois dedos girando a rotacionam em torno da vertical.
        /// </summary>
        void AtualizarPelaTela()
        {
            Transform b = bancada.transform;
            Transform transformCamera = cam.transform;

            int ativos = 0;
            Vector2 dedoA = default, dedoB = default;
            if (Touchscreen.current != null)
            {
                foreach (var toque in Touchscreen.current.touches)
                {
                    if (!toque.press.isPressed)
                        continue;
                    if (ativos == 0) dedoA = toque.position.ReadValue();
                    else if (ativos == 1) dedoB = toque.position.ReadValue();
                    ativos++;
                }
            }

            if (ativos >= 2)
            {
                float angulo = Mathf.Atan2(dedoB.y - dedoA.y, dedoB.x - dedoA.x) * Mathf.Rad2Deg;
                if (doisDedosAtivo)
                    rotacaoAlvo = Quaternion.AngleAxis(-Mathf.DeltaAngle(anguloDoisDedos, angulo), b.up) * rotacaoAlvo;
                anguloDoisDedos = angulo;
                doisDedosAtivo = true;
                return;
            }

            doisDedosAtivo = false;

            // Pointer cobre o dedo no celular e o mouse no Editor.
            var ponteiro = Pointer.current;
            if (ponteiro == null || !ponteiro.press.isPressed)
                return;

            Vector2 arraste = ponteiro.delta.ReadValue();
            if (arraste == Vector2.zero)
                return;

            Vector3 direita = Vector3.ProjectOnPlane(transformCamera.right, b.up).normalized;
            Vector3 frente = Vector3.Cross(direita, b.up).normalized;
            float distancia = Vector3.Distance(transformCamera.position, bancada.Peca.position);
            float metrosPorPixel = 2f * distancia * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) / Screen.height;

            Vector3 posicao = posicaoAlvo + (direita * arraste.x + frente * arraste.y) * metrosPorPixel * config.multiplicador;
            posicaoAlvo = NaSuperficie(bancada.LimitarArea(posicao));
        }

        void Soltar()
        {
            if (!Segurando || bancada == null)
            {
                Segurando = false;
                return;
            }

            Segurando = false;

            // A peça cai deitada sobre a bancada, mantendo só o giro em torno da vertical.
            Transform peca = bancada.Peca;
            Vector3 up = bancada.transform.up;
            Vector3 frente = Vector3.ProjectOnPlane(peca.forward, up);
            if (frente.sqrMagnitude < 1e-4f)
                frente = Vector3.ProjectOnPlane(-peca.up, up);

            peca.SetPositionAndRotation(
                NaSuperficie(bancada.LimitarArea(peca.position)),
                Quaternion.LookRotation(frente, up));
        }

        Vector3 ManterAcimaDaBancada(Vector3 pontoMundo)
        {
            Transform b = bancada.transform;
            Vector3 local = b.InverseTransformPoint(pontoMundo);
            local.y = Mathf.Max(local.y, bancada.AlturaSuperficie);
            return b.TransformPoint(local);
        }

        Vector3 NaSuperficie(Vector3 pontoMundo)
        {
            Transform b = bancada.transform;
            Vector3 local = b.InverseTransformPoint(pontoMundo);
            local.y = bancada.AlturaSuperficie;
            return b.TransformPoint(local);
        }
    }
}
