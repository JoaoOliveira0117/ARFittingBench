using System.Collections.Generic;
using System.IO;
using System.Linq;
using ARFittingBench;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;

/// <summary>
/// Gera materiais, configurações dos modos, prefabs e a cena da Bancada de Encaixe AR.
/// Menu: AR Fitting Bench > Criar Cena Completa.
/// </summary>
public static class BancadaSetup
{
    const string Raiz = "Assets/ARFittingBench";
    const string PastaMateriais = Raiz + "/Materials";
    const string PastaConfig = Raiz + "/Config";
    const string PastaPrefabs = Raiz + "/Prefabs";
    const string PastaCenas = Raiz + "/Scenes";
    const string CaminhoCena = PastaCenas + "/ARFittingBench.unity";

    // Medidas em metros.
    const float AlturaMesa = 0.02f;
    const float AlturaPeca = 0.03f;
    const float AlturaParede = 0.04f;
    const float EspessuraParede = 0.012f;
    const float Folga = 0.006f;
    static readonly Vector2 TamanhoMesa = new Vector2(0.6f, 0.4f);

    // Contorno da peça em L no plano XZ, em sentido anti-horário visto de cima.
    static readonly Vector2[] ContornoL =
    {
        new Vector2(-0.04f, -0.02f),
        new Vector2(0.08f, -0.02f),
        new Vector2(0.08f, 0.02f),
        new Vector2(0.0f, 0.02f),
        new Vector2(0.0f, 0.08f),
        new Vector2(-0.04f, 0.08f),
    };

    struct Materiais
    {
        public Material mesa, peca, marcador, encaixe, fantasma, plano, bordaPlano;
    }

    [MenuItem("AR Fitting Bench/Criar Cena Completa")]
    public static void CriarTudo()
    {
        foreach (var pasta in new[] { PastaMateriais, PastaConfig, PastaPrefabs, PastaCenas })
            Directory.CreateDirectory(pasta);

        var materiais = CriarMateriais();
        var modos = CriarModos().Select(AssetDatabase.GetAssetPath).ToArray();
        var bancada = AssetDatabase.GetAssetPath(CriarPrefabBancada(materiais));
        var plano = AssetDatabase.GetAssetPath(CriarPrefabPlano(materiais));
        AssetDatabase.SaveAssets();

        // A cena nova descarrega objetos não referenciados; os assets são recarregados pelo caminho.
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CriarCena(
            AssetDatabase.LoadAssetAtPath<GameObject>(bancada).GetComponent<Bancada>(),
            AssetDatabase.LoadAssetAtPath<GameObject>(plano),
            modos.Select(AssetDatabase.LoadAssetAtPath<ControleConfig>).ToArray());
        ConfigurarPlataformas();
        AssetDatabase.SaveAssets();

        Debug.Log($"AR Fitting Bench pronto em {CaminhoCena}.");
    }

    // ---------------------------------------------------------------- Materiais

    static Materiais CriarMateriais() => new Materiais
    {
        mesa = Material("Mesa", new Color(0.55f, 0.42f, 0.3f)),
        peca = Material("Peca", new Color(0.15f, 0.45f, 0.95f)),
        marcador = Material("Marcador", Color.white),
        encaixe = Material("Encaixe", new Color(0.9f, 0.15f, 0.1f), emissao: true),
        fantasma = Material("Fantasma", new Color(1f, 1f, 1f, 0.35f), transparente: true),
        plano = Material("Superficie", new Color(0.1f, 0.85f, 1f, 0.25f), transparente: true),
        bordaPlano = Material("BordaSuperficie", new Color(0.1f, 0.85f, 1f)),
    };

    static Material Material(string nome, Color cor, bool transparente = false, bool emissao = false)
    {
        string caminho = $"{PastaMateriais}/{nome}.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(caminho);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(material, caminho);
        }

        material.SetColor("_BaseColor", cor);
        material.SetFloat("_Smoothness", 0.3f);

        if (transparente)
        {
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
            material.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)RenderQueue.Transparent;
        }

        if (emissao)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", Color.black);
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }

        EditorUtility.SetDirty(material);
        return material;
    }

    // ---------------------------------------------------------------- Modos de controle

    static ControleConfig[] CriarModos() => new[]
    {
        Modo("Modo1_Celular", "Celular", 0.3f, 1f, OrigemControle.Celular),
        Modo("Modo2_CelularAmplificado", "Celular ×2", 0.3f, 2f, OrigemControle.Celular),
        Modo("Modo3_Tela", "Tela", 0.3f, 1f, OrigemControle.Tela),
    };

    static ControleConfig Modo(string arquivo, string nome, float distancia, float multiplicador, OrigemControle origem)
    {
        string caminho = $"{PastaConfig}/{arquivo}.asset";
        var config = AssetDatabase.LoadAssetAtPath<ControleConfig>(caminho);
        if (config != null)
            return config;

        config = ScriptableObject.CreateInstance<ControleConfig>();
        config.nomeModo = nome;
        config.distanciaFrente = distancia;
        config.multiplicador = multiplicador;
        config.origem = origem;
        AssetDatabase.CreateAsset(config, caminho);
        return config;
    }

    // ---------------------------------------------------------------- Prefab da bancada

    static Bancada CriarPrefabBancada(Materiais m)
    {
        var raiz = new GameObject("Bancada");

        Caixa("Mesa", raiz.transform, new Vector3(0f, AlturaMesa / 2f, 0f),
            new Vector3(TamanhoMesa.x, AlturaMesa, TamanhoMesa.y), m.mesa);

        // Peça: L com um marcador no topo, para que a orientação fique evidente.
        var peca = new GameObject("Peca").transform;
        peca.SetParent(raiz.transform, false);
        peca.localPosition = new Vector3(-0.15f, AlturaMesa, 0f);
        FormaL(peca, AlturaPeca, m.peca);
        var marcador = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        marcador.name = "Marcador";
        marcador.transform.SetParent(peca, false);
        marcador.transform.localPosition = new Vector3(0.06f, AlturaPeca, 0f);
        marcador.transform.localScale = new Vector3(0.022f, 0.003f, 0.022f);
        marcador.GetComponent<Renderer>().sharedMaterial = m.marcador;
        Object.DestroyImmediate(marcador.GetComponent<Collider>());
        var colisor = peca.gameObject.AddComponent<BoxCollider>();
        colisor.center = new Vector3(0.02f, AlturaPeca / 2f, 0.03f);
        colisor.size = new Vector3(0.15f, AlturaPeca + 0.03f, 0.13f);

        // Encaixe: paredes com o negativo do L e uma silhueta indicando o alvo.
        var encaixe = new GameObject("Encaixe").transform;
        encaixe.SetParent(raiz.transform, false);
        encaixe.localPosition = new Vector3(0.15f, AlturaMesa, 0f);
        var paredes = Paredes(encaixe, m.encaixe);
        var fantasma = new GameObject("Silhueta").transform;
        fantasma.SetParent(encaixe, false);
        FormaL(fantasma, 0.002f, m.fantasma);

        var bancada = raiz.AddComponent<Bancada>();
        var so = new SerializedObject(bancada);
        so.FindProperty("peca").objectReferenceValue = peca;
        so.FindProperty("encaixe").objectReferenceValue = encaixe;
        var lista = so.FindProperty("paredesEncaixe");
        lista.arraySize = paredes.Count;
        for (int i = 0; i < paredes.Count; i++)
            lista.GetArrayElementAtIndex(i).objectReferenceValue = paredes[i];
        so.FindProperty("tamanhoArea").vector2Value = TamanhoMesa;
        so.FindProperty("alturaSuperficie").floatValue = AlturaMesa;
        so.ApplyModifiedPropertiesWithoutUndo();

        var prefab = PrefabUtility.SaveAsPrefabAsset(raiz, $"{PastaPrefabs}/Bancada.prefab");
        Object.DestroyImmediate(raiz);
        return prefab.GetComponent<Bancada>();
    }

    static void FormaL(Transform pai, float altura, Material material)
    {
        Caixa("BracoLongo", pai, new Vector3(0.02f, altura / 2f, 0f), new Vector3(0.12f, altura, 0.04f), material);
        Caixa("BracoCurto", pai, new Vector3(-0.02f, altura / 2f, 0.05f), new Vector3(0.04f, altura, 0.06f), material);
    }

    static List<Renderer> Paredes(Transform pai, Material material)
    {
        int n = ContornoL.Length;
        var normais = new Vector2[n];
        var expandido = new Vector2[n];
        var convexo = new bool[n];

        for (int i = 0; i < n; i++)
        {
            Vector2 d = (ContornoL[(i + 1) % n] - ContornoL[i]).normalized;
            normais[i] = new Vector2(d.y, -d.x);
        }

        for (int i = 0; i < n; i++)
        {
            int anterior = (i + n - 1) % n;
            expandido[i] = ContornoL[i] + Folga * (normais[anterior] + normais[i]);
            Vector2 dAnterior = new Vector2(-normais[anterior].y, normais[anterior].x);
            Vector2 dAtual = new Vector2(-normais[i].y, normais[i].x);
            convexo[i] = dAnterior.x * dAtual.y - dAnterior.y * dAtual.x > 0f;
        }

        var paredes = new List<Renderer>();
        for (int i = 0; i < n; i++)
        {
            Vector2 a = expandido[i];
            Vector2 b = expandido[(i + 1) % n];
            Vector2 d = (b - a).normalized;
            float extInicio = convexo[i] ? EspessuraParede : 0f;
            float extFim = convexo[(i + 1) % n] ? EspessuraParede : 0f;
            float comprimento = Vector2.Distance(a, b) + extInicio + extFim;
            Vector2 centro = (a + b) / 2f + d * (extFim - extInicio) / 2f + normais[i] * EspessuraParede / 2f;

            Vector3 tamanho = Mathf.Abs(d.x) > 0.5f
                ? new Vector3(comprimento, AlturaParede, EspessuraParede)
                : new Vector3(EspessuraParede, AlturaParede, comprimento);

            var parede = Caixa($"Parede{i}", pai, new Vector3(centro.x, AlturaParede / 2f, centro.y), tamanho, material);
            paredes.Add(parede.GetComponent<Renderer>());
        }

        return paredes;
    }

    static GameObject Caixa(string nome, Transform pai, Vector3 posicao, Vector3 tamanho, Material material)
    {
        var caixa = GameObject.CreatePrimitive(PrimitiveType.Cube);
        caixa.name = nome;
        caixa.transform.SetParent(pai, false);
        caixa.transform.localPosition = posicao;
        caixa.transform.localScale = tamanho;
        caixa.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(caixa.GetComponent<Collider>());
        return caixa;
    }

    // ---------------------------------------------------------------- Prefab da superfície detectada

    static GameObject CriarPrefabPlano(Materiais m)
    {
        var go = new GameObject("SuperficieDetectada",
            typeof(ARPlane), typeof(MeshFilter), typeof(MeshRenderer), typeof(LineRenderer), typeof(ARPlaneMeshVisualizer));

        go.GetComponent<MeshRenderer>().sharedMaterial = m.plano;
        go.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;

        var borda = go.GetComponent<LineRenderer>();
        borda.sharedMaterial = m.bordaPlano;
        borda.widthMultiplier = 0.004f;
        borda.useWorldSpace = false;
        borda.loop = true;
        borda.shadowCastingMode = ShadowCastingMode.Off;

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, $"{PastaPrefabs}/SuperficieDetectada.prefab");
        Object.DestroyImmediate(go);
        return prefab;
    }

    // ---------------------------------------------------------------- Cena

    static void CriarCena(Bancada bancadaPrefab, GameObject planoPrefab, ControleConfig[] modos)
    {
        Selection.activeObject = null;

        EditorApplication.ExecuteMenuItem("GameObject/XR/AR Session");
        EditorApplication.ExecuteMenuItem("GameObject/XR/XR Origin (Mobile AR)");
        var origin = Object.FindAnyObjectByType<XROrigin>();
        if (origin == null)
        {
            Debug.LogError("Não foi possível criar o XR Origin (Mobile AR). Verifique se o AR Foundation está instalado.");
            return;
        }

        var luz = new GameObject("Luz Direcional").AddComponent<Light>();
        luz.type = LightType.Directional;
        luz.shadows = LightShadows.Soft;
        luz.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        var xr = origin.gameObject;
        var planeManager = Componente<ARPlaneManager>(xr);
        planeManager.planePrefab = planoPrefab;
        planeManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;
        Componente<ARRaycastManager>(xr);
        Componente<ARAnchorManager>(xr);
        var placer = Componente<BancadaPlacer>(xr);

        var jogo = new GameObject("Jogo");
        var movimento = jogo.AddComponent<MovimentoPeca>();
        var interfaceBancada = jogo.AddComponent<InterfaceBancada>();

        Definir(movimento, ("config", modos[0]), ("cam", origin.Camera));
        Definir(placer, ("bancadaPrefab", bancadaPrefab), ("movimento", movimento), ("cam", origin.Camera));

        // Interface
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        var canvasGO = new GameObject("Interface", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;
        var canvas = canvasGO.transform;

        var botoes = new Button[modos.Length];
        const float larguraBotao = 320f, espaco = 20f;
        float inicioX = -(modos.Length - 1) * (larguraBotao + espaco) / 2f;
        for (int i = 0; i < modos.Length; i++)
        {
            botoes[i] = Botao(canvas, $"Botao{modos[i].name}", modos[i].nomeModo,
                new Vector2(0.5f, 1f), new Vector2(inicioX + i * (larguraBotao + espaco), -150f),
                new Vector2(larguraBotao, 110f));
        }

        var painelStatus = Painel(canvas, "PainelStatus", new Vector2(0.5f, 1f), new Vector2(0f, -320f),
            new Vector2(1000f, 190f), new Color(0f, 0f, 0f, 0.55f));
        var textoStatus = Texto(painelStatus, "Status", 44, TextAnchor.MiddleCenter);

        var painelInfo = Painel(canvas, "PainelInfo", new Vector2(0.5f, 0f), new Vector2(0f, 260f),
            new Vector2(1000f, 150f), new Color(0f, 0f, 0f, 0.45f));
        var textoInfo = Texto(painelInfo, "Info", 38, TextAnchor.MiddleCenter);

        var reposicionar = Botao(canvas, "BotaoReposicionar", "Reposicionar bancada",
            new Vector2(0.5f, 0f), new Vector2(0f, 110f), new Vector2(560f, 110f));
        reposicionar.image.color = new Color(0.08f, 0.08f, 0.08f, 0.7f);

        var so = new SerializedObject(interfaceBancada);
        so.FindProperty("placer").objectReferenceValue = placer;
        so.FindProperty("movimento").objectReferenceValue = movimento;
        Lista(so.FindProperty("modos"), modos);
        Lista(so.FindProperty("botoesModo"), botoes);
        so.FindProperty("botaoReposicionar").objectReferenceValue = reposicionar;
        so.FindProperty("textoStatus").objectReferenceValue = textoStatus;
        so.FindProperty("textoInfo").objectReferenceValue = textoInfo;
        so.ApplyModifiedPropertiesWithoutUndo();

        var cena = EditorSceneManager.GetActiveScene();
        EditorSceneManager.SaveScene(cena, CaminhoCena);

        var cenas = EditorBuildSettings.scenes.Where(c => c.path != CaminhoCena).Select(c =>
        {
            c.enabled = false;
            return c;
        }).ToList();
        cenas.Insert(0, new EditorBuildSettingsScene(CaminhoCena, true));
        EditorBuildSettings.scenes = cenas.ToArray();
    }

    static T Componente<T>(GameObject go) where T : Component =>
        go.TryGetComponent(out T existente) ? existente : go.AddComponent<T>();

    static void Definir(Object alvo, params (string campo, Object valor)[] valores)
    {
        var so = new SerializedObject(alvo);
        foreach (var (campo, valor) in valores)
            so.FindProperty(campo).objectReferenceValue = valor;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void Lista(SerializedProperty propriedade, IReadOnlyList<Object> itens)
    {
        propriedade.arraySize = itens.Count;
        for (int i = 0; i < itens.Count; i++)
            propriedade.GetArrayElementAtIndex(i).objectReferenceValue = itens[i];
    }

    static RectTransform Painel(Transform pai, string nome, Vector2 ancora, Vector2 posicao, Vector2 tamanho, Color cor)
    {
        var go = new GameObject(nome, typeof(RectTransform), typeof(Image));
        var rt = (RectTransform)go.transform;
        rt.SetParent(pai, false);
        rt.anchorMin = rt.anchorMax = rt.pivot = ancora;
        rt.anchoredPosition = posicao;
        rt.sizeDelta = tamanho;
        var imagem = go.GetComponent<Image>();
        imagem.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        imagem.type = Image.Type.Sliced;
        imagem.color = cor;
        imagem.raycastTarget = false;
        return rt;
    }

    static Text Texto(RectTransform pai, string nome, int tamanho, TextAnchor alinhamento)
    {
        var go = new GameObject(nome, typeof(RectTransform), typeof(Text));
        var rt = (RectTransform)go.transform;
        rt.SetParent(pai, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(24f, 12f);
        rt.offsetMax = new Vector2(-24f, -12f);
        var texto = go.GetComponent<Text>();
        texto.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        texto.fontSize = tamanho;
        texto.alignment = alinhamento;
        texto.color = Color.white;
        texto.horizontalOverflow = HorizontalWrapMode.Wrap;
        texto.verticalOverflow = VerticalWrapMode.Truncate;
        texto.resizeTextForBestFit = true;
        texto.resizeTextMinSize = 24;
        texto.resizeTextMaxSize = tamanho;
        texto.raycastTarget = false;
        return texto;
    }

    static Button Botao(Transform pai, string nome, string rotulo, Vector2 ancora, Vector2 posicao, Vector2 tamanho)
    {
        var rt = Painel(pai, nome, ancora, posicao, tamanho, new Color(0.08f, 0.08f, 0.08f, 0.7f));
        rt.GetComponent<Image>().raycastTarget = true;
        var botao = rt.gameObject.AddComponent<Button>();
        var texto = Texto(rt, "Rotulo", 40, TextAnchor.MiddleCenter);
        texto.text = rotulo;
        return botao;
    }

    // ---------------------------------------------------------------- Plataformas

    const string NomeProduto = "AR Fitting Bench";
    const string NomeEmpresa = "ARFittingBench";
    const string IdentificadorApp = "com.arfittingbench.app";

    [MenuItem("AR Fitting Bench/Configurar Android e iOS")]
    public static void ConfigurarPlataformas()
    {
        PlayerSettings.productName = NomeProduto;
        PlayerSettings.companyName = NomeEmpresa;
        foreach (var alvo in new[] { UnityEditor.Build.NamedBuildTarget.Android, UnityEditor.Build.NamedBuildTarget.iOS, UnityEditor.Build.NamedBuildTarget.Standalone })
            PlayerSettings.SetApplicationIdentifier(alvo, IdentificadorApp);

        // Em celulares o app é pausado de qualquer forma; isto atende à validação do XR Plug-in Management.
        PlayerSettings.runInBackground = true;

        PlayerSettings.iOS.cameraUsageDescription = "A câmera é usada para a realidade aumentada.";
        if (string.IsNullOrEmpty(PlayerSettings.iOS.targetOSVersionString) ||
            System.Version.TryParse(PlayerSettings.iOS.targetOSVersionString, out var versao) && versao.Major < 13)
            PlayerSettings.iOS.targetOSVersionString = "13.0";

        // ARCore "Required" com Vulkan exige Android 10 (API 29); ARCore também exige IL2CPP e ARM64.
        if (PlayerSettings.Android.minSdkVersion < AndroidSdkVersions.AndroidApiLevel29)
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;

        HabilitarLoader(BuildTargetGroup.Android, "UnityEngine.XR.ARCore.ARCoreLoader");
        HabilitarLoader(BuildTargetGroup.iOS, "UnityEngine.XR.ARKit.ARKitLoader");
    }

    static void HabilitarLoader(BuildTargetGroup plataforma, string loader)
    {
        if (!EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out XRGeneralSettingsPerBuildTarget porPlataforma))
        {
            Debug.LogWarning("XR Plug-in Management não está configurado; habilite os loaders em Project Settings > XR Plug-in Management.");
            return;
        }

        if (!porPlataforma.HasSettingsForBuildTarget(plataforma))
            porPlataforma.CreateDefaultSettingsForBuildTarget(plataforma);
        if (!porPlataforma.HasManagerSettingsForBuildTarget(plataforma))
            porPlataforma.CreateDefaultManagerSettingsForBuildTarget(plataforma);

        var gerenciador = porPlataforma.ManagerSettingsForBuildTarget(plataforma);
        if (!gerenciador.activeLoaders.Any(l => l != null && l.GetType().FullName == loader))
            XRPackageMetadataStore.AssignLoader(gerenciador, loader, plataforma);
    }
}
