// -----------------------------------------------------------------------------
// Vena Framework
// Unity game framework layer built on Vena Core and Vena World.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
// ReSharper disable All

namespace Vena.Framework
{
    /// <summary>
    ///  As a game manager
    ///  1 game entry
    ///  2 Manage game state machine, such as GameWorld and GameMode
    ///  3 Manage game service modules and inject them into game service modules by means of inversion of control
    /// </summary>
    public partial class GameWorld : BasedStackGameMachine
    {
        private static readonly GameWorld _game = new GameWorld();

        public static GameLevel level => _game.current as GameLevel;
        
        public static GameMode mode => level.mode;
        
        /// <summary>
        /// launch game with description and entry level
        /// </summary>
        /// <param name="description"></param>
        /// <param name="entryLevel"></param>
        public static async void Launch(GameDescription description, GameLevel entryLevel)
        {
            try
            {
                _UIRoot = description.UIRoot;
                _UILoader = description.UILoader;
                _SceneLoader = description.SceneLoader;

                List<Type> commandTypes = new List<Type>();
                List<Type> serviceTypes = new List<Type>();
                List<Type> moduleTypes = new List<Type>();
                List<Type> stateTypes = new List<Type>();
                using (var _ = new TimeWatch("collect modules、commands、services"))
                {
                    foreach (var assembly in description.Assemblies)
                    {
                        foreach (var type in assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract))
                        {
                            if (type.IsSubclassOf(typeof(ICommand)))
                            {
                                commandTypes.Add(type);
                            }
                            else if (type.IsSubclassOf(typeof(Service)))
                            {
                                serviceTypes.Add(type);
                            }
                            else if (typeof(IModule).IsAssignableFrom(type))
                            {
                                moduleTypes.Add(type);
                            }
                            else if (type.IsSubclassOf(typeof(BasedStackGameState)))
                            {
                                stateTypes.Add(type);
                            }
                        }
                    }
                }

                // init game commands
                InitializeCommands(commandTypes.ToArray());

                // init game service
                InitializeServices(serviceTypes.ToArray());

                // init game module
                InitializeModules(stateTypes.ToArray(), moduleTypes.ToArray());

                // init default world
                await _game.Enter(entryLevel, default);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GameWorld launch failed: {ex}");
            }
        }

        public static void Tick(float time, float deltaTime)
        {
            (_game as IBasedStackGameState).StateUpdate(time, deltaTime);
        }

        public static void Quit()
        {
            // shutdown game service
            foreach (var service in _services.Values.ToArray())
            {
                ((IService)service).Shutdown();
            }

            _services.Clear();
        }

        public static async void EnterWorld(GameLevel gameWorld, IGameTransition transition = default)
        {
            await _game.Enter(gameWorld, transition);
        }

        public static async void ExitWorld(IGameTransition transition)
        {
            await _game.Exit(transition);
        }

        public static async void EnterMode(GameMode gameMode, IGameTransition transition = default)
        {
            await level.Enter(gameMode, transition);
        }

        public static async void ExitMode(IGameTransition transition = default)
        {
            await level.Exit(transition);
        }
    }
}