using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace ARFittingBench
{
    /// <summary>
    /// Detecta toques curtos (tap) na tela, ignorando arrastes e toques sobre a interface.
    /// O resultado é calculado uma vez por frame e pode ser consumido por quem o tratar.
    /// </summary>
    public static class Toque
    {
        const float MovimentoMaximoTap = 30f;
        const float DuracaoMaximaTap = 0.4f;

        static readonly List<RaycastResult> resultadosUI = new List<RaycastResult>();

        static int ultimoFrame = -1;
        static Vector2 posicaoInicio;
        static float tempoInicio;
        static bool podeSerTap;
        static bool tapNesteFrame;
        static Vector2 posicaoTap;

        public static bool Tap(out Vector2 posicao)
        {
            Atualizar();
            posicao = posicaoTap;
            return tapNesteFrame;
        }

        public static void Consumir()
        {
            Atualizar();
            tapNesteFrame = false;
        }

        static void Atualizar()
        {
            if (Time.frameCount == ultimoFrame)
                return;

            ultimoFrame = Time.frameCount;
            tapNesteFrame = false;

            // Pointer cobre o toque principal no celular e o mouse no Editor.
            var ponteiro = Pointer.current;
            if (ponteiro == null)
                return;

            Vector2 posicao = ponteiro.position.ReadValue();

            if (ponteiro.press.wasPressedThisFrame)
            {
                posicaoInicio = posicao;
                tempoInicio = Time.unscaledTime;
                podeSerTap = !SobreInterface(posicao);
            }

            if (ponteiro.press.isPressed && (posicao - posicaoInicio).sqrMagnitude > MovimentoMaximoTap * MovimentoMaximoTap)
                podeSerTap = false;

            if (ponteiro.press.wasReleasedThisFrame && podeSerTap && Time.unscaledTime - tempoInicio <= DuracaoMaximaTap)
            {
                tapNesteFrame = true;
                posicaoTap = posicao;
            }
        }

        static bool SobreInterface(Vector2 posicao)
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null)
                return false;

            resultadosUI.Clear();
            eventSystem.RaycastAll(new PointerEventData(eventSystem) { position = posicao }, resultadosUI);
            return resultadosUI.Count > 0;
        }
    }
}
