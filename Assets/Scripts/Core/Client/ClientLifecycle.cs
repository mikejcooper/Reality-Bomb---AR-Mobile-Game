using System;
using System.Collections.Generic;

namespace ClientLifecycle {
	public enum ProcessState {Idle, Searching, Connecting, Sandbox, PlayingGame, SpectatingGame, Leaderboard};

	public enum Command {JoinGame, ConnectGame, LeaveGame, JoinedGame, GameReady, GameEnd, PlaySandbox, ConnectionTimeout};

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
				{ new StateTransition(ProcessState.Connecting, Command.JoinedGame), ProcessState.Sandbox },
				{ new StateTransition(ProcessState.Sandbox, Command.GameReady), ProcessState.PlayingGame },
				{ new StateTransition(ProcessState.PlayingGame, Command.GameEnd), ProcessState.Leaderboard },
				{ new StateTransition(ProcessState.Leaderboard, Command.PlaySandbox), ProcessState.Sandbox },

				{ new StateTransition(ProcessState.Sandbox, Command.GameEnd), ProcessState.Leaderboard },

				{ new StateTransition(ProcessState.Leaderboard, Command.JoinedGame), ProcessState.Leaderboard },
				{ new StateTransition(ProcessState.Leaderboard, Command.GameReady), ProcessState.PlayingGame },

				{ new StateTransition(ProcessState.Searching, Command.LeaveGame), ProcessState.Idle },
				{ new StateTransition(ProcessState.Connecting, Command.LeaveGame), ProcessState.Idle },
				{ new StateTransition(ProcessState.Sandbox, Command.LeaveGame), ProcessState.Idle },
				{ new StateTransition(ProcessState.PlayingGame, Command.LeaveGame), ProcessState.Idle },
				{ new StateTransition(ProcessState.Leaderboard, Command.LeaveGame), ProcessState.Idle },

				{ new StateTransition(ProcessState.Sandbox, Command.ConnectionTimeout), ProcessState.Searching },
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
			ProcessState lastState = CurrentState;
			CurrentState = GetNext(command);
			UnityEngine.Debug.Log (string.Format("Succesfully moved: {0} -({1})-> {2}", lastState, command, CurrentState));
			return CurrentState;
		}
	}

}