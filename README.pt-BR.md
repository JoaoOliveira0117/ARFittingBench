# AR Fitting Bench (Bancada de Encaixe AR)

🇺🇸 [Read in English](README.md)

Aplicativo de realidade aumentada para celular (Android e iOS) em que o usuário leva uma peça virtual até um encaixe e alinha as duas. A bancada fica ancorada em uma superfície real, como uma mesa ou o chão. A peça acompanha o celular (ou gestos na tela), e o encaixe muda de cor conforme a peça se aproxima. Ele fica verde quando a peça entra na tolerância de posição e de ângulo.

Feito com **Unity 6 + AR Foundation** (ARCore no Android, ARKit no iOS).

---

## Sumário

1. [Como o app funciona](#como-o-app-funciona)
2. [Requisitos](#requisitos)
3. [Instalação (Windows e macOS)](#instalação-windows-e-macos)
4. [Abrindo o projeto](#abrindo-o-projeto)
5. [Testando no Editor (XR Simulation)](#testando-no-editor-xr-simulation)
6. [Build para Android](#build-para-android)
7. [Build para iOS](#build-para-ios)
8. [Estrutura do projeto](#estrutura-do-projeto)
9. [Configuração](#configuração)
10. [Problemas comuns](#problemas-comuns)

---

## Como o app funciona

1. A câmera procura superfícies planas, que aparecem destacadas em azul.
2. Toque numa superfície para fixar a **bancada**. Ela fica presa a uma âncora AR e não sai do lugar, mesmo que você se afaste.
3. Sobre a bancada aparecem uma **peça** em L (azul) e um **encaixe** (o negativo do L). As posições e rotações dos dois são sorteadas a cada rodada.
4. Toque na peça para pegá-la. Leve-a até o encaixe e gire para alinhar. Toque de novo para soltar.
5. Enquanto a peça se move, o encaixe vai de vermelho a amarelo e depois verde-claro conforme a proximidade. Quando a peça entra na tolerância (2 cm e 10°, por padrão), o encaixe acende em **verde** e uma nova rodada começa.

Os três **modos de controle** usam a mesma tarefa, os mesmos objetos e as mesmas tolerâncias. Muda só o jeito de mover a peça:

| Modo | Como a peça se move |
|---|---|
| **Celular** | A peça fica 30 cm à frente do celular e acompanha a posição e a rotação dele (1:1). |
| **Celular ×2** | Igual ao anterior, mas o deslocamento do celular é multiplicado por 2. |
| **Tela** | Arraste com um dedo para deslizar a peça sobre a bancada. Gire dois dedos para rotacioná-la. |

O resultado de cada rodada é gravado num CSV no aparelho (veja [Configuração](#configuração)).

---

## Requisitos

### Versões exatas usadas no projeto

| Componente | Versão |
|---|---|
| **Unity Editor** | **6000.4.0f1** (Unity 6.4), changeset `8cf496087c8f` |
| AR Foundation | 6.4.3 |
| Google ARCore XR Plugin | 6.4.3 |
| Apple ARKit XR Plugin | 6.4.3 |
| XR Plug-in Management | 4.5.2 |
| XR Core Utilities | 2.5.3 |
| Universal Render Pipeline (URP) | 17.4.0 |
| Input System | 1.19.0 |
| Unity UI (uGUI) | 2.0.0 |

Na primeira vez que você abre o projeto, o Unity baixa todos os pacotes sozinho a partir de `Packages/manifest.json` / `Packages/packages-lock.json`. Não é preciso instalá-los à mão.

> ⚠️ Use **exatamente o Unity 6000.4.0f1**. Outras versões podem atualizar os pacotes e quebrar a configuração de AR.

### Software por plataforma de destino

| Você quer… | No Windows | No macOS |
|---|---|---|
| Abrir e editar o projeto | Unity Hub + Unity 6000.4.0f1 | Unity Hub + Unity 6000.4.0f1 |
| Buildar para **Android** | ✅ Módulos do Unity: *Android Build Support*, *OpenJDK*, *Android SDK & NDK Tools* | ✅ mesmos módulos |
| Buildar para **iOS** | ⚠️ Só dá para **exportar** o projeto Xcode (módulo *iOS Build Support*). Compilar e instalar **exige um Mac com Xcode**. | ✅ Módulo *iOS Build Support* + **Xcode** (testado com Xcode 26.6 no macOS 26.5) |
| Controle de versão | [Git](https://git-scm.com/downloads) | Git (`xcode-select --install` ou Homebrew) |

### Aparelhos

- **Android**: Android **10 (API 29)** ou mais novo, ARM64, na [lista de aparelhos compatíveis com ARCore](https://developers.google.com/ar/devices). O *Google Play Services para RA* é instalado automaticamente pela Play Store na primeira execução.
- **iOS**: iPhone ou iPad com **iOS/iPadOS 15.0** ou mais novo e **chip A9 ou mais novo** (iPhone 6s em diante; o ARKit não roda no iPad Air 2 nem no iPad mini 4).
- **Assinatura iOS**: um Apple ID. A conta gratuita ("Personal Team") basta para instalar no seu próprio iPhone. O Apple Developer Program pago só é necessário para TestFlight ou App Store.

---

## Instalação (Windows e macOS)

### 1. Git

- **Windows**: instale o [Git for Windows](https://git-scm.com/download/win). Mantenha a opção padrão *"Checkout Windows-style, commit Unix-style line endings"*. O `.gitattributes` do repositório mantém os arquivos do Unity consistentes entre sistemas.
- **macOS**: rode `xcode-select --install` no Terminal, ou `brew install git`.

### 2. Unity Hub

Baixe em <https://unity.com/download> e entre com sua conta Unity. A licença gratuita **Unity Personal** é suficiente.

### 3. Unity 6000.4.0f1 com os módulos de build

**Opção A: pelo Unity Hub (recomendado)**

1. Abra este link no navegador para instalar a versão certa: `unityhub://6000.4.0f1/8cf496087c8f`
   (ou vá em Unity Hub → **Installs** → **Install Editor** → **Archive** → *download archive* e procure **6000.4.0f1**).
2. Na tela de módulos, marque:
   - **Android Build Support**, com os subitens **OpenJDK** e **Android SDK & NDK Tools**
   - **iOS Build Support**
   - (Windows) **Microsoft Visual Studio Community**, se ainda não tiver um editor de C#
3. Aceite a licença do Android SDK e aguarde a instalação terminar.

Se o Editor já estiver instalado sem esses módulos: Unity Hub → **Installs** → ⚙️ ao lado da 6000.4.0f1 → **Add modules**.

**Opção B: pela linha de comando (Unity CLI)**

Com o [Unity CLI](https://unity.com/download) instalado:

```bash
unity install 6000.4.0f1 --module android ios --childModules --yes --accept-eula
```

Se o Editor já estiver instalado, adicione só os módulos:

```bash
unity install-modules --editor-version 6000.4.0f1 --module android ios --childModules --accept-eula
```

O `--childModules` também instala o **OpenJDK** e o **Android SDK & NDK Tools**.

> Use o **JDK, SDK e NDK instalados pelo Unity**. Não aponte para um SDK separado do Android Studio, a menos que você saiba exatamente quais versões são necessárias.

### 4. Xcode (só no macOS, para buildar para iOS)

1. Instale o **Xcode** pela Mac App Store.
2. Abra-o uma vez, aceite a licença e deixe-o instalar os componentes adicionais.
3. Em **Xcode → Settings → Components**, confira se a plataforma **iOS** está instalada.
4. Em **Xcode → Settings → Accounts**, adicione seu Apple ID.

---

## Abrindo o projeto

```bash
git clone https://github.com/JoaoOliveira0117/ARFittingBench.git
```

> **Windows:** clone num caminho curto, **sem espaços nem acentos**, por exemplo `C:\dev\ARFittingBench`. Caminhos como `C:\Users\João\Área de Trabalho\...` costumam quebrar o build Android (Gradle).

1. Unity Hub → **Projects** → **Add** → **Add project from disk** → escolha a pasta clonada.
2. Abra com a versão **6000.4.0f1**.
3. A primeira importação demora alguns minutos, porque a pasta `Library/` (vários GB) é gerada localmente e não vai para o repositório.
4. Abra a cena **`Assets/ARFittingBench/Scenes/ARFittingBench.unity`**. Ela já é a primeira da lista de build.

---

## Testando no Editor (XR Simulation)

Dá para testar sem celular, usando o ambiente simulado do AR Foundation. Ele já está habilitado para o alvo *Standalone* (Windows/Mac).

1. Em **File → Build Profiles**, confira se a plataforma ativa é **Windows/Mac/Linux**. O XR Simulation só roda nessa plataforma.
2. Abra a cena `ARFittingBench` e aperte **Play**.
3. Mova a câmera simulada: segure o **botão direito do mouse** na Game view e use **W A S D** (mover) e **Q E** (descer/subir). O ambiente é escaneado conforme a câmera se mexe, e as superfícies aparecem em azul.
4. **Clique** numa superfície para fixar a bancada e depois clique na peça para pegá-la.
5. Para trocar o ambiente simulado: **Window → XR → AR Foundation → XR Environment**.

> Os cliques só chegam ao jogo quando a **Game view está em foco**.

---

## Build para Android

Funciona igual no **Windows e no macOS**.

### Preparando o celular

1. No celular, ative as **Opções do desenvolvedor** (Configurações → Sobre o telefone → toque 7 vezes em *Número da versão*).
2. Ative a **Depuração USB** e conecte o celular pelo cabo. Aceite o aviso "Permitir depuração USB?".
3. **Windows**: alguns fabricantes (Samsung, Motorola, Xiaomi…) exigem o driver USB do fabricante, ou o *Google USB Driver*, para o celular aparecer.

### Build

1. **File → Build Profiles** → selecione **Android** → **Switch Platform**. A primeira troca reimporta os assets e demora alguns minutos.
2. Confira se a cena `ARFittingBench` está em **Scene List**.
3. (Opcional) Rode **AR Fitting Bench → Configurar Android e iOS** na barra de menus. Ele reaplica as configurações de AR: ARCore habilitado, API mínima 29, IL2CPP, ARM64, retrato.
4. Confira **Edit → Project Settings → XR Plug-in Management → Project Validation** (aba Android). Não deve haver erros.
5. Clique em **Build And Run** para instalar direto no celular conectado, ou em **Build** para gerar um `.apk`. Salve na pasta `Builds/`, que é ignorada pelo git.

Para instalar um `.apk` já gerado à mão:

```bash
adb install -r Builds/ARFittingBench.apk
```

(O `adb` fica no SDK instalado pelo Unity: `<instalação do Unity>/Editor/Data/PlaybackEngines/AndroidPlayer/SDK/platform-tools/` no Windows, ou `/Applications/Unity/Hub/Editor/6000.4.0f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/` no macOS.)

### Configurações Android já aplicadas no projeto

| Configuração | Valor |
|---|---|
| Package name | `com.arfittingbench.app` |
| Minimum API Level | 29 (Android 10). Exigido pelo ARCore com Vulkan quando o AR é "Required". |
| Target API Level | Automático (o maior instalado) |
| Scripting Backend | IL2CPP |
| Arquiteturas | ARM64 |
| Graphics APIs | Vulkan, OpenGLES3 |
| ARCore | Habilitado, Requirement = *Required* |

---

## Build para iOS

### No macOS

1. **File → Build Profiles** → **iOS** → **Switch Platform**.
2. (Opcional) Rode **AR Fitting Bench → Configurar Android e iOS** e confira a **Project Validation** (aba iOS).
3. Clique em **Build** e escolha uma pasta vazia, como `Builds/iOS`. O Unity gera um **projeto Xcode**.
4. Abra `Builds/iOS/Unity-iPhone.xcodeproj` no Xcode.
5. Selecione o target **Unity-iPhone** → **Signing & Capabilities**:
   - marque **Automatically manage signing**;
   - em **Team**, escolha seu Apple ID (Personal Team serve);
   - se o Xcode disser que o bundle identifier já está em uso, troque `com.arfittingbench.app` por algo único, como `com.seunome.arfittingbench`.
6. Conecte o iPhone, escolha-o como destino no topo do Xcode e clique em **▶ Run**.
7. No iPhone (só na primeira vez):
   - iOS 16+: **Ajustes → Privacidade e Segurança → Modo de Desenvolvedor** → ative e reinicie;
   - **Ajustes → Geral → VPN e Gerenciamento de Dispositivos** → confie no seu certificado de desenvolvedor.

> Para não precisar trocar o bundle ID no Xcode a cada build, troque no Unity: **Edit → Project Settings → Player → iOS → Other Settings → Bundle Identifier**.

Verificação pela linha de comando (sem assinatura), útil para confirmar que o projeto compila:

```bash
cd Builds/iOS
xcodebuild -project Unity-iPhone.xcodeproj -scheme Unity-iPhone -configuration Release \
  -sdk iphoneos -destination 'generic/platform=iOS' \
  CODE_SIGNING_ALLOWED=NO build
```

### No Windows

O Windows **não consegue compilar nem assinar** apps iOS, porque isso exige o Xcode, que só roda no macOS. O que dá para fazer:

1. Instale o módulo **iOS Build Support** e gere o projeto Xcode (**Build Profiles → iOS → Switch Platform → Build**).
2. Copie a pasta gerada para um Mac e siga os passos 4 a 7 da seção do macOS.
   - Prefira copiar via **git**, **AirDrop** ou pasta de rede. Se usar um `.zip` criado no Windows, rode no Mac antes de buildar:
     ```bash
     find Builds/iOS -name "*.sh" -exec chmod +x {} +
     ```
3. Sem Mac disponível: o **Unity Build Automation** (build na nuvem da Unity) compila iOS a partir deste repositório. Ele exige configurar os certificados de assinatura no Unity Cloud.

### Configurações iOS já aplicadas no projeto

| Configuração | Valor |
|---|---|
| Bundle Identifier | `com.arfittingbench.app` |
| Target minimum iOS Version | 15.0 |
| Camera Usage Description | "A câmera é usada para a realidade aumentada." |
| ARKit | Habilitado, Requirement = *Required* |

---

## Estrutura do projeto

```
Assets/ARFittingBench/
├── Scenes/ARFittingBench.unity     Cena principal (AR Session, XR Origin, interface)
├── Scripts/
│   ├── BancadaPlacer.cs            Detecção de plano + toque para ancorar a bancada
│   ├── Bancada.cs                  Sorteio, cor por proximidade, tolerâncias, rodadas
│   ├── MovimentoPeca.cs            Script único de movimentação da peça (lê ControleConfig)
│   ├── ControleConfig.cs           Config.: distância à frente do celular, multiplicador, origem
│   ├── InterfaceBancada.cs         Instruções, botões dos modos, dados da rodada
│   ├── RegistroResultados.cs       Grava os resultados em CSV
│   └── Toque.cs                    Detecção de toque (dedo no celular, mouse no Editor)
├── Editor/BancadaSetup.cs          Menu "AR Fitting Bench": recria cena/prefabs/configurações
├── Config/                         Os 3 modos de controle (ScriptableObjects)
├── Prefabs/                        Bancada (mesa + peça + encaixe) e SuperficieDetectada
└── Materials/
Assets/XR/                          Configurações do XR Plug-in Management, ARCore e ARKit
Assets/Settings/                    Configurações do URP (Mobile_RPAsset é usado nos celulares)
```

### Menu "AR Fitting Bench" (barra de menus do Unity)

| Item | O que faz |
|---|---|
| **Criar Cena Completa** | Recria do zero materiais, configs dos modos, prefabs e a cena, e aplica as configurações de plataforma. Sobrescreve a cena e os prefabs. |
| **Configurar Android e iOS** | Aplica só as configurações de player e XR: nome, bundle ID, ARCore/ARKit habilitados, API 29 no Android, IL2CPP/ARM64, retrato. |

---

## Configuração

### Modos de controle

Cada modo é um asset em `Assets/ARFittingBench/Config/` com três valores:

| Campo | Significado |
|---|---|
| `distanciaFrente` | Distância (m) à frente do celular em que a peça fica enquanto é segurada |
| `multiplicador` | Multiplicador aplicado ao movimento (deslocamento do celular ou arraste na tela) |
| `origem` | `Celular` (segue o aparelho) ou `Tela` (gestos de toque) |

Para criar um modo: clique com o botão direito no Project → **Create → AR Fitting Bench → Configuração de Controle**. Depois adicione-o à lista `modos` e um botão em `botoesModo` no objeto **Jogo** (componente `InterfaceBancada`).

### Tolerâncias e rodada

No prefab `Assets/ARFittingBench/Prefabs/Bancada.prefab` (componente `Bancada`): `toleranciaPosicao` (m), `toleranciaAngulo` (°), tamanho da área, distância mínima entre peça e encaixe, cores e a pausa entre rodadas.

### Resultados (CSV)

Cada rodada acrescenta uma linha a `resultados_bancada.csv` com: data/hora, modo, origem, multiplicador, distância, rodada, tempo (s), erro de posição (cm) e erro de ângulo (°).

| Onde roda | Local do arquivo |
|---|---|
| Android | `/sdcard/Android/data/com.arfittingbench.app/files/` (use `adb pull` nesse caminho) |
| iOS | Contêiner do app. No Xcode: **Window → Devices and Simulators** → app → ⚙️ → **Download Container** |
| Editor (Windows) | `%USERPROFILE%\AppData\LocalLow\ARFittingBench\AR Fitting Bench\` |
| Editor (macOS) | `~/Library/Application Support/ARFittingBench/AR Fitting Bench/` |

```bash
adb pull /sdcard/Android/data/com.arfittingbench.app/files/resultados_bancada.csv
```

---

## Problemas comuns

### Ao abrir o projeto

| Problema | Solução |
|---|---|
| O Hub pede para **atualizar/trocar a versão** | Instale e use exatamente a **6000.4.0f1** (veja [Instalação](#3-unity-600040f1-com-os-módulos-de-build)). Não aceite o upgrade. |
| O Editor abre em **Safe Mode** | Há erros de compilação. Veja o Console. Em geral é um pacote que não baixou: confira a internet e use **Window → Package Manager → ⟳**. |
| Erro ao resolver `com.unity.pipeline` (`-exp`) ou `com.unity.ai.assistant` (`-pre`) | São ferramentas **opcionais** do Editor (automação/IA) e não são usadas pelo app. Remova as linhas do `Packages/manifest.json` e reabra o projeto. |
| Materiais **rosa/magenta** | O URP não está atribuído. **Edit → Project Settings → Graphics** deve usar `PC_RPAsset`, e **Quality** deve usar `Mobile_RPAsset` no Android/iOS. |
| Hub mostra "*No valid Unity Editor license*" | Unity Hub → ⚙️ **Preferences → Licenses → Add** → *Get a free personal license*. |
| Primeira importação muito lenta | É normal: a `Library/` é gerada localmente. Não copie a `Library/` entre Windows e macOS. Apague-a se o projeto se comportar de forma estranha depois de trocar de máquina. |

### Build Android

| Problema | Solução |
|---|---|
| "*Android SDK/JDK/NDK not found*" | Instale os módulos pelo Unity Hub (Android Build Support + OpenJDK + SDK & NDK Tools). Em **Preferences → External Tools**, deixe marcadas as opções *"…installed with Unity"*. |
| **Gradle build failed** | 1) Caminho do projeto com espaços/acentos ou longo demais (Windows): mova para `C:\dev\ARFittingBench`. 2) Apague `Library/Bee` e tente de novo. 3) Leia o primeiro `error:` do Console, não o último. |
| `Filename too long` / `MAX_PATH` (Windows) | Use um caminho curto, ou habilite caminhos longos: `git config --system core.longpaths true` e a política *LongPathsEnabled* do Windows. |
| Erro de validação "*ARCore requires … API Level 29*" | Rode **AR Fitting Bench → Configurar Android e iOS**, ou ajuste **Player → Android → Minimum API Level = 29**. |
| O celular não aparece em *Run Device* | Confira a depuração USB, aceite o aviso RSA no celular, troque o cabo/porta, instale o driver USB do fabricante (Windows). Teste com `adb devices`. |
| `INSTALL_FAILED_UPDATE_INCOMPATIBLE` | Já existe uma versão instalada assinada com outra chave. Desinstale-a do celular (`adb uninstall com.arfittingbench.app`). |
| O app diz "*This device does not support AR*" / pede *Google Play Services para RA* | O aparelho não é certificado para ARCore, ou o serviço precisa ser instalado/atualizado pela Play Store. |
| **Tela preta** (sem câmera) | Permita o acesso à câmera (Configurações → Apps → AR Fitting Bench → Permissões). Confira se o nível de Quality ativo no Android é **Mobile** (o renderer dele tem a *AR Background Renderer Feature*). |

### Build iOS

| Problema | Solução |
|---|---|
| "*Signing for 'Unity-iPhone' requires a development team*" | Signing & Capabilities → escolha um **Team**. |
| "*Failed to register bundle identifier*" / "*No profiles for … were found*" | O bundle ID já é de outra pessoa. Troque por um único (`com.seunome.arfittingbench`). |
| "*iOS X.Y is not installed*" / aparelho "*not available*" | Xcode → Settings → Components → instale a plataforma iOS da versão do iPhone. Atualize o Xcode se o iPhone for mais novo. |
| "*Desenvolvedor não confiável*" ao abrir o app | iPhone: Ajustes → Geral → VPN e Gerenciamento de Dispositivos → confie no certificado. |
| "*Developer Mode disabled*" | iPhone (iOS 16+): Ajustes → Privacidade e Segurança → Modo de Desenvolvedor → ative e reinicie. |
| `Permission denied` num *Run Script* do build (`process_symbols.sh`, `process_symbols_il2cpp.sh`) | O projeto veio do Windows e perdeu a permissão de execução. No Mac: `chmod +x` nesses `.sh` (veja [iOS no Windows](#no-windows)). |
| Conta gratuita: o app para de abrir depois de 7 dias | Os perfis da Personal Team expiram em 7 dias. Rode pelo Xcode de novo. |
| O app fecha assim que abre (câmera) | A permissão de câmera foi negada. Ajustes → AR Fitting Bench → Câmera. |

### Durante o uso

| Problema | Solução |
|---|---|
| Nenhuma superfície é detectada | Boa iluminação, superfícies com textura (evite mesa branca lisa ou vidro), mova o celular devagar por alguns segundos. |
| A peça "pula" ao ser pega | Esperado nos modos *Celular*: ela vai para `distanciaFrente` à frente do celular. Diminua esse valor no asset do modo. |
| A bancada ficou num lugar ruim | Toque em **Reposicionar bancada** para escolher a superfície de novo. |
| Os cliques não funcionam no Editor | Clique dentro da **Game** view para dar foco. Teste na plataforma *Windows/Mac/Linux* (XR Simulation). |
