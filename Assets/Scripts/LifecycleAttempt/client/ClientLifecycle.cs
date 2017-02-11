using System;
using System.Collections.Generic;

namespace ClientLifecycle {
	public enum ProcessState {Idle, Searching, Connecting, MiniGame, PlayingGame, SpectatingGame, Leaderboard};

	public enum Command {JoinGame, ConnectGame, LeaveGame, JoinedGame, GameReady, GameEnd, PlayMinigame, ConnectionTimeout};

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
			CurrentState = ProcessState.Idle;
			transitions = new Dictionary<StateTransition, ProcessState>
			{
				{ new StateTransition(ProcessState.Idle, Command.JoinGame), ProcessState.Searching },
				{ new StateTransition(ProcessState.Searching, Command.ConnectGame), ProcessState.Connecting },
				{ new StateTransition(ProcessState.Connecting, Command.JoinedGame), ProcessState.MiniGame },
				{ new StateTransition(ProcessState.MiniGame, Command.GameReady), ProcessState.PlayingGame },
				{ new StateTransition(ProcessState.PlayingGame, Command.GameEnd), ProcessState.Leaderboard },
				{ new StateTransition(ProcessState.Leaderboard, Command.PlayMinigame), ProcessState.MiniGame },

				{ new StateTransition(ProcessState.Searching, Command.LeaveGame), ProcessState.Idle },
				{ new StateTransition(ProcessState.Connecting, Command.LeaveGame), ProcessState.Idle },
				{ new StateTransition(ProcessState.MiniGame, Command.LeaveGame), ProcessState.Idle },
				{ new StateTransition(ProcessState.PlayingGame, Command.LeaveGame), ProcessState.Idle },
				{ new StateTransition(ProcessState.Leaderboard, Command.LeaveGame), ProcessState.Idle },

				{ new StateTransition(ProcessState.MiniGame, Command.ConnectionTimeout), ProcessState.Searching },
				{ new StateTransition(ProcessState.PlayingGame, Command.ConnectionTimeout), ProcessState.Searching },
				{ new StateTransition(ProcessState.Leaderboard, Command.ConnectionTimeout), ProcessState.Searching },
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