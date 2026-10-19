using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ARFittingBench
{
    /// <summary>
    /// Fixa a bancada no ponto tocado de uma superfície detectada, usando uma âncora do AR Foundation.
    /// </summary>
    [RequireComponent(typeof(ARRaycastManager), typeof(ARAnchorManager), typeof(ARPlaneManager))]
    public class BancadaPlacer : MonoBehaviour
    {
        [SerializeField] Bancada bancadaPrefab;
        [SerializeField] MovimentoPeca movimento;
        [SerializeField] Camera cam;

        static readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();

        ARRaycastManager raycastManager;
        ARAnchorManager anchorManager;
        ARPlaneManager planeManager;
        GameObject ancora;
        bool fixando;

        public Bancada BancadaAtual { get; private set; }
        public bool HaSuperficies => planeManager.trackables.count > 0;

        public event System.Action<Bancada> BancadaFixada;

        void Awake()
        {
            raycastManager = GetComponent<ARRaycastManager>();
            anchorManager = GetComponent<ARAnchorManager>();
            planeManager = GetComponent<ARPlaneManager>();
            if (cam == null)
                cam = Camera.main;
        }

        void Update()
        {
            if (BancadaAtual != null || fixando)
                return;

            if (!Toque.Tap(out Vector2 posicao))
                return;

            if (!raycastManager.Raycast(posicao, hits, TrackableType.PlaneWithinPolygon))
                return;

            Toque.Consumir();
            Fixar(hits[0]);
        }

        async void Fixar(ARRaycastHit hit)
        {
            fixando = true;

            // A bancada fica virada para o usuário no momento do toque.
            Vector3 up = hit.pose.up;
            Vector3 paraUsuario = Vector3.ProjectOnPlane(cam.transform.position - hit.pose.position, up);
            Quaternion rotacao = paraUsuario.sqrMagnitude > 1e-4f
                ? Quaternion.LookRotation(-paraUsuario, up)
                : hit.pose.rotation;
            var pose = new Pose(hit.pose.position, rotacao);

            ARAnchor anchor = null;
            try
            {
                if (hit.trackable is ARPlane plano)
                    anchor = anchorManager.AttachAnchor(plano, pose);

                if (anchor == null)
                {
                    var resultado = await anchorManager.TryAddAnchorAsync(pose);
                    if (resultado.status.IsSuccess())
                        anchor = resultado.value;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Não foi possível criar a âncora: {e.Message}");
            }

            if (this == null)
                return;

            if (anchor != null)
            {
                ancora = anchor.gameObject;
            }
            else
            {
                Debug.LogWarning("Âncora indisponível; a bancada será fixada sem rastreamento de âncora.");
                ancora = new GameObject("Ancora Bancada");
                ancora.transform.SetPositionAndRotation(pose.position, pose.rotation);
            }

            BancadaAtual = Instantiate(bancadaPrefab, ancora.transform);
            BancadaAtual.transform.SetPositionAndRotation(pose.position, pose.rotation);

            MostrarSuperficies(false);
            movimento.DefinirBancada(BancadaAtual);
            fixando = false;
            BancadaFixada?.Invoke(BancadaAtual);
        }

        /// <summary>Remove a bancada e volta a procurar superfícies.</summary>
        public void Reposicionar()
        {
            movimento.DefinirBancada(null);

            if (ancora != null)
            {
                var anchor = ancora.GetComponent<ARAnchor>();
                if (anchor == null || !anchorManager.TryRemoveAnchor(anchor))
                    Destroy(ancora);
            }

            ancora = null;
            BancadaAtual = null;
            MostrarSuperficies(true);
        }

        void MostrarSuperficies(bool mostrar)
        {
            planeManager.enabled = mostrar;
            foreach (var plano in planeManager.trackables)
                plano.gameObject.SetActive(mostrar);
        }
    }
}
