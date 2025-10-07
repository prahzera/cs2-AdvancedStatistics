using System.Collections.Concurrent;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;
using System;

namespace AdvancedStatistics.Utils
{
    public class MainThreadDispatcher
    {
        private static readonly ConcurrentQueue<Action> _actionQueue = new();
        private static ILogger? _logger;

        public static void SetLogger(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Añade una acción a la cola para ser ejecutada en el hilo principal
        /// </summary>
        /// <param name="action">La acción a ejecutar</param>
        public static void Enqueue(Action action)
        {
            _actionQueue.Enqueue(action);
        }

        /// <summary>
        /// Procesa todas las acciones en cola en el hilo principal
        /// Este método debe ser llamado periódicamente desde el hilo principal
        /// </summary>
        public static void ProcessQueue()
        {
            try
            {
                while (_actionQueue.TryDequeue(out var action))
                {
                    try
                    {
                        action?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError($"Error executing action in main thread: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error processing main thread queue: {ex.Message}");
            }
        }
    }
}