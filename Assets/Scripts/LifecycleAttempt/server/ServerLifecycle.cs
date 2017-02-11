using System;
using System.Collections.Generic;

namespace ServerLifecycle {
	public enum ProcessState {AwaitingData, AwaitingPlayers, AwaitingMesh, PreparingGame, PlayingGame };

	public enum Command {EnoughPlayersJoined, TooFewPlayersRemaining, MeshReceived, GameReady, GameEnd };

	public class Process
	{
		public class StateTransition
		{
			readonly ProcessState CurrentState;
			readonly Command Command;

			public StateTransition(ProcessState currentState, Command command)
			{
				CurrentState = currentState;
				Command = command;
			}

			public override int GetHashCode()
			{
				return 17 + 31 * CurrentState.GetHashCode() + 31 * Command.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				StateTransition other = obj as StateTransition;
				return other != null && this.CurrentState == other.CurrentState && this.Command == other.Command;
			}
		}

		Dictionary<StateTransition, ProcessState> transitions;
		public ProcessState CurrentState { get; private set; }

		public Process()
		{
			CurrentState = ProcessState.AwaitingData;
			transitions = new Dictionary<StateTransition, ProcessState>
			{
				{ new StateTransition(ProcessState.AwaitingData, Command.EnoughPlayersJoined), ProcessState.AwaitingMesh },
				{ new StateTransition(ProcessState.AwaitingData, Command.MeshReceived), ProcessState.AwaitingPlayers },
				{ new StateTransition(ProcessState.AwaitingMesh, Command.MeshReceived), ProcessState.PreparingGame },
				{ new StateTransition(ProcessState.AwaitingPlayers, Command.EnoughPlayersJoined), ProcessState.PreparingGame },
				{ new StateTransition(ProcessState.PreparingGame, Command.GameReady), ProcessState.PlayingGame },

				{ new StateTransition(ProcessState.PlayingGame, Command.TooFewPlayersRemaining), ProcessState.AwaitingPlayers },
				{ new StateTransition(ProcessState.PreparingGame, Command.TooFewPlayersRemaining), ProcessState.AwaitingPlayers },
				{ new StateTransition(ProcessState.AwaitingMesh, Command.TooFewPlayersRemaining), ProcessState.AwaitingData },

				{ new StateTransition(ProcessState.PlayingGame, Command.GameEnd), ProcessState.AwaitingPlayers }
			};
		}

		public ProcessState GetNext(Command command)
		{
			StateTransition transition = new StateTransition(CurrentState, command);
			ProcessState nextState;
			if (!transitions.TryGetValue(transition, out nextState))
				throw new Exception("Invalid transition: " + CurrentState + " -> " + command);
			return nextState;
		}

		public ProcessState MoveNext(Command command)
		{
			CurrentState = GetNext(command);
			return CurrentState;
		}
	}

}