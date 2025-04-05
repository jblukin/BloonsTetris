using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

#if UNITY_EDITOR
using UnityEditor;

#endif

#if UNITY_EDITOR
[InitializeOnLoad]
#endif

//Original Code found here (https://discussions.unity.com/t/implement-a-mouse-drag-composite/772121/10)

public class MouseDragInteraction : IInputInteraction
{
    static MouseDragInteraction()
    {
        InputSystem.RegisterInteraction<MouseDragInteraction>();
    }

    public void Reset()
    {
    }

    public void Process( ref InputInteractionContext context )
    {
        if ( context.timerHasExpired )
        {
            context.Performed();
            return;
        }

        var phase = context.phase;

        switch ( phase )
        {
            case InputActionPhase.Disabled:
                break;
            case InputActionPhase.Waiting:
                if ( context.ControlIsActuated() )
                {
                    context.Started();
                    context.SetTimeout( float.PositiveInfinity );
                }

                break;
            case InputActionPhase.Started:
                context.PerformedAndStayPerformed();
                break;
            case InputActionPhase.Performed:
                if ( context.ControlIsActuated() )
                {
                    context.PerformedAndStayPerformed();
                }
                else if ( !( (ButtonControl)context.action.controls[ 0 ] ).isPressed )
                {
                    context.Canceled();
                }

                break;
            case InputActionPhase.Canceled:
                break;
            default:
                throw new ArgumentOutOfRangeException( nameof( phase ), phase, null );
        }
    }

    [RuntimeInitializeOnLoadMethod]
    private static void Init()
    {
    }
}